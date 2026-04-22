using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Parameters and Functions")]
public class ParametersAndFunctions
{
    [Test]
    public void ExpressionShouldEvaluateCustomFunctions()
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.Functions["SecretOperation"] = (args) => (int)args[0].Evaluate(CancellationToken.None) + (int)args[1].Evaluate(CancellationToken.None);
        Assert.Expression(9, e);
    }

    [Test]
    public void ExpressionShouldEvaluateCustomFunctionsWithParameters()
    {
        var e = new Expression("SecretOperation([e], 6) + f");
        e.Parameters["e"] = 3;
        e.Parameters["f"] = 1;

        e.Functions["SecretOperation"] = (args) => (int)args[0].Evaluate() + (int)args[1].Evaluate();

        Assert.Expression(10, e);
    }

    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctionsWithSameName()
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

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(12);
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(12);

        await Assert.That(d.FirstOrDefault().Value).IsEqualTo(2);
    }

    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctionsWithEffects()
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

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(false);

        e.Parameters["value"] = 11;
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(false);
        e.Parameters["value"] = 12;
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(false);
        e.Parameters["value"] = 13;
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(true);
        await Assert.That(times).HasSingleItem();
    }

    [Test]
    public void ExpressionShouldEvaluateParameters()
    {
        var e = new Expression("Round(Pow(Pi, 2) + Pow([Pi Squared], 2) + [X], 2)");

        e.Parameters["Pi Squared"] = new Expression("Pi * [Pi]");
        e.Parameters["X"] = 10;

        e.DynamicParameters["Pi"] = _ => 3.14;

        Assert.Expression(117.07, e);
    }

    [Test]
    public async Task Should_Evaluate_Function_Only_Once_Issue_107()
    {
        var counter = 0;
        var totalCounter = 0;

        var expression = new Expression("MyFunc()");

        expression.Functions["MyFunc"] = Expression_EvaluateFunction;

        for (var i = 0; i < 10; i++)
        {
            counter = 0;
            _ = expression.Evaluate(CancellationToken.None);
        }

        object Expression_EvaluateFunction(ExpressionFunctionData args)
        {
            counter++;
            totalCounter++;
            return 1;
        }

        await Assert.That(totalCounter).IsEqualTo(10);
    }

    [Test]
    public void ShouldOverrideExistingFunctions()
    {
        var e = new Expression("Round(1.99, 2)");

        Assert.Expression(1.99d, e);

        e.Functions["Round"] = (_) => 3;

        Assert.Expression(3, e);
    }

    [Test]
    public void ShouldHandleCustomParametersWhenNoSpecificParameterIsDefined()
    {
        var e = new Expression("Round(Pow([Pi], 2) + Pow([Pi], 2) + 10, 2)");

        e.DynamicParameters["Pi"] = _ => 3.14;
        e.Evaluate(CancellationToken.None);
    }

    [Test]
    public void ShouldHandleCustomFunctionsInFunctions()
    {
        var e = new Expression("if(true, func1(x) + func2(func3(y)), 0)");

        e.Functions["func1"] = (_) => 1;
        e.Functions["func2"] = (arg) => 2 * Convert.ToDouble(arg[0].Evaluate());
        e.Functions["func3"] = (arg) => 3 * Convert.ToDouble(arg[0].Evaluate());

        e.DynamicParameters["x"] = _ => 1;
        e.DynamicParameters["y"] = _ => 2;
        e.DynamicParameters["z"] = _ => 3;

        Assert.Expression(13d, e);
    }

    [Test]
    public async Task CustomFunctionShouldReturnNull()
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.Functions["SecretOperation"] = (_) => null;

        await Assert.That(e.Evaluate(CancellationToken.None)).IsNull();
    }

    [Test]
    public async Task CustomParametersShouldReturnNull()
    {
        var e = new Expression("x");

        e.DynamicParameters["x"] = _ => null;

        await Assert.That(e.Evaluate(CancellationToken.None)).IsNull();
    }

    [Test]
    [Arguments("notExistingfunction")]
    [Arguments("andDoThis")]
    public void ShouldTreatOperatorsWithoutWhitespaceAsFunctionName(string functionName)
    {
        var expression = new Expression($"{functionName}(3.14)")
        {
            Functions =
            {
                [functionName] = (_) => 1
            }
        };

        Assert.Expression(1, expression);
    }

    [Test]
    public void ShouldHandleCaseInsensitiveParameter()
    {
        var parameters = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Name", "Beatriz" }
        };

        Assert.Expression(true, "name == 'Beatriz'", parameters);
    }

    [Test]
    public async Task GetArgsCount()
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
        _ = expression.Evaluate(CancellationToken.None);
        await Assert.That(count).IsEqualTo(3);
    }

    [Test]
    public void MaxShouldWorkWithUShortAndShortTypes()
    {
        const ushort a = ushort.MaxValue;
        const short b = short.MaxValue;

        const int expected = ushort.MaxValue;

        Assert.Expression(expected,"Max([a], [b])", new Dictionary<string, object>
        {
            ["a"] = a,
            ["b"] = b
        });
    }
}