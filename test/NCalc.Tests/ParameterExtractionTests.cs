using System.Globalization;

namespace NCalc.Tests;

[Trait("Category","Parameter Extraction")]
public class ParameterExtractionTests
{
    [Fact]
    public void Should_Get_Parameters_Issue_103()
    {
        var expression = new Expression("PageState == 'LIST' && a == 1 && customFunction() == true || in(1 + 1, 1, 2, 3)", ExpressionOptions.CaseInsensitiveComparer)
        {
            Parameters =
            {
                ["a"] = 1
            }
        };
        expression.EvaluateParameter += (name, args) =>
        {
            if (name == "PageState")
                args.Result = "List";
        };
        
        expression.EvaluateFunction += (name, args) =>
        {
            if (name == "customfunction")
                args.Result = "true";
        };

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
                ExpressionOptions.CaseInsensitiveComparer,
                CultureInfo.InvariantCulture);
        var parameters = expression.GetParametersNames();
        
        Assert.Equal(2,parameters.Count);
    }
}