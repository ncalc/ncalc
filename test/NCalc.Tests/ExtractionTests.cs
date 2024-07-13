namespace NCalc.Tests;

[Trait("Category","Extraction")]
public class ExtractionTests
{
    [Fact]
    public void ShouldGetParametersIssue103()
    {
        var expression = new Expression("PageState == 'LIST' && a == 1 && customFunction() == true || in(1 + 1, 1, 2, 3)", ExpressionOptions.CaseInsensitiveStringComparer)
        {
            Parameters =
            {
                ["a"] = 1
            }
        };
        expression.DynamicParameters["PageState"] = _ => "List";
        expression.Functions["customfunction"] = _ => true;

        var parameters = expression.GetParametersNames();
        Assert.Contains("a", parameters);
        Assert.Contains("PageState", parameters);
        Assert.Equal(2, parameters.Count);
    }

    [Fact]
    public void ShouldGetParametersOneTimeIssue141()
    {
        var expression =
            new Expression("if(x=0,x,y)",
                ExpressionOptions.CaseInsensitiveStringComparer);
        var parameters = expression.GetParametersNames();
        
        Assert.Equal(2,parameters.Count);
    }

    [Fact]
    public void ShouldGetParametersWithUnary()
    {
        var expression = new Expression("-0.68");
        var p = expression.GetParametersNames();
        Assert.Empty(p);
    }
    
    [Theory]
    [InlineData("(a, b, c)", 3)]
    [InlineData("725 - 1 == result * secret_operation(secretValue)", 2)]
    [InlineData("getLightsaberColor(selectedJedi) == selectedColor", 2)]
    public void ShouldGetParameters(string formula, int expectedCount)
    {
        var expression = new Expression(formula);
        var p = expression.GetParametersNames();
        Assert.Equal(expectedCount, p.Count);
    }

    [InlineData("(a, drop_database(), c) == toUpper(getName())", 3)]
    [InlineData("Abs(523/2) == Abs(523/2)", 1)]
    [InlineData("getLightsaberColor('Yoda') == selectedColor", 1)]
    [Theory]
    public void ShouldGetFunctions(string formula, int expectedCount)
    {
        var expression = new Expression(formula);
        var functions = expression.GetFunctionsNames();
        Assert.Equal(expectedCount, functions.Count);
    }
}