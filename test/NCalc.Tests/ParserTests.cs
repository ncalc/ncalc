using NCalc.Domain;
using NCalc.Factories;
using NCalc.Parser;

namespace NCalc.Tests;

[Trait("Category", "Parser")]
public class ParserTests
{
    [Theory]
    [InlineData("11+33 ", 44)]
    [InlineData(" 11+33", 44)]
    [InlineData(" 11+33 ", 44)]
    [InlineData("0.0-1.1", -1.1)]
    public void ShouldIgnoreWhitespacesIssue222(string formula, object expectedValue)
    {
        var expression = new Expression(formula, CultureInfo.InvariantCulture);
        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("not( true )", false)]
    [InlineData("not ( true )", false)]
    [InlineData("not(true)", false)]
    [InlineData(" not(true) ", false)]
    public void NotBehaviorIssue226(string formula, object expectedValue)
    {
        var expression = new Expression(formula, CultureInfo.InvariantCulture);
        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void ShouldHandleNewLines()
    {
        const string formula = """
                               2+3


                               """;

        var expression = new Expression(formula, CultureInfo.InvariantCulture);
        var result = expression.Evaluate();

        Assert.Equal(5, result);
    }

    [Fact]
    public void RequireClosingAtIdentifiersIssue244()
    {
        const string formula = "[{Diagnostic}.Data]";

        var logicalExpression = LogicalExpressionFactory.Create(formula);

        Assert.IsType<Identifier>(logicalExpression);

        Assert.Equal("{Diagnostic}.Data", ((Identifier)logicalExpression).Name);
    }

    [Fact]
    public void AllowCharValues()
    {
        const string formula = "'c'";

        var logicalExpression = LogicalExpressionFactory.Create(formula, ExpressionOptions.AllowCharValues);

        Assert.IsType<ValueExpression>(logicalExpression);

        Assert.Equal('c', ((ValueExpression)logicalExpression).Value);
    }

    [InlineData("(1+2)*3", 9)]
    [InlineData("(8 * 8) + 1", 65)]
    [InlineData("1 + 1", 2)]
    [InlineData("-1 - 1", -2)]
    [Theory]
    public void ShouldHandleBinaryExpression(string formula, int expectedResult)
    {
        var logicalExpression = LogicalExpressionFactory.Create(formula);

        Assert.IsType<BinaryExpression>(logicalExpression);

        var expression = new Expression(logicalExpression);

        Assert.Equal(expectedResult, expression.Evaluate());
    }

    [InlineData("(1,2,3,4,5)", 5)]
    [InlineData("()", 0)]
    [InlineData("('Hello', func())", 2)]
    [Theory]
    public void ShouldParseLists(string formula, int arrayExpectedCount)
    {
        var logicalExpression = LogicalExpressionFactory.Create(formula);

        Assert.IsType<LogicalExpressionList>(logicalExpression);

        Assert.Equal(arrayExpectedCount, ((LogicalExpressionList)logicalExpression).Count);
    }

    [InlineData("78b1941f4e7941c9bef656fad7326538")]
    [InlineData("b1548bd5-2556-4d2a-9f47-bb8d421026dd")]
    [InlineData("f44e449f-b02f-4f81-96d8-9292c5623b8b")]
    [InlineData("db6ff6c1-8290-4bdb-8c32-b78f3f2f6231")]
    [InlineData("e21aefee-8ffe-422c-a03f-7beb60975992")]
    [InlineData("13f722a3-5139-4eda-bffb-ed32e8cc90d1")]
    [InlineData("58ede4c6-2323-4b1a-ad66-c062fccb8473")]
    [InlineData("F6A41126850F41489192C89A949B2392")]
    [InlineData("E3825D69DAC04CA88D73E8CC3C8ADA2F")]
    [InlineData("B0D256EB-E88C-4409-A80B-E7341295B9F1")]
    [InlineData("C785E8B3-D080-45AF-947E-3D8F246788A6")]
    [Theory]
    public void ShouldParseGuids(string formula)
    {
        var logicalExpression = LogicalExpressionFactory.Create(formula);

        Assert.IsType<ValueExpression>(logicalExpression);

        Assert.IsType<Guid>(new Expression(logicalExpression).Evaluate());
    }

    [Fact]
    public void ShouldParseGuidInsideFunction()
    {
        var logicalExpression = LogicalExpressionFactory.Create("getUser(78b1941f4e7941c9bef656fad7326538)");

        if (logicalExpression is Function function)
        {
            Assert.True(function.Parameters[0] is ValueExpression { Value: Guid });
        }
        else
        {
            Assert.Fail("Logical expression is not a function.");
        }
    }

    [Fact]
    public void OperatorPriorityIssue337()
    {
        Assert.True((bool)new Expression("true or true and false").Evaluate()!);
    }

    [Fact]
    public void ShouldNotFailIssue372()
    {
        var chars = new List<string>();
        for (var c = 'a'; c < 'z'; ++c)
        {
            chars.Add(c.ToString());
        }

        for (var c = 'A'; c < 'Z'; ++c)
        {
            chars.Add(c.ToString());
        }

        var failed = new List<string>();
        foreach (var c in chars)
        {
            try
            {
                var context = new LogicalExpressionParserContext(c, ExpressionOptions.None);
                LogicalExpressionParser.Parse(context);
            }
            catch (Exception)
            {
                failed.Add(c);
            }
        }

        Assert.Empty(failed);
    }

    [Fact]
    public void ShouldNotFailIssue399()
    {
        bool exceptionThrown = false;

        try
        {
            ExpressionOptions expressionOptions = ExpressionOptions.DecimalAsDefault | ExpressionOptions.NoCache;
            var expression = new Expression("0.3333333333333333333333 + 1.6666666666666666666667", expressionOptions, CultureInfo.InvariantCulture);
            var result = expression.Evaluate();
        }
        catch(Exception ex)
        {
            exceptionThrown = true;
        }

        Assert.False(exceptionThrown);
    }
}