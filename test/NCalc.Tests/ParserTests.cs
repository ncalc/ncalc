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
    public void NotBehaviorIssue226(string formula, object expectedValue)
    {
        var expression = new Expression(formula, CultureInfo.InvariantCulture);
        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }
}