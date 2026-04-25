#nullable enable
using NCalc.Exceptions;
using NCalc.Parser;
using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Evaluations")]
public class EvaluationTests
{
    [Test]
    [MethodDataSource(typeof(EvaluationTestData), "GetEnumerator")]
    public async Task Expression_Should_Evaluate(string expression, object expected)
    {
        await Assert.That(new Expression(expression).Evaluate(CancellationToken.None)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(typeof(ValuesTestData), "GetEnumerator")]
    public async Task ShouldParseValues(string input, object expectedValue)
    {
        var expression = new Expression(input);

        var result = expression.Evaluate(CancellationToken.None);

        if (expectedValue is double expectedDouble)
        {
            // TODO: TUnit migration - xUnit Assert.Equal had additional argument(s) (precision: 15) that could not be converted.
            await Assert.That((double)result!).IsEqualTo(expectedDouble);
        }
        else
        {
            await Assert.That(result).IsEqualTo(expectedValue);
        }
    }

    [Test]
    public async Task ShouldEvaluateInFunction()
    {
        // The last argument should not be evaluated
        var ein = new Expression("in((2 + 2), [1], [2], 1 + 2, 4, 1 / 0)");
        ein.Parameters["1"] = 2;
        ein.Parameters["2"] = 5;

        await Assert.That(ein.Evaluate(CancellationToken.None)).IsEqualTo(true);

        var eout = new Expression("in((2 + 2), [1], [2], 1 + 2, 3)");
        eout.Parameters["1"] = 2;
        eout.Parameters["2"] = 5;

        await Assert.That(eout.Evaluate(CancellationToken.None)).IsEqualTo(false);

        // Should work with strings
        var estring = new Expression("in('to' + 'to', 'titi', 'toto')");

        await Assert.That(estring.Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateTernaryExpression()
    {
        await Assert.That(new Expression("1+2<3 ? 3+4 : 1").Evaluate(CancellationToken.None)).IsEqualTo(1);
    }

    [Test]
    public async Task Should_Not_Throw_Function_Not_Found_Issue_110()
    {
        const string expressionStr = "IN([acp_associated_person_transactions], 'T', 'Z', 'A')";
        var expression = new Expression(expressionStr)
        {
            Options = ExpressionOptions.RoundAwayFromZero | ExpressionOptions.IgnoreCaseAtBuiltInFunctions,
            Parameters =
            {
                ["acp_associated_person_transactions"] = 'T'
            }
        };

        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task Should_Evaluate_Ifs()
    {
        // Test first case true, return next value
        var eifs = new Expression("ifs([divider] != 0, [divided] / [divider], -1)");
        eifs.Parameters["divider"] = 5;
        eifs.Parameters["divided"] = 5;

        await Assert.That(eifs.Evaluate(CancellationToken.None)).IsEqualTo(1d);

        // Test first case false, no next case, return default value
        eifs = new Expression("ifs([divider] != 0, [divided] / [divider], -1)");
        eifs.Parameters["divider"] = 0;
        eifs.Parameters["divided"] = 5;

        await Assert.That(eifs.Evaluate(CancellationToken.None)).IsEqualTo(-1);

        // Test first case false, next case true, return next value (eg 4th expr)

        eifs = new Expression("ifs([number] == 3, 5, [number] == 5, 3, 8)");
        eifs.Parameters["number"] = 5;
        await Assert.That(eifs.Evaluate(CancellationToken.None)).IsEqualTo(3);

        // Test first case false, next case false, return default value (eg 5th expr)

        eifs = new Expression("ifs([number] == 3, 5, [number] == 5, 3, 8)");
        eifs.Parameters["number"] = 1337;

        await Assert.That(eifs.Evaluate(CancellationToken.None)).IsEqualTo(8);
    }

    [Test]
    public async Task ShouldEvaluateConditional()
    {
        var eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
        eif.Parameters["divider"] = 5;
        eif.Parameters["divided"] = 5;

        await Assert.That(eif.Evaluate(CancellationToken.None)).IsEqualTo(1d);

        eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
        eif.Parameters["divider"] = 0;
        eif.Parameters["divided"] = 5;
        await Assert.That(eif.Evaluate(CancellationToken.None)).IsEqualTo(0);
    }

    [Test]
    public async Task ShouldHandleCaseSensitiveness()
    {
        await Assert.That(new Expression("aBs(-1)", ExpressionOptions.DecimalAsDefault | ExpressionOptions.IgnoreCaseAtBuiltInFunctions).Evaluate(CancellationToken.None)).IsEqualTo(1M);
        await Assert.That(new Expression("Abs(-1)", ExpressionOptions.DecimalAsDefault).Evaluate(CancellationToken.None)).IsEqualTo(1M);

        Assert.Throws<NCalcFunctionNotFoundException>(() => new Expression("aBs(-1)").Evaluate(CancellationToken.None));
    }

    [Test]
    public async Task ShouldCompareDates()
    {
        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        await Assert.That(new Expression($"#1{dateSeparator}1{dateSeparator}2009#==#1{dateSeparator}1{dateSeparator}2009#")
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
        await Assert.That(new Expression($"#2{dateSeparator}1{dateSeparator}2009#==#1{dateSeparator}1{dateSeparator}2009#")
            .Evaluate(CancellationToken.None)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldRoundAwayFromZero()
    {
        await Assert.That(new Expression("Round(22.5, 0)")
            .Evaluate(CancellationToken.None)).IsEqualTo(22d);
        await Assert.That(new Expression("Round(22.5, 0)", ExpressionOptions.RoundAwayFromZero)
            .Evaluate(CancellationToken.None)).IsEqualTo(23d);
    }

    [Test]
    public async Task ShouldEvaluateSubExpressions()
    {
        var volume = new Expression("[surface] * h");
        var surface = new Expression("[l] * [L]");
        volume.Parameters["surface"] = surface;
        volume.Parameters["h"] = 3;
        surface.Parameters["l"] = 1;
        surface.Parameters["L"] = 2;

        await Assert.That(volume.Evaluate(CancellationToken.None)).IsEqualTo(6);
    }

    [Test]
    [Arguments("Round(1.412;2)", 1.41)]
    [Arguments("Max(5.1;10.2)", 10.2)]
    [Arguments("Min(1.3;2)", 1.3)]
    [Arguments("Pow(5;2)", 25d)]
    public async Task ShouldAllowSemicolonAsArgumentSeparator(string expression, object expected)
    {
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        var logicalExpression = LogicalExpressionParser.Parse(context);

        await Assert.That(new Expression(logicalExpression)
            .Evaluate(CancellationToken.None)).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldAllowToUseCurlyBraces()
    {
        var volume = new Expression("{surface} * h");
        var surface = new Expression("{l} * {L}");
        volume.Parameters["surface"] = surface;
        volume.Parameters["h"] = 3;
        surface.Parameters["l"] = 1;
        surface.Parameters["L"] = 2;

        await Assert.That(volume.Evaluate(CancellationToken.None)).IsEqualTo(6);
    }

    [Test]
    [MethodDataSource(typeof(NullCheckTestData), "GetEnumerator")]
    public async Task ShouldAllowOperatorsWithNulls(string expression, object expected)
    {
        var e = new Expression(expression, ExpressionOptions.AllowNullParameter);
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldEvaluateArrayParameters()
    {
        var e = new Expression("x * x", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0, 1, 2, 3, 4 }
            }
        };

        var result = (IList<object>?)e.Evaluate(CancellationToken.None);

        await Assert.That(result).IsNotNull();
        await Assert.That(result[0]).IsEqualTo(0);
        await Assert.That(result[1]).IsEqualTo(1);
        await Assert.That(result[2]).IsEqualTo(4);
        await Assert.That(result[3]).IsEqualTo(9);
        await Assert.That(result[4]).IsEqualTo(16);
    }

    [Test]
    public async Task ShouldEvaluateArrayParametersWithFunctions()
    {
        var e = new Expression("Round(x, 2)", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0.51, 1.671, 2.237, 3.568, 4.11 }
            }
        };

        var result = (IList<object>?)e.Evaluate(CancellationToken.None);

        await Assert.That(result).IsNotNull();
        await Assert.That(result[0]).IsEqualTo(0.51);
        await Assert.That(result[1]).IsEqualTo(1.67);
        await Assert.That(result[2]).IsEqualTo(2.24);
        await Assert.That(result[3]).IsEqualTo(3.57);
        await Assert.That(result[4]).IsEqualTo(4.11);
    }

    [Test]
    public async Task AllowNullOrEmptyExpressions()
    {
        await Assert.That(new Expression("", ExpressionOptions.AllowNullOrEmptyExpressions)
            .Evaluate(CancellationToken.None)).IsEqualTo("");
        await Assert.That(new Expression((string?)null, ExpressionOptions.AllowNullOrEmptyExpressions)
            .Evaluate(CancellationToken.None)).IsNull();
    }
    [Test]
    [Arguments("01 == ''")]
    [Arguments("' ' == 01")]
    [Arguments("\" \" == 01")]
    [Arguments("\"dog\" == 01")]
    public async Task ShouldUseStrictTypeMatching(string expression)
    {
        await Assert.That(new Expression(expression, ExpressionOptions.StrictTypeMatching)
            .Evaluate(CancellationToken.None) as bool?).IsFalse();
    }

    [Test]
    public async Task SpaceCharacterComparisonShouldBeTrue()
    {
        var expr = new Expression("[Test] == ' '")
        {
            Parameters =
            {
                ["Test"] = ' '
            }
        };
        var result = expr.Evaluate(CancellationToken.None);
        await Assert.That(result is true).IsTrue();
    }
}