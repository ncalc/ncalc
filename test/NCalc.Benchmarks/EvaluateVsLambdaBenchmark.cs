using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using NCalc.Factories;

namespace NCalc.Benchmarks;

[RankColumn]
[CategoriesColumn]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class EvaluateVsLambdaBenchmark
{
    private Expression Expression { get; set; }
    private Func<bool> LambdaExpression { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var logicalExpression = LogicalExpressionFactory.Create("(1 + a == 5 + b) == (42 == answer)");
        var expression = new Expression(logicalExpression, ExpressionOptions.NoCache)
        {
            Parameters =
            {
                ["a"] = 2,
                ["b"] = -2,
                ["answer"] = 42
            }
        };

        Expression = expression;
        LambdaExpression = expression.ToLambda<bool>();
    }

    [Benchmark]
    public bool Evaluate()
    {
        return (bool)Expression.Evaluate()!;
    }

    [Benchmark]
    public bool LambdaWithCompilation()
    {
        return Expression.ToLambda<bool>()();
    }

    [Benchmark]
    public bool LambdaWithoutCompilation()
    {
        return LambdaExpression();
    }
}