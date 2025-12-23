#nullable enable
using NCalc.Exceptions;
using NCalc.Parser;
using NCalc.Tests.TestData;

namespace NCalc.Tests;

[Property("Category", "Evaluations")]
public class EvaluationTests
{
    [Test]
    [MethodDataSource<EvaluationTestData>(nameof(EvaluationTestData.GetTestData))]
    public async Task Expression_Should_Evaluate(string expression, object expected, CancellationToken cancellationToken)
    {
        await Assert.That(new Expression(expression).Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource<ValuesTestData>(nameof(ValuesTestData.GetTestData))]
    public async Task ShouldParseValues(string input, object expectedValue, CancellationToken cancellationToken)
    {
        var expression = new Expression(input);

        var result = expression.Evaluate(cancellationToken);

        if (expectedValue is double expectedDouble)
        {
            await Assert.That((double)result!).IsEqualTo(expectedDouble).Within(15);
        }
        else
        {
            await Assert.That(result).IsEqualTo(expectedValue);
        }
    }

    [Test]
    public async Task ShouldEvaluateInFunction(CancellationToken cancellationToken)
    {
        // The last argument should not be evaluated
        var ein = new Expression("in((2 + 2), [1], [2], 1 + 2, 4, 1 / 0)");
        ein.Parameters["1"] = 2;
        ein.Parameters["2"] = 5;

        await Assert.That(ein.Evaluate(cancellationToken)).IsEqualTo(true);

        var eout = new Expression("in((2 + 2), [1], [2], 1 + 2, 3)");
        eout.Parameters["1"] = 2;
        eout.Parameters["2"] = 5;

        await Assert.That(eout.Evaluate(cancellationToken)).IsEqualTo(false);

        // Should work with strings
        var estring = new Expression("in('to' + 'to', 'titi', 'toto')");

        await Assert.That(estring.Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateTernaryExpression(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("1+2<3 ? 3+4 : 1").Evaluate(cancellationToken)).IsEqualTo(1);
    }

    [Test]
    public async Task Should_Not_Throw_Function_Not_Found_Issue_110(CancellationToken cancellationToken)
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

        await Assert.That(expression.Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task Should_Evaluate_Ifs(CancellationToken cancellationToken)
    {
        // Test first case true, return next value
        var eifs = new Expression("ifs([divider] != 0, [divided] / [divider], -1)");
        eifs.Parameters["divider"] = 5;
        eifs.Parameters["divided"] = 5;

        await Assert.That(eifs.Evaluate(cancellationToken)).IsEqualTo(1d);

        // Test first case false, no next case, return default value
        eifs = new Expression("ifs([divider] != 0, [divided] / [divider], -1)");
        eifs.Parameters["divider"] = 0;
        eifs.Parameters["divided"] = 5;

        await Assert.That(eifs.Evaluate(cancellationToken)).IsEqualTo(-1);

        // Test first case false, next case true, return next value (eg 4th expr)

        eifs = new Expression("ifs([number] == 3, 5, [number] == 5, 3, 8)");
        eifs.Parameters["number"] = 5;
        await Assert.That(eifs.Evaluate(cancellationToken)).IsEqualTo(3);

        // Test first case false, next case false, return default value (eg 5th expr)

        eifs = new Expression("ifs([number] == 3, 5, [number] == 5, 3, 8)");
        eifs.Parameters["number"] = 1337;

        await Assert.That(eifs.Evaluate(cancellationToken)).IsEqualTo(8);
    }

    [Test]
    public async Task ShouldEvaluateConditional(CancellationToken cancellationToken)
    {
        var eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
        eif.Parameters["divider"] = 5;
        eif.Parameters["divided"] = 5;

        await Assert.That(eif.Evaluate(cancellationToken)).IsEqualTo(1d);

        eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
        eif.Parameters["divider"] = 0;
        eif.Parameters["divided"] = 5;
        await Assert.That(eif.Evaluate(cancellationToken)).IsEqualTo(0);
    }

    [Test]
    public async Task ShouldHandleCaseSensitiveness(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("aBs(-1)", ExpressionOptions.DecimalAsDefault | ExpressionOptions.IgnoreCaseAtBuiltInFunctions).Evaluate(cancellationToken)).IsEqualTo(1M);
        await Assert.That(new Expression("Abs(-1)", ExpressionOptions.DecimalAsDefault).Evaluate(cancellationToken)).IsEqualTo(1M);

        await Assert.That(() => new Expression("aBs(-1)").Evaluate(cancellationToken)).ThrowsExactly<NCalcFunctionNotFoundException>();
    }

    [Test]
    public async Task ShouldCompareDates(CancellationToken cancellationToken)
    {
        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        await Assert.That(new Expression($"#1{dateSeparator}1{dateSeparator}2009#==#1{dateSeparator}1{dateSeparator}2009#")
            .Evaluate(cancellationToken)).IsEqualTo(true);
        await Assert.That(new Expression($"#2{dateSeparator}1{dateSeparator}2009#==#1{dateSeparator}1{dateSeparator}2009#")
            .Evaluate(cancellationToken)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldRoundAwayFromZero(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("Round(22.5, 0)")
            .Evaluate(cancellationToken)).IsEqualTo(22d);
        await Assert.That(new Expression("Round(22.5, 0)", ExpressionOptions.RoundAwayFromZero)
            .Evaluate(cancellationToken)).IsEqualTo(23d);
    }

    [Test]
    public async Task ShouldEvaluateSubExpressions(CancellationToken cancellationToken)
    {
        var volume = new Expression("[surface] * h");
        var surface = new Expression("[l] * [L]");
        volume.Parameters["surface"] = surface;
        volume.Parameters["h"] = 3;
        surface.Parameters["l"] = 1;
        surface.Parameters["L"] = 2;

        await Assert.That(volume.Evaluate(cancellationToken)).IsEqualTo(6);
    }

    [Test]
    [Arguments("Round(1.412;2)", 1.41)]
    [Arguments("Max(5.1;10.2)", 10.2)]
    [Arguments("Min(1.3;2)", 1.3)]
    [Arguments("Pow(5;2)", 25d)]
    public async Task ShouldAllowSemicolonAsArgumentSeparator(string expression, object expected, CancellationToken cancellationToken)
    {
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        var logicalExpression = LogicalExpressionParser.Parse(context);

        await Assert.That(new Expression(logicalExpression)
            .Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldAllowToUseCurlyBraces(CancellationToken cancellationToken)
    {
        var volume = new Expression("{surface} * h");
        var surface = new Expression("{l} * {L}");
        volume.Parameters["surface"] = surface;
        volume.Parameters["h"] = 3;
        surface.Parameters["l"] = 1;
        surface.Parameters["L"] = 2;

        await Assert.That(volume.Evaluate(cancellationToken)).IsEqualTo(6);
    }

    [Test]
    [MethodDataSource<NullCheckTestData>(nameof(NullCheckTestData.GetTestData))]
    public async Task ShouldAllowOperatorsWithNulls(string expression, object expected, CancellationToken cancellationToken)
    {
        var e = new Expression(expression, ExpressionOptions.AllowNullParameter);
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldEvaluateArrayParameters(CancellationToken cancellationToken)
    {
        var e = new Expression("x * x", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0, 1, 2, 3, 4 }
            }
        };

        var result = (IList<object>?)e.Evaluate(cancellationToken);

        await Assert.That(result).IsNotNull();
        await Assert.That(result[0]).IsEqualTo(0);
        await Assert.That(result[1]).IsEqualTo(1);
        await Assert.That(result[2]).IsEqualTo(4);
        await Assert.That(result[3]).IsEqualTo(9);
        await Assert.That(result[4]).IsEqualTo(16);
    }

    [Test]
    public async Task ShouldEvaluateArrayParametersWithFunctions(CancellationToken cancellationToken)
    {
        var e = new Expression("Round(x, 2)", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0.51, 1.671, 2.237, 3.568, 4.11 }
            }
        };

        var result = (IList<object>?)e.Evaluate(cancellationToken);

        await Assert.That(result).IsNotNull();
        await Assert.That(result[0]).IsEqualTo(0.51);
        await Assert.That(result[1]).IsEqualTo(1.67);
        await Assert.That(result[2]).IsEqualTo(2.24);
        await Assert.That(result[3]).IsEqualTo(3.57);
        await Assert.That(result[4]).IsEqualTo(4.11);
    }

    [Test]
    public async Task AllowNullOrEmptyExpressions(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("", ExpressionOptions.AllowNullOrEmptyExpressions)
            .Evaluate(cancellationToken)).IsEqualTo("");
        await Assert.That(new Expression((string?)null, ExpressionOptions.AllowNullOrEmptyExpressions)
            .Evaluate(cancellationToken)).IsNull();
    }

    [Test]
    [Arguments("01 == ''")]
    [Arguments("' ' == 01")]
    [Arguments("\" \" == 01")]
    [Arguments("\"dog\" == 01")]
    public async Task ShouldUseStrictTypeMatching(string expression, CancellationToken cancellationToken)
    {
        await Assert.That(new Expression(expression, ExpressionOptions.StrictTypeMatching)
            .Evaluate(cancellationToken) as bool?).IsFalse();
    }

    [Test]
    public async Task SpaceCharacterComparisonShouldBeTrue(CancellationToken cancellationToken)
    {
        var expr = new Expression("[Test] == ' '")
        {
            Parameters =
            {
                ["Test"] = ' '
            }
        };
        var result = expr.Evaluate(cancellationToken);
        await Assert.That(result is true).IsTrue();
    }
}