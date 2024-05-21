extern alias CsProjVersion;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using CsProjExpression = CsProjVersion::NCalc.Expression;
using CsProjDomain = CsProjVersion::NCalc.Handlers;
using NuGetExpression = NCalc.Expression;
using NuGetDomain = NCalc;

namespace NCalc.Benchmarks;

[Config(typeof(Config))]
[RankColumn]
[CategoriesColumn]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class NCalcBenchmark
{
    private class Config : ManualConfig
    {
        public Config()
        {
            var job = Job.ShortRun
                .WithToolchain(InProcessEmitToolchain.Instance);
            AddJob(job.WithRuntime(ClrRuntime.Net462));
            AddJob(job.WithRuntime(CoreRuntime.Core80));
        }
    }
    [BenchmarkCategory("SimpleExpression"), Benchmark]
    public object? CsProj_SimpleExpression()
    {
        string expression =
            "1 - ( 3 + 2.5 ) * 4 - 1 / 2 + 1 - ( 3 + 2.5 ) * 4 - 1 / 2 + 1 - ( 3 + 2.5 ) * 4 - 1 / 2";

        CsProjExpression.CacheEnabled = false;
        var expr = new CsProjExpression(expression);

        return expr.Evaluate();
    }


    [BenchmarkCategory("SimpleExpression"), Benchmark]
    public object? NuGet_SimpleExpression()
    {
        string expression =
            "1 - ( 3 + 2.5 ) * 4 - 1 / 2 + 1 - ( 3 + 2.5 ) * 4 - 1 / 2 + 1 - ( 3 + 2.5 ) * 4 - 1 / 2";

        NuGetExpression.CacheEnabled = false;
        var expr = new NuGetExpression(expression);

        return expr.Evaluate();
    }

    [BenchmarkCategory("EvaluateCustomFunction"), Benchmark]
    public object? CsProj_EvaluateCustomFunction()
    {
        CsProjExpression.CacheEnabled = false;
        var expr = new CsProjExpression("SecretOperation(3, 6)");
        expr.EvaluateFunction += delegate(string name, CsProjDomain.FunctionArgs args)
        {
            if (name == "SecretOperation")
                args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
        };

        return expr.Evaluate();
    }

    [BenchmarkCategory("EvaluateCustomFunction"), Benchmark]
    public object? NuGet_EvaluateCustomFunction()
    {
        NuGetExpression.CacheEnabled = false;
        var expr = new NuGetExpression("SecretOperation(3, 6)");
        expr.EvaluateFunction += delegate(string name, NuGetDomain.FunctionArgs args)
        {
            if (name == "SecretOperation")
                args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
        };

        return expr.Evaluate();
    }

    [BenchmarkCategory("EvaluateParameters"), Benchmark]
    public object? CsProj_EvaluateParameters()
    {
        CsProjExpression.CacheEnabled = false;
        var expr = new CsProjExpression("Round(Pow([Pi], 2) + Pow([Pi2], 2) + [X], 2)");

        expr.Parameters["Pi2"] = new CsProjExpression("Pi * [Pi]");
        expr.Parameters["X"] = 10;

        expr.EvaluateParameter += delegate(string name, CsProjDomain.ParameterArgs args)
        {
            if (name == "Pi")
                args.Result = 3.14;
        };

        return expr.Evaluate();
    }

    [BenchmarkCategory("EvaluateParameters"), Benchmark]
    public object? NuGet_EvaluateParameters()
    {
        NuGetExpression.CacheEnabled = false;
        var expr = new NuGetExpression("Round(Pow([Pi], 2) + Pow([Pi2], 2) + [X], 2)");

        expr.Parameters["Pi2"] = new NuGetExpression("Pi * [Pi]");
        expr.Parameters["X"] = 10;

        expr.EvaluateParameter += delegate(string name, NuGetDomain.ParameterArgs args)
        {
            if (name == "Pi")
                args.Result = 3.14;
        };

        return expr.Evaluate();
    }
}