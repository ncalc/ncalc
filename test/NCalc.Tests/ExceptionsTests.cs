using NCalc.Exceptions;
using NCalc.Factories;

namespace NCalc.Tests;

[Trait("Category", "Exceptions")]
public class ExceptionsTests
{
    [Theory]
    [InlineData("ifs()")]
    [InlineData("ifs([divider] > 0)")]
    [InlineData("ifs([divider] > 0, [divider] / [divided])")]
    [InlineData("ifs([divider] > 0, [divider] / [divided], [divider < 0], [divider] + [divided])")]
    public void Ifs_With_Improper_Arguments_Should_Throw_Exceptions(string expression)
    {
        Assert.Throws<NCalcEvaluationException>(() => new Expression(expression).Evaluate());
    }

    [Theory]
    [InlineData(". + 2")]
    [InlineData("(3 + 2")]
    [InlineData("42a")]
    [InlineData("42a.3")]
    [InlineData("42.3a")]
    [InlineData("42a.3b")]
    [InlineData("42.3e-5a")]
    [InlineData("42 + [a + 10")]
    [InlineData("42 a")]
    [InlineData("42 '")]
    [InlineData("Abs(-1) ]")]
    [InlineData("42. 3")]
    [InlineData("42 .3")]
    [InlineData("42 . 3")]
    public void Should_Throw_Parse_Exception(string expression)
    {
        Assert.Throws<NCalcParserException>(() => new Expression(expression).Evaluate());
    }

    [Fact]
    public void Should_Detect_Syntax_Errors_Before_Evaluation()
    {
        //Yes, I know at v3 the test was a + b * ( , but it's impossible to define an empty expression.
        var e = new Expression("a + b * ( 1 + 1");
        Assert.Null(e.Error);
        Assert.True(e.HasErrors());
        Assert.NotNull(e.Error);

        e = new Expression("* b ");
        Assert.Null(e.Error);
        Assert.True(e.HasErrors());
        Assert.NotNull(e.Error);
    }

    [Fact]
    public void Should_Display_Error_If_UncompatibleTypes()
    {
        var e = new Expression("(a > b) + 10")
        {
            Parameters =
            {
                ["a"] = 1,
                ["b"] = 2
            }
        };

        Assert.Throws<InvalidOperationException>(() =>
        {
            e.Evaluate();
        });
    }

    [Fact]
    public void Should_Throw_Exception_On_Lexer_Errors_Issue_6()
    {
        Assert.Throws<NCalcParserException>(() => LogicalExpressionFactory.Create("#t -chers"));

        var invalidDateException = Assert.Throws<FormatException>(() => LogicalExpressionFactory.Create("#13/13/2222#"));
        Assert.IsType<FormatException>(invalidDateException);

        //At v4, DateTime is better handled, and this should no longer cause errors.
        // https://github.com/ncalc/ncalc-async/issues/6
        try
        {
            LogicalExpressionFactory.Create("Format(\"{0:(###) ###-####}\", \"9999999999\")");
        }
        catch
        {
            Assert.Fail("No exception should be thrown here.");
        }
    }

    [Fact]
    public void ShouldProvideErrorPosition()
    {
        var expression = new Expression("42a");
        try
        {
            expression.Evaluate();
            Assert.Throws<NCalcParserException>(() => true);
        }
        catch (NCalcParserException ex)
        {
            Assert.Equal("Invalid token in expression at position (1:3)", ex.InnerException.Message);
        }
    }

    [Fact]
    public void Should_Throw_Function_Not_Found()
    {
        var expression = new Expression("drop_database()");
        var exception = Assert.Throws<NCalcFunctionNotFoundException>(() => expression.Evaluate());
        Assert.Equal("drop_database", exception.FunctionName);
    }

    [Fact]
    public void Should_Throw_Parameter_Not_Found()
    {
        var expression = new Expression("{Name} == 'Spinella'");
        var exception = Assert.Throws<NCalcParameterNotDefinedException>(() => expression.Evaluate());
        Assert.Equal("Name", exception.ParameterName);
    }

    [Theory]
    [InlineData("5+-*10")]
    [InlineData("5+*10")]
    [InlineData("5/-*10")]
    public void Should_Throw_Issue_195(string expressionString)
    {
        var expression = new Expression(expressionString);
        Assert.Throws<NCalcParserException>(() => expression.Evaluate());
    }

    [Fact]
    public void Should_Throw_Issue_208()
    {
        var expression = new Expression("1.3,4.5");
        Assert.Throws<NCalcParserException>(() => expression.Evaluate());
    }

    [Fact]
    public void ShouldThrowExceptionWhenNullOrEmpty()
    {
        Assert.Throws<NCalcException>(() => new Expression("").Evaluate());
        Assert.Throws<NCalcException>(() => new Expression((string)null).Evaluate());
    }
}