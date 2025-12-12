namespace NCalc.Tests;

[Property("Category", "Parameters and Functions")]
public class ParametersAndFunctions
{
    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctions(CancellationToken cancellationToken)
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.Functions["SecretOperation"] = (args) => (int)args[0].Evaluate(cancellationToken) + (int)args[1].Evaluate(cancellationToken);
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(9);
    }

    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctionsWithParameters(CancellationToken cancellationToken)
    {
        var e = new Expression("SecretOperation([e], 6) + f");
        e.Parameters["e"] = 3;
        e.Parameters["f"] = 1;

        e.Functions["SecretOperation"] = (args) => (int)args[0].Evaluate() + (int)args[1].Evaluate();

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(10);
    }

    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctionsWithSameName(CancellationToken cancellationToken)
    {
        var e = new Expression("SecretOperation(3, 6) + SecretOperation(1, 2)");

        var d = new Dictionary<string, int>();

        e.Functions["SecretOperation"] = (args) =>
        {
            var id = args.Id.ToString();

            if (id != null)
            {
                if (!d.TryGetValue(id, out int value))
                {
                    d[id] = 3;
                }
                else
                {
                    d[id] = --value;
                }
            }

            return (int)args[0].Evaluate() + (int)args[1].Evaluate();
        };

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(12);
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(12);

        await Assert.That(d.FirstOrDefault().Value).IsEqualTo(2);
    }

    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctionsWithEffects(CancellationToken cancellationToken)
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
                if (!times.TryGetValue(id, out int value))
                {
                    times[id] = t;
                }
                else
                {
                    times[id] = --value;
                }
            }

            return r && times[id] == 0;
        };
        e.Parameters["value"] = 9;

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(false);

        e.Parameters["value"] = 11;
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(false);
        e.Parameters["value"] = 12;
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(false);
        e.Parameters["value"] = 13;
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(true);
        await Assert.That(times).HasSingleItem();
    }

    [Test]
    public async Task ExpressionShouldEvaluateParameters(CancellationToken cancellationToken)
    {
        var e = new Expression("Round(Pow(Pi, 2) + Pow([Pi Squared], 2) + [X], 2)");

        e.Parameters["Pi Squared"] = new Expression("Pi * [Pi]");
        e.Parameters["X"] = 10;

        e.DynamicParameters["Pi"] = _ => 3.14;

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(117.07);
    }

    [Test]
    public async Task Should_Evaluate_Function_Only_Once_Issue_107(CancellationToken cancellationToken)
    {
        var counter = 0;
        var totalCounter = 0;

        var expression = new Expression("MyFunc()");

        expression.Functions["MyFunc"] = Expression_EvaluateFunction;

        for (var i = 0; i < 10; i++)
        {
            counter = 0;
            _ = expression.Evaluate(cancellationToken);
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
    public async Task ShouldOverrideExistingFunctions(CancellationToken cancellationToken)
    {
        var e = new Expression("Round(1.99, 2)");

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(1.99d);

        e.Functions["Round"] = (_) => 3;

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(3);
    }

    [Test]
    public async Task ShouldHandleCustomParametersWhenNoSpecificParameterIsDefined(CancellationToken cancellationToken)
    {
        var e = new Expression("Round(Pow([Pi], 2) + Pow([Pi], 2) + 10, 2)");

        e.DynamicParameters["Pi"] = _ => 3.14;
        e.Evaluate(cancellationToken);
    }

    [Test]
    public async Task ShouldHandleCustomFunctionsInFunctions(CancellationToken cancellationToken)
    {
        var e = new Expression("if(true, func1(x) + func2(func3(y)), 0)");

        e.Functions["func1"] = (_) => 1;
        e.Functions["func2"] = (arg) => 2 * Convert.ToDouble(arg[0].Evaluate());
        e.Functions["func3"] = (arg) => 3 * Convert.ToDouble(arg[0].Evaluate());

        e.DynamicParameters["x"] = _ => 1;
        e.DynamicParameters["y"] = _ => 2;
        e.DynamicParameters["z"] = _ => 3;

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(13d);
    }

    [Test]
    public async Task CustomFunctionShouldReturnNull(CancellationToken cancellationToken)
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.Functions["SecretOperation"] = (_) => null;

        await Assert.That(e.Evaluate(cancellationToken)).IsNull();
    }

    [Test]
    public async Task CustomParametersShouldReturnNull(CancellationToken cancellationToken)
    {
        var e = new Expression("x");

        e.DynamicParameters["x"] = _ => null;

        await Assert.That(e.Evaluate(cancellationToken)).IsNull();
    }

    [Test]
    [Arguments("notExistingfunction")]
    [Arguments("andDoThis")]
    public async Task ShouldTreatOperatorsWithoutWhitespaceAsFunctionName(string functionName, CancellationToken cancellationToken)
    {
        var expression = new Expression($"{functionName}(3.14)");
        expression.Functions[functionName] = (_) => 1;

        await Assert.That(expression.Evaluate(cancellationToken)).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldHandleCaseInsensitiveParameter(CancellationToken cancellationToken)
    {
        var expression = new Expression("name == 'Beatriz'")
        {
            Parameters = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "Name", "Beatriz" }
            }
        };
        await Assert.That(expression.Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task GetArgsCount(CancellationToken cancellationToken)
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
        _ = expression.Evaluate(cancellationToken);
        await Assert.That(count).IsEqualTo(3);
    }

    [Test]
    public async Task MaxShouldWorkWithUShortAndShortTypes(CancellationToken cancellationToken)
    {
        const ushort a = ushort.MaxValue;
        const short b = short.MaxValue;

        var expr = new Expression("Max([a], [b])");
        expr.Parameters["a"] = a;
        expr.Parameters["b"] = b;

        var res = expr.Evaluate(cancellationToken);
        const int expected = ushort.MaxValue;
        await Assert.That(res).IsEqualTo(expected);
    }
}