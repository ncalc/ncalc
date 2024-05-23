namespace NCalc.Tests;

[Trait("Category","Evaluations")]
public class EvaluationTests
{
    
    [Theory]
    [InlineData("2 + 3 + 5", 10)]
    [InlineData("2 * 3 + 5", 11)]
    [InlineData("2 * (3 + 5)", 16)]
    [InlineData("2 * (2*(2*(2+1)))", 24)]
    [InlineData("10 % 3", 1)]
    [InlineData("true or false", true)]
    [InlineData("not true", false)]
    [InlineData("false || not (false and true)", true)]
    [InlineData("3 > 2 and 1 <= (3-2)", true)]
    [InlineData("3 % 2 != 10 % 3", false)]
    public void Expression_Should_Evaluate(string expression, object expected)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate());
    }
    
    [Theory]
    [InlineData("123456", 123456)]
    [InlineData(".2", 0.2d)]
    [InlineData("123.456", 123.456d)]
    [InlineData("123.", 123d)]
    [InlineData("123.E2", 12300d)]
    [InlineData("true", true)]
    [InlineData("'true'", "true")]
    [InlineData("'azerty'", "azerty")]
    public void Should_Parse_Values(string input, object expectedValue, params object[] args)
    {
        var expression = new Expression(input);
        var result = expression.Evaluate();
        
        if (expectedValue is double expectedDouble)
        {
            Assert.Equal(expectedDouble, (double)result, precision: 15);
        }
        else
        {
            Assert.Equal(expectedValue, result);
        }
    }
    

    [Fact]
    public void ShouldEvaluateInOperator()
    {
        // The last argument should not be evaluated
        var ein = new Expression("in((2 + 2), [1], [2], 1 + 2, 4, 1 / 0)");
        ein.Parameters["1"] = 2;
        ein.Parameters["2"] = 5;

        Assert.Equal(true, ein.Evaluate());

        var eout = new Expression("in((2 + 2), [1], [2], 1 + 2, 3)");
        eout.Parameters["1"] = 2;
        eout.Parameters["2"] = 5;

        Assert.Equal(false, eout.Evaluate());

        // Should work with strings
        var estring = new Expression("in('to' + 'to', 'titi', 'toto')");

        Assert.Equal(true, estring.Evaluate());
    }
    
    [Fact]
    public void ShouldEvaluateTernaryExpression()
    {
        Assert.Equal(1, new Expression("1+2<3 ? 3+4 : 1").Evaluate());
    }
    
    
    [Fact]
    public void Should_Not_Throw_Function_Not_Found_Issue_110()
    {
        const string expressionStr = "IN([acp_associated_person_transactions], 'T', 'Z', 'A')";
        var expression = new Expression(expressionStr)
        {
            Options = ExpressionOptions.RoundAwayFromZero | ExpressionOptions.IgnoreCase,
            Parameters =
            {
                ["acp_associated_person_transactions"] = 'T'
            }
        };

        Assert.Equal(true, expression.Evaluate());
    }

        
    [Fact]
    public void Should_Evaluate_Ifs()
    {
        // Test first case true, return next value
        var eifs = new Expression("ifs([divider] != 0, [divided] / [divider], -1)");
        eifs.Parameters["divider"] = 5;
        eifs.Parameters["divided"] = 5;

        Assert.Equal(1d, eifs.Evaluate());

        // Test first case false, no next case, return default value
        eifs = new Expression("ifs([divider] != 0, [divided] / [divider], -1)");
        eifs.Parameters["divider"] = 0;
        eifs.Parameters["divided"] = 5;

        Assert.Equal(-1, eifs.Evaluate());

        // Test first case false, next case true, return next value (eg 4th expr)

        eifs = new Expression("ifs([number] == 3, 5, [number] == 5, 3, 8)");
        eifs.Parameters["number"] = 5;
        Assert.Equal(3, eifs.Evaluate());

        // Test first case false, next case false, return default value (eg 5th expr)

        eifs = new Expression("ifs([number] == 3, 5, [number] == 5, 3, 8)");
        eifs.Parameters["number"] = 1337;

        Assert.Equal(8, eifs.Evaluate());
    }
    
    [Fact]
    public void ShouldEvaluateConditional()
    {
        var eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
        eif.Parameters["divider"] = 5;
        eif.Parameters["divided"] = 5;

        Assert.Equal(1d, eif.Evaluate());

        eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
        eif.Parameters["divider"] = 0;
        eif.Parameters["divided"] = 5;
        Assert.Equal(0, eif.Evaluate());
    }

    [Fact]
    public void ShouldHandleCaseSensitiveness()
    {
        Assert.Equal(1M, new Expression("aBs(-1)", ExpressionOptions.IgnoreCase).Evaluate());
        Assert.Equal(1M, new Expression("Abs(-1)").Evaluate());

        Assert.Throws<ArgumentException>(() => new Expression("aBs(-1)").Evaluate());
    }

    [Fact]
    public void ShouldCompareDates()
    {
        Assert.Equal(true, new Expression("#1/1/2009#==#1/1/2009#").Evaluate());
        Assert.Equal(false, new Expression("#2/1/2009#==#1/1/2009#").Evaluate());
    }

    [Fact]
    public void ShouldRoundAwayFromZero()
    {
        Assert.Equal(22d, new Expression("Round(22.5, 0)").Evaluate());
        Assert.Equal(23d, new Expression("Round(22.5, 0)", ExpressionOptions.RoundAwayFromZero).Evaluate());
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

        Assert.Equal(6, volume.Evaluate());
    }

    [Theory]
    [InlineData("Round(1.412; 2)", 1.41)]
    [InlineData("Max(5.1; 10.2)", 10.2)]
    [InlineData("Min(1.3; 2)", 1.3)]
    [InlineData("Pow(5;2)", 25)]
    public void ShouldAllowSemicolonAsArgumentSeparator(string expression, object expected)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate());
    }
}