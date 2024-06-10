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
        var expr = new Expression(formula, CultureInfo.InvariantCulture);
        var res = expr.Evaluate();

        Assert.Equal(expectedValue, res);
    }
}