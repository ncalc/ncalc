namespace NCalc.Tests;

using Xunit;

[Trait("Category", "Parameters and Functions")]
public class ParametersAndFunctions
{
    [Fact]
    public void ExpressionShouldEvaluateCustomFunctions()
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.Functions["SecretOperation"] = (args) => (int)args[0].Evaluate() + (int)args[1].Evaluate();
        Assert.Equal(9, e.Evaluate());
    }

    [Fact]
    public void ExpressionShouldEvaluateCustomFunctionsWithParameters()
    {
        var e = new Expression("SecretOperation([e], 6) + f");
        e.Parameters["e"] = 3;
        e.Parameters["f"] = 1;

        e.Functions["SecretOperation"] = (args) => (int)args[0].Evaluate() + (int)args[1].Evaluate();

        Assert.Equal(10, e.Evaluate());
    }


    [Fact]
    public void ExpressionShouldEvaluateCustomFunctionsWithSameName()
    {
        var e = new Expression("SecretOperation(3, 6) + SecretOperation(1, 2)");

        var d = new Dictionary<string, int>();

        e.Functions["SecretOperation"] = (args) =>
        {
            var id = args.Id.ToString();

            if (id != null)
            {
                if (!d.ContainsKey(id))
                {
                    d[id] = 3;
                }
                else
                {
                    d[id]--;
                }
            }

            return (int)args[0].Evaluate() + (int)args[1].Evaluate();
        };

        Assert.Equal(12, e.Evaluate());
        Assert.Equal(12, e.Evaluate());

        Assert.Equal(2, d.FirstOrDefault().Value);
    }


    [Fact]
    public void ExpressionShouldEvaluateCustomFunctionsWithEffects()
    {
        var e = new Expression("Repeat([value] > 10, 3)");

        const string id = "Repeat";

        var times = new Dictionary<string, int>();
        e.Functions[id] = (args) =>
        {
            var t = (int)args[1].Evaluate() - 1;
            var r = (bool)args[0].Evaluate();
            if (r)
            {
                if (!times.ContainsKey(id))
                {
                    times[id] = t;
                }
                else
                {
                    times[id]--;
                }
            }

            return r && times[id] == 0;
        };
        e.Parameters["value"] = 9;

        Assert.Equal(false, e.Evaluate());

        e.Parameters["value"] = 11;
        Assert.Equal(false, e.Evaluate());
        e.Parameters["value"] = 12;
        Assert.Equal(false, e.Evaluate());
        e.Parameters["value"] = 13;
        Assert.Equal(true, e.Evaluate());
        Assert.Single(times);
    }

    [Fact]
    public void ExpressionShouldEvaluateParameters()
    {
        var e = new Expression("Round(Pow(Pi, 2) + Pow([Pi Squared], 2) + [X], 2)");

        e.Parameters["Pi Squared"] = new Expression("Pi * [Pi]");
        e.Parameters["X"] = 10;

        e.DynamicParameters["Pi"] = _ => 3.14;

        Assert.Equal(117.07, e.Evaluate());
    }

    [Fact]
    public void Should_Evaluate_Function_Only_Once_Issue_107()
    {
        var counter = 0;
        var totalCounter = 0;

        var expression = new Expression("MyFunc()");

        expression.Functions["MyFunc"] = Expression_EvaluateFunction;

        for (var i = 0; i < 10; i++)
        {
            counter = 0;
            _ = expression.Evaluate();
        }


        object Expression_EvaluateFunction(ExpressionFunctionData args)
        {
            counter++;
            totalCounter++;
            return 1;
        }

        Assert.Equal(10, totalCounter);
    }

    [Fact]
    public void ShouldOverrideExistingFunctions()
    {
        var e = new Expression("Round(1.99, 2)");

        Assert.Equal(1.99d, e.Evaluate());

        e.Functions["Round"] = (_) => 3;

        Assert.Equal(3, e.Evaluate());
    }

    [Fact]
    public void ShouldHandleCustomParametersWhenNoSpecificParameterIsDefined()
    {
        var e = new Expression("Round(Pow([Pi], 2) + Pow([Pi], 2) + 10, 2)");

        e.DynamicParameters["Pi"] = _ => 3.14;
        e.Evaluate();
    }

    [Fact]
    public void ShouldHandleCustomFunctionsInFunctions()
    {
        var e = new Expression("if(true, func1(x) + func2(func3(y)), 0)");

        e.Functions["func1"] = (_) => 1;
        e.Functions["func2"] = (arg) => 2 * Convert.ToDouble(arg[0].Evaluate());
        e.Functions["func3"] = (arg) => 3 * Convert.ToDouble(arg[0].Evaluate());

        e.DynamicParameters["x"] = _ => 1;
        e.DynamicParameters["y"] = _ => 2;
        e.DynamicParameters["z"] = _ => 3;

        Assert.Equal(13d, e.Evaluate());
    }

    [Fact]
    public void CustomFunctionShouldReturnNull()
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.Functions["SecretOperation"] = (_) => null;

        Assert.Null(e.Evaluate());
    }

    [Fact]
    public void CustomParametersShouldReturnNull()
    {
        var e = new Expression("x");

        e.DynamicParameters["x"] = _ => null;


        Assert.Null(e.Evaluate());
    }


    [Theory]
    [InlineData("notExistingfunction")]
    [InlineData("andDoThis")]
    public void ShouldTreatOperatorsWithoutWhitespaceAsFunctionName(string functionName)
    {
        var expression = new Expression($"{functionName}(3.14)");
        expression.Functions[functionName] = (_) => 1;

        Assert.Equal(1, expression.Evaluate());
    }

    [Fact]
    public void ShouldHandleCaseInsensitiveParameter()
    {
        var expression = new Expression("name == 'Beatriz'")
        {
            Parameters = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "Name", "Beatriz" }
            }
        };
        Assert.Equal(true, expression.Evaluate());
    }

    [Fact]
    public void GetArgsCount()
    {
        int count = 0;
        var expression = new Expression("count(1,2,3)")
        {
            Functions =
            {
                {
                    "count", args =>
                    {
                        count = args.Count();
                        return count;
                    }
                }
            }
        };
        _ = expression.Evaluate();
        Assert.Equal(3, count);
    }
}