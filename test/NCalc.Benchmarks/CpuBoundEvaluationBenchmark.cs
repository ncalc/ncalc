using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using NCalc.Factories;

namespace NCalc.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[RankColumn]
[CategoriesColumn]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CpuBoundEvaluationBenchmark
{
    private Expression Expression { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var logicalExpression = LogicalExpressionFactory.Create(
            "if(is_valid(a), calc(a, b), 0) + if(is_valid(c), calc(c, d), 0) + if(a > b, calc(a, c), calc(b, d))",
            ExpressionOptions.NoCache);
        var expression = new Expression(logicalExpression, ExpressionOptions.NoCache)
        {
            Parameters =
            {
                ["a"] = 42,
                ["b"] = 12,
                ["c"] = 7,
                ["d"] = 3
            }
        };

        expression.Functions["is_valid"] = args => (int)args.Evaluate(0)! > 0;
        expression.Functions["calc"] = args => (int)args.Evaluate(0)! * 2 + (int)args.Evaluate(1)!;

        Expression = expression;
    }

    [Benchmark(Baseline = true)]
    public object SyncEvaluate()
    {
        return Expression.Evaluate()!;
    }

    [Benchmark]
    public async ValueTask<object> AsyncEvaluate()
    {
        return (await Expression.EvaluateAsync())!;
    }
}
