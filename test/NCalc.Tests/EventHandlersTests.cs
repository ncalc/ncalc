using NCalc.Handlers;
using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Event Handlers")]
public class EventHandlersTests
{
    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctions()
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.EvaluateFunction += (name, args) =>
        {
            if (name == "SecretOperation")
            {
                args.Result = (int)args.Parameters[0].Evaluate(args.CancellationToken) +
                    (int)args.Parameters[1].Evaluate(args.CancellationToken);
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(9);
    }

    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctionsWithParameters()
    {
        var e = new Expression("SecretOperation([e], 6) + f");
        e.Parameters["e"] = 3;
        e.Parameters["f"] = 1;

        e.EvaluateFunction += (name, args) =>
        {
            if (name == "SecretOperation")
                args.Result = (int)args.Parameters[0].Evaluate(args.CancellationToken) + (int)args.Parameters[1].Evaluate(args.CancellationToken);
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(10);
    }

    [Test]
    public async Task ExpressionShouldEvaluateParameters()
    {
        var e = new Expression("Round(Pow(Pi, 2) + Pow([Pi Squared], 2) + [X], 2)");

        e.Parameters["Pi Squared"] = new Expression("Pi * [Pi]");
        e.Parameters["X"] = 10;

        e.EvaluateParameter += (name, args) =>
        {
            if (name == "Pi")
                args.Result = 3.14;
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(117.07);
    }

    [Test]
    public async Task Should_Evaluate_Function_Only_Once_Issue_107()
    {
        var counter = 0;
        var totalCounter = 0;

        var expression = new Expression("MyFunc()");

        expression.EvaluateFunction += Expression_EvaluateFunction;

        for (var i = 0; i < 10; i++)
        {
            counter = 0;
            _ = expression.Evaluate(CancellationToken.None);
        }

        void Expression_EvaluateFunction(string name, FunctionArgs args)
        {
            if (name != "MyFunc")
                return;
            args.Result = 1;
            counter++;
            totalCounter++;
        }

        await Assert.That(totalCounter).IsEqualTo(10);
    }

    [Test]
    public async Task ShouldOverrideExistingFunctions()
    {
        var e = new Expression("Round(1.99, 2)");

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(1.99d);

        e.EvaluateFunction += (name, args) =>
        {
            if (name == "Round")
                args.Result = 3;
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(3);
    }

    [Test]
    public void ShouldHandleCustomParametersWhenNoSpecificParameterIsDefined()
    {
        var e = new Expression("Round(Pow([Pi], 2) + Pow([Pi], 2) + 10, 2)");

        e.EvaluateParameter += (name, arg) =>
        {
            if (name == "Pi")
                arg.Result = 3.14;
        };

        e.Evaluate(CancellationToken.None);
    }

    [Test]
    public async Task ShouldHandleCustomFunctionsInFunctions()
    {
        var e = new Expression("if(true, func1(x) + func2(func3(y)), 0)");

        e.EvaluateFunction += (name, arg) =>
        {
            switch (name)
            {
                case "func1":
                    arg.Result = 1;
                    break;
                case "func2":
                    arg.Result = 2 * Convert.ToDouble(arg.Parameters[0].Evaluate(arg.CancellationToken));
                    break;
                case "func3":
                    arg.Result = 3 * Convert.ToDouble(arg.Parameters[0].Evaluate(arg.CancellationToken));
                    break;
            }
        };

        e.EvaluateParameter += (name, arg) =>
        {
            switch (name)
            {
                case "x":
                    arg.Result = 1;
                    break;
                case "y":
                    arg.Result = 2;
                    break;
                case "z":
                    arg.Result = 3;
                    break;
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(13d);
    }

    [Test]
    public async Task CustomFunctionShouldReturnNull()
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.EvaluateFunction += (name, args) =>
        {
            if (args.HasResult)
            {
                Assert.Fail("Expected no result before assignment.");
            }
            if (name == "SecretOperation")
                args.Result = null;
            if (!args.HasResult)
            {
                Assert.Fail("Expected a result after assignment.");
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsNull();
    }

    [Test]
    public async Task CustomParametersShouldReturnNull()
    {
        var e = new Expression("x");

        e.EvaluateParameter += (name, args) =>
        {
            if (args.HasResult)
            {
                Assert.Fail("Expected no result before assignment.");
            }
            if (name == "x")
                args.Result = null;
            if (!args.HasResult)
            {
                Assert.Fail("Expected a result after assignment.");
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsNull();
    }

    [Test]
    [Arguments("notExistingfunction")]
    [Arguments("andDoThis")]
    public async Task ShouldTreatOperatorsWithoutWhitespaceAsFunctionName(string functionName)
    {
        var expression = new Expression($"{functionName}(3.14)");
        expression.EvaluateFunction += (name, args) =>
        {
            if (name.Equals(functionName, StringComparison.OrdinalIgnoreCase))
                args.Result = 1;
        };

        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(1);
    }

    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctionsWithSameName()
    {
        var e = new Expression("SecretOperation(3, 6) + SecretOperation(1, 2)");

        var d = new Dictionary<string, int>();

        e.EvaluateFunction += (name, args) =>
        {
            var id = args.Id.ToString();
            if (name == "SecretOperation")
            {
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

                args.Result = (int)args.Parameters[0].Evaluate(args.CancellationToken) +
                    (int)args.Parameters[1].Evaluate(args.CancellationToken);
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(12);
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(12);

        await Assert.That(d.FirstOrDefault().Value).IsEqualTo(2);
    }

    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctionsWithEffects()
    {
        var e = new Expression("Repeat([value] > 10, 3)");

        var times = new Dictionary<string, int>();

        e.EvaluateFunction += (name, args) =>
        {
            var id = name;
            if (name == "Repeat")
            {
                var t = (int)args.Parameters[1].Evaluate(args.CancellationToken) - 1;
                var r = (bool)args.Parameters[0].Evaluate(args.CancellationToken);
                if (r && id != null)
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

                args.Result = r && times[id] == 0;
            }
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
}
