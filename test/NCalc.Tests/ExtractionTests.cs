namespace NCalc.Tests;

[Trait("Category", "Extraction")]
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

        var parameters = expression.GetParameterNames();
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
        var parameters = expression.GetParameterNames();

        Assert.Equal(2, parameters.Count);
    }

    [Fact]
    public void ShouldGetParametersWithUnary()
    {
        var expression = new Expression("-0.68");
        var p = expression.GetParameterNames();
        Assert.Empty(p);
    }

    [Theory]
    [InlineData("(a, b, c)", 3)]
    [InlineData("725 - 1 == result * secret_operation(secretValue)", 2)]
    [InlineData("getLightsaberColor(selectedJedi) == selectedColor", 2)]
    public void ShouldGetParameters(string formula, int expectedCount)
    {
        var expression = new Expression(formula);
        var p = expression.GetParameterNames();
        Assert.Equal(expectedCount, p.Count);
    }

    [InlineData("(a, drop_database(), c) == toUpper(getName())", 3)]
    [InlineData("Abs(523/2) == Abs(523/2)", 1)]
    [InlineData("getLightsaberColor('Yoda') == selectedColor", 1)]
    [Theory]
    public void ShouldGetFunctions(string formula, int expectedCount)
    {
        var expression = new Expression(formula);
        var functions = expression.GetFunctionNames();
        Assert.Equal(expectedCount, functions.Count);
    }

    [Fact]
    public void ShouldGetParametersInsideFunctionsIssue305()
    {
        var expression = new Expression("if([Value] >= 50, 'background-color: #80ffcc;', null)", ExpressionOptions.AllowNullParameter);
        var parameters = expression.GetParameterNames();
        Assert.Equal(2, parameters.Count);
    }

    [Fact]
    public void ShouldGetFunctionsInsideFunctionsIssue305()
    {
        var expression = new Expression("if(getValue() >= 50, 'background-color: #80ffcc;', null)", ExpressionOptions.AllowNullParameter);
        var functions = expression.GetFunctionNames();
        Assert.Equal(2, functions.Count);
    }
}