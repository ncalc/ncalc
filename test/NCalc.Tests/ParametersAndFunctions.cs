using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Parser;

namespace NCalc.Tests;

[Property("Category", "Parameters and Functions")]
public class ParametersAndFunctions
{
    [Test]
    public async Task ShouldInitializeExpressionContextDictionaries()
    {
        var context = new ExpressionContext(
            new Dictionary<string, object> { ["staticValue"] = 1 },
            new Dictionary<string, ExpressionParameter> { ["dynamicValue"] = _ => 2 },
            new Dictionary<string, ExpressionFunction> { ["Custom"] = _ => 3 },
            new Dictionary<string, AsyncExpressionFunction> { ["AsyncCustom"] = async _ => await Task.FromResult(4) },
            new Dictionary<string, AsyncExpressionParameter> { ["asyncDynamicValue"] = async _ => await Task.FromResult((object)5) });

        var expression = new Expression("staticValue + dynamicValue + asyncDynamicValue + Custom() + AsyncCustom()", context);
        var result = await expression.EvaluateAsync(CancellationToken.None);

        await Assert.That(result).IsEqualTo(15);
    }

    [Test]
    public async Task ShouldCreateExpressionConfigurationHelperOptionsFromExpressionOptions()
    {
        var configuration = ExpressionConfiguration.FromOptions(
            ExpressionOptions.DecimalAsDefault | ExpressionOptions.CaseInsensitiveStringComparer);

        await Assert.That(configuration.Parsing.FloatingPointNumberType).IsEqualTo(FloatingPointNumberType.Decimal);
        await Assert.That(configuration.Evaluation.Math.FloatingPointNumberType).IsEqualTo(FloatingPointNumberType.Decimal);
        await Assert.That(configuration.Evaluation.Math.IntegerNumberType).IsEqualTo(IntegerNumberType.Int32);
        await Assert.That(configuration.Evaluation.StringComparer.Equals("a", "A")).IsTrue();
    }

    [Test]
    public async Task ShouldCreateExpressionConfigurationHelperOptionsFromUpdatedOptions()
    {
        const ExpressionOptions options = ExpressionOptions.DecimalAsDefault | ExpressionOptions.OrdinalStringComparer;
        var configuration = ExpressionConfiguration.FromOptions(options);

        await Assert.That(configuration.Parsing.FloatingPointNumberType).IsEqualTo(FloatingPointNumberType.Decimal);
        await Assert.That(configuration.Evaluation.Math.FloatingPointNumberType).IsEqualTo(FloatingPointNumberType.Decimal);
        await Assert.That(configuration.Evaluation.Math.IntegerNumberType).IsEqualTo(IntegerNumberType.Int32);
        await Assert.That(configuration.Evaluation.StringComparer.Compare("a", "A")).IsGreaterThan(0);
    }

    [Test]
    public async Task ShouldSplitDecimalAndLongExpressionOptions()
    {
        var configuration = ExpressionConfiguration.FromOptions(
            ExpressionOptions.DecimalAsDefault | ExpressionOptions.LongAsDefault);

        await Assert.That(configuration.Parsing.FloatingPointNumberType).IsEqualTo(FloatingPointNumberType.Decimal);
        await Assert.That(configuration.Parsing.IntegerNumberType).IsEqualTo(IntegerNumberType.Int64);
        await Assert.That(configuration.Evaluation.Math.FloatingPointNumberType).IsEqualTo(FloatingPointNumberType.Decimal);
        await Assert.That(configuration.Evaluation.Math.IntegerNumberType).IsEqualTo(IntegerNumberType.Int64);
    }

    [Test]
    public async Task ShouldCreateExpressionWithConfigurationAndContext()
    {
        var context = new ExpressionContext();
        context.Parameters["value"] = 22.5;
        var configuration = ExpressionConfiguration.FromOptions(ExpressionOptions.RoundAwayFromZero);

        var expression = new Expression("Round(value, 0)", configuration, context);

        await Assert.That(configuration.Evaluation.Math.RoundAwayFromZero).IsTrue();
        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(23d);
    }

    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctions()
    {
        var e = new Expression("SecretOperation(3, 6)")
        {
            Functions =
            {
                ["SecretOperation"] = (args) => (int)args.Evaluate(0) + (int)args.Evaluate(1)
            }
        };

        await Assert.That(e).Expression().IsEqualTo(9);
    }

    [Test]
    public async Task ExpressionShouldEvaluateCustomFunctionsWithParameters()
    {
        var e = new Expression("SecretOperation([e], 6) + f")
        {
            Parameters =
            {
                ["e"] = 3,
                ["f"] = 1
            },
            Functions =
            {
                ["SecretOperation"] = (args) => (int)args.Evaluate(0) + (int)args.Evaluate(1)
            }
        };

        await Assert.That(e).Expression().IsEqualTo(10);
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

            return (int)args.Evaluate(0) + (int)args.Evaluate(1);
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
            var t = (int)args.Evaluate(1) - 1;
            var r = (bool)args.Evaluate(0);
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

        await Assert.That(e).Expression<bool>().IsFalse();

        e.Parameters["value"] = 11;
        await Assert.That(e).Expression<bool>().IsFalse();
        e.Parameters["value"] = 12;
        await Assert.That(e).Expression<bool>().IsFalse();
        e.Parameters["value"] = 13;
        await Assert.That(e).Expression<bool>().IsTrue();
        await Assert.That(times).HasSingleItem();
    }

    [Test]
    public async Task ExpressionShouldEvaluateParameters()
    {
        var e = new Expression("Round(Pow(Pi, 2) + Pow([Pi Squared], 2) + [X], 2)");

        e.Parameters["Pi Squared"] = new Expression("Pi * [Pi]");
        e.Parameters["X"] = 10;

        e.DynamicParameters["Pi"] = _ => 3.14;

        await Assert.That(e).Expression().IsEqualTo(117.07);
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

        object Expression_EvaluateFunction(FunctionData args)
        {
            counter++;
            totalCounter++;
            return 1;
        }

        await Assert.That(totalCounter).IsEqualTo(10);
    }

    [Test]
    public async Task ShouldOverrideExistingFunctions()
    {
        var e = new Expression("Round(1.99, 2)");

        await Assert.That(e).Expression().IsEqualTo(1.99d);

        e.Functions["Round"] = (_) => 3;

        await Assert.That(e).Expression().IsEqualTo(3);
    }

    [Test]
    public void ShouldHandleCustomParametersWhenNoSpecificParameterIsDefined()
    {
        var e = new Expression("Round(Pow([Pi], 2) + Pow([Pi], 2) + 10, 2)");

        e.DynamicParameters["Pi"] = _ => 3.14;
        e.Evaluate(CancellationToken.None);
    }

    [Test]
    public async Task ShouldHandleCustomFunctionsInFunctions()
    {
        var e = new Expression("if(true, func1(x) + func2(func3(y)), 0)");

        e.Functions["func1"] = (_) => 1;
        e.Functions["func2"] = (arg) => 2 * Convert.ToDouble(arg.Evaluate(0));
        e.Functions["func3"] = (arg) => 3 * Convert.ToDouble(arg.Evaluate(0));

        e.DynamicParameters["x"] = _ => 1;
        e.DynamicParameters["y"] = _ => 2;
        e.DynamicParameters["z"] = _ => 3;

        await Assert.That(e).Expression().IsEqualTo(13d);
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
    public async Task ShouldTreatOperatorsWithoutWhitespaceAsFunctionName(string functionName)
    {
        var expression = new Expression($"{functionName}(3.14)")
        {
            Functions =
            {
                [functionName] = (_) => 1
            }
        };

        await Assert.That(expression).Expression().IsEqualTo(1);
    }

    [Test]
    public async Task ShouldHandleCaseInsensitiveParameter()
    {
        var parameters = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Name", "Beatriz" }
        };

        await Assert.That("name == 'Beatriz'").Expression<bool>(new ExpressionContext
        {
            Parameters = parameters
        }).IsTrue();
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
    public async Task MaxShouldWorkWithUShortAndShortTypes()
    {
        const ushort a = ushort.MaxValue;
        const short b = short.MaxValue;

        const int expected = ushort.MaxValue;

        await Assert.That("Max([a], [b])").Expression(new Dictionary<string, object>
        {
            ["a"] = a,
            ["b"] = b
        }).IsEqualTo(expected);
    }
}
