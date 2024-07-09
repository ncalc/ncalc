using NCalc.Domain;
using NCalc.Factories;

namespace NCalc.Tests;

[Trait("Category", "Parser")]
public class ParserTests
{
    [Theory]
    [InlineData("11+33 ", 44)]
    [InlineData(" 11+33", 44)]
    [InlineData(" 11+33 ", 44)]
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
        
        Assert.Equal("{Diagnostic}.Data",((Identifier)logicalExpression).Name);
    }
    
    [Fact]
    public void AllowCharValues()
    {
        const string formula = "'c'";

        var logicalExpression = LogicalExpressionFactory.Create(formula, ExpressionOptions.AllowCharValues);

        Assert.IsType<ValueExpression>(logicalExpression);
        
        Assert.Equal('c',((ValueExpression)logicalExpression).Value);
    }
    
    [InlineData("(1+2)*3",9)]
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
    
    [InlineData("(1,2,3,4,5)",5)]
    [InlineData("()", 0)]
    [InlineData("('Hello', func())", 2)]
    [Theory]
    public void ShouldParseArrays(string formula, int arrayExpectedCount)
    {
        var logicalExpression = LogicalExpressionFactory.Create(formula);

        Assert.IsType<ArrayExpression>(logicalExpression);
        
        Assert.Equal(arrayExpectedCount, ((ArrayExpression)logicalExpression).Values.Length);
    }
}