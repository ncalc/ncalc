using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using NCalc.Factories;
using FastExpressionCompiler;

namespace NCalc.Benchmarks;

/*
## Initial Results

> Seems nothing to woкry about, but check the InvokeCompiledBenchmark

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2


| Method       | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Allocated | Alloc Ratio |
|------------- |----------:|----------:|----------:|------:|--------:|-----:|----------:|------------:|
| Compiled     | 0.5119 ns | 0.0395 ns | 0.0660 ns |  1.02 |    0.19 |    1 |         - |          NA |
| CompiledFast | 0.9849 ns | 0.0472 ns | 0.0562 ns |  1.96 |    0.28 |    2 |         - |          NA |

*/
[RankColumn]
[CategoriesColumn]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class EvaluateVsLambdaBenchmark
{
    private Expression Expression { get; set; }
    private Func<bool> LambdaCompiled { get; set; }
    private Func<bool> LambdaCompiledFast { get; set; }
    private Expression<Func<bool>> LambdaExpression { get; set; }

    public static Expression BuildNCalcExpression()
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
        return expression;
    }

    [GlobalSetup]
    public void Setup()
    {
        var expression = BuildNCalcExpression();

        Expression = expression;
        LambdaExpression = expression.ToLambdaExpression<bool>();
#if DEBUG
        TestTools.AllowPrintCS = true;
        TestTools.AllowPrintIL = true;
#endif
        LambdaExpression.PrintCSharp();

        LambdaCompiled = LambdaExpression.Compile();
        LambdaCompiled.PrintIL("system");

        LambdaCompiledFast = LambdaExpression.CompileFast();
        LambdaCompiledFast.PrintIL("fec");
    }

    [Benchmark(Baseline = true)]
    public object Compile()
    {
        return LambdaExpression.Compile();
    }

    [Benchmark]
    public object CompileFast()
    {
        return LambdaExpression.CompileFast();
    }

    // [Benchmark]
    public bool Evaluate()
    {
        return (bool)Expression.Evaluate()!;
    }

    // [Benchmark]
    public bool LambdaWithCompilationFast()
    {
        return Expression.ToLambda<bool>()();
    }

    // [Benchmark]
    public bool LambdaWithoutCompilation()
    {
        return LambdaCompiled();
    }
}

/*
## Initial Results

Huh, 2x slower than compiled is bad. Will work from here.

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2


| Method       | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Allocated | Alloc Ratio |
|------------- |----------:|----------:|----------:|------:|--------:|-----:|----------:|------------:|
| Compiled     | 0.5119 ns | 0.0395 ns | 0.0660 ns |  1.02 |    0.19 |    1 |         - |          NA |
| CompiledFast | 0.9849 ns | 0.0472 ns | 0.0562 ns |  1.96 |    0.28 |    2 |         - |          NA |

*/
[RankColumn]
[CategoriesColumn]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class InvokeCompiledBenchmark
{
    private Func<bool> LambdaCompiled;
    private Func<bool> LambdaCompiledFast;

    [GlobalSetup]
    public void Setup()
    {
        var nCalcExpression = EvaluateVsLambdaBenchmark.BuildNCalcExpression();

        var lambdaExpression = nCalcExpression.ToLambdaExpression<bool>();

        LambdaCompiled = lambdaExpression.Compile();
        LambdaCompiledFast = lambdaExpression.CompileFast();
    }

    [Benchmark(Baseline = true)]
    public bool CompiledFast()
    {
        return LambdaCompiledFast();
    }

    [Benchmark]
    public bool Compiled()
    {
        return LambdaCompiled();
    }
}