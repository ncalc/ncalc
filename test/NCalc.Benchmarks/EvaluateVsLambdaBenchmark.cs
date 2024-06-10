using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

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
        var expression = new Expression("(1 + a == 5 + b) == (42 == answer)")
        {
            Parameters =
            {
                ["a"] = 2,
                ["b"] = -2,
                ["answer"] = 42
            }
        };

        Expression.CacheEnabled = false;
        
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