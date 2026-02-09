#nullable enable
using NCalc.Exceptions;
using NCalc.Parser;

namespace NCalc.Tests;

[Trait("Category", "Evaluations")]
public class EvaluationTests
{
    [Theory]
    [ClassData(typeof(EvaluationTestData))]
    public void Expression_Should_Evaluate(string expression, object expected)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [ClassData(typeof(ValuesTestData))]
    public void ShouldParseValues(string input, object expectedValue)
    {
        var expression = new Expression(input);

        var result = expression.Evaluate(TestContext.Current.CancellationToken);

        if (expectedValue is double expectedDouble)
        {
            Assert.Equal(expectedDouble, (double)result!, precision: 15);
        }
        else
        {
            Assert.Equal(expectedValue, result);
        }
    }

    [Fact]
    public void ShouldEvaluateInFunction()
    {
        // The last argument should not be evaluated
        var ein = new Expression("in((2 + 2), [1], [2], 1 + 2, 4, 1 / 0)");
        ein.Parameters["1"] = 2;
        ein.Parameters["2"] = 5;

        Assert.Equal(true, ein.Evaluate(TestContext.Current.CancellationToken));

        var eout = new Expression("in((2 + 2), [1], [2], 1 + 2, 3)");
        eout.Parameters["1"] = 2;
        eout.Parameters["2"] = 5;

        Assert.Equal(false, eout.Evaluate(TestContext.Current.CancellationToken));

        // Should work with strings
        var estring = new Expression("in('to' + 'to', 'titi', 'toto')");

        Assert.Equal(true, estring.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldEvaluateTernaryExpression()
    {
        Assert.Equal(1, new Expression("1+2<3 ? 3+4 : 1").Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Should_Not_Throw_Function_Not_Found_Issue_110()
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

        Assert.Equal(true, expression.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Should_Evaluate_Ifs()
    {
        // Test first case true, return next value
        var eifs = new Expression("ifs([divider] != 0, [divided] / [divider], -1)");
        eifs.Parameters["divider"] = 5;
        eifs.Parameters["divided"] = 5;

        Assert.Equal(1d, eifs.Evaluate(TestContext.Current.CancellationToken));

        // Test first case false, no next case, return default value
        eifs = new Expression("ifs([divider] != 0, [divided] / [divider], -1)");
        eifs.Parameters["divider"] = 0;
        eifs.Parameters["divided"] = 5;

        Assert.Equal(-1, eifs.Evaluate(TestContext.Current.CancellationToken));

        // Test first case false, next case true, return next value (eg 4th expr)

        eifs = new Expression("ifs([number] == 3, 5, [number] == 5, 3, 8)");
        eifs.Parameters["number"] = 5;
        Assert.Equal(3, eifs.Evaluate(TestContext.Current.CancellationToken));

        // Test first case false, next case false, return default value (eg 5th expr)

        eifs = new Expression("ifs([number] == 3, 5, [number] == 5, 3, 8)");
        eifs.Parameters["number"] = 1337;

        Assert.Equal(8, eifs.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldEvaluateConditional()
    {
        var eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
        eif.Parameters["divider"] = 5;
        eif.Parameters["divided"] = 5;

        Assert.Equal(1d, eif.Evaluate(TestContext.Current.CancellationToken));

        eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
        eif.Parameters["divider"] = 0;
        eif.Parameters["divided"] = 5;
        Assert.Equal(0, eif.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldHandleCaseSensitiveness()
    {
        Assert.Equal(1M, new Expression("aBs(-1)", ExpressionOptions.DecimalAsDefault | ExpressionOptions.IgnoreCaseAtBuiltInFunctions).Evaluate(TestContext.Current.CancellationToken));
        Assert.Equal(1M, new Expression("Abs(-1)", ExpressionOptions.DecimalAsDefault).Evaluate(TestContext.Current.CancellationToken));

        Assert.Throws<NCalcFunctionNotFoundException>(() => new Expression("aBs(-1)").Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldCompareDates()
    {
        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        Assert.Equal(true, new Expression($"#1{dateSeparator}1{dateSeparator}2009#==#1{dateSeparator}1{dateSeparator}2009#")
            .Evaluate(TestContext.Current.CancellationToken));
        Assert.Equal(false, new Expression($"#2{dateSeparator}1{dateSeparator}2009#==#1{dateSeparator}1{dateSeparator}2009#")
            .Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldRoundAwayFromZero()
    {
        Assert.Equal(22d, new Expression("Round(22.5, 0)")
            .Evaluate(TestContext.Current.CancellationToken));
        Assert.Equal(23d, new Expression("Round(22.5, 0)", ExpressionOptions.RoundAwayFromZero)
            .Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldEvaluateSubExpressions()
    {
        var volume = new Expression("[surface] * h");
        var surface = new Expression("[l] * [L]");
        volume.Parameters["surface"] = surface;
        volume.Parameters["h"] = 3;
        surface.Parameters["l"] = 1;
        surface.Parameters["L"] = 2;

        Assert.Equal(6, volume.Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("Round(1.412;2)", 1.41)]
    [InlineData("Max(5.1;10.2)", 10.2)]
    [InlineData("Min(1.3;2)", 1.3)]
    [InlineData("Pow(5;2)", 25d)]
    public void ShouldAllowSemicolonAsArgumentSeparator(string expression, object expected)
    {
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        var logicalExpression = LogicalExpressionParser.Parse(context);

        Assert.Equal(expected, new Expression(logicalExpression)
            .Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldAllowToUseCurlyBraces()
    {
        var volume = new Expression("{surface} * h");
        var surface = new Expression("{l} * {L}");
        volume.Parameters["surface"] = surface;
        volume.Parameters["h"] = 3;
        surface.Parameters["l"] = 1;
        surface.Parameters["L"] = 2;

        Assert.Equal(6, volume.Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [ClassData(typeof(NullCheckTestData))]
    public void ShouldAllowOperatorsWithNulls(string expression, object expected)
    {
        var e = new Expression(expression, ExpressionOptions.AllowNullParameter);
        Assert.Equal(expected, e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldEvaluateArrayParameters()
    {
        var e = new Expression("x * x", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0, 1, 2, 3, 4 }
            }
        };

        var result = (IList<object>?)e.Evaluate(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(0, result[0]);
        Assert.Equal(1, result[1]);
        Assert.Equal(4, result[2]);
        Assert.Equal(9, result[3]);
        Assert.Equal(16, result[4]);
    }

    [Fact]
    public void ShouldEvaluateArrayParametersWithFunctions()
    {
        var e = new Expression("Round(x, 2)", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0.51, 1.671, 2.237, 3.568, 4.11 }
            }
        };

        var result = (IList<object>?)e.Evaluate(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(0.51, result[0]);
        Assert.Equal(1.67, result[1]);
        Assert.Equal(2.24, result[2]);
        Assert.Equal(3.57, result[3]);
        Assert.Equal(4.11, result[4]);
    }

    [Fact]
    public void AllowNullOrEmptyExpressions()
    {
        Assert.Equal("", new Expression("", ExpressionOptions.AllowNullOrEmptyExpressions)
            .Evaluate(TestContext.Current.CancellationToken));
        Assert.Null(new Expression((string?)null, ExpressionOptions.AllowNullOrEmptyExpressions)
            .Evaluate(TestContext.Current.CancellationToken));
    }
    [Theory]
    [InlineData("01 == ''")]
    [InlineData("' ' == 01")]
    [InlineData("\" \" == 01")]
    [InlineData("\"dog\" == 01")]
    public void ShouldUseStrictTypeMatching(string expression)
    {
        Assert.False(new Expression(expression, ExpressionOptions.StrictTypeMatching)
            .Evaluate(TestContext.Current.CancellationToken) as bool?);
    }

    [Fact]
    public void SpaceCharacterComparisonShouldBeTrue()
    {
        var expr = new Expression("[Test] == ' '")
        {
            Parameters =
            {
                ["Test"] = ' '
            }
        };
        var result = expr.Evaluate(TestContext.Current.CancellationToken);
        Assert.True(result is true);
    }
}