namespace NCalc.Tests;

[Trait("Category","Parameter Extraction")]
public class ParameterExtractionTests
{
    [Fact]
    public void Should_Get_Parameters_Issue_103()
    {
        var expression = new Expression("PageState == 'LIST' && a == 1 && customFunction() == true || in(1 + 1, 1, 2, 3)", ExpressionOptions.CaseInsensitiveStringComparer)
        {
            Parameters =
            {
                ["a"] = 1
            }
        };
        expression.DynamicParameters["PageState"] = _ => "List";
        expression.Functions["customfunction"] = (_, _) => true;

        var parameters = expression.GetParametersNames();
        Assert.Contains("a", parameters);
        Assert.Contains("PageState", parameters);
        Assert.Equal(2, parameters.Count);
    }

    [Fact]
    public void Should_Get_Parameters_One_Time_Issue_141()
    {
        var expression =
            new Expression("if(x=0,x,y)",
                ExpressionOptions.CaseInsensitiveStringComparer);
        var parameters = expression.GetParametersNames();
        
        Assert.Equal(2,parameters.Count);
    }

    [Fact]
    public void Should_Get_Parameters_With_Unary()
    {
        var expression = new Expression("-0.68");
        var p = expression.GetParametersNames();
        Assert.Empty(p);
    }
}