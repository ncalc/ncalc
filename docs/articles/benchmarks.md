# Benchmarks

If you want to check the benchmarks by yourself, `git clone https://github.com/ncalc/ncalc`, open the solution and
run `NCalc.Benchmarks` at `Release` mode.

## Legends
Mean      : Arithmetic mean of all measurements
Error     : Half of 99.9% confidence interval
StdDev    : Standard deviation of all measurements
Rank      : Relative position of current benchmark mean among all benchmarks (Arabic style)
Gen0      : GC Generation 0 collects per 1000 operations
Gen1      : GC Generation 1 collects per 1000 operations
Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
1 us      : 1 Microsecond (0.000001 sec)

## Parlot vs Antlr

This is a benchmark of the default parser of NCalc v4+ vs the Antlr plugin (default parser of older versions).

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3672/23H2/2023Update/SunValley3)
13th Gen Intel Core i5-1335U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 8.0.300
[Host]     : .NET 8.0.5 (8.0.524.21615), X64 RyuJIT AVX2
DefaultJob : .NET 8.0.5 (8.0.524.21615), X64 RyuJIT AVX2


| Method                   | Mean      | Error     | StdDev    | Rank | Gen0   | Gen1   | Allocated |
|------------------------- |----------:|----------:|----------:|-----:|-------:|-------:|----------:|
| SimpleParlotExpression   |  4.994 us | 0.0624 us | 0.0553 us |    1 | 0.1678 |      - |   1.05 KB |
| SimpleAntlrExpression    |  5.475 us | 0.0829 us | 0.1598 us |    2 | 2.1744 | 0.0992 |  13.34 KB |
| AdvancedParlotExpression | 15.378 us | 0.3059 us | 0.7504 us |    3 | 0.4120 |      - |   2.58 KB |
| AdvancedAntlrExpression  | 18.031 us | 0.3508 us | 0.3110 us |    4 | 6.2561 | 0.7324 |  38.39 KB |

// * Warnings *
MultimodalDistribution
LogicalExpressionFactoryBenchmark.AdvancedParlotExpression: Default -> It seems that the distribution can have several modes (mValue = 3.18)

// * Hints *
Outliers
LogicalExpressionFactoryBenchmark.SimpleParlotExpression: Default  -> 1 outlier  was  removed, 2 outliers were detected (4.88 us, 5.29 us)
LogicalExpressionFactoryBenchmark.SimpleAntlrExpression: Default   -> 15 outliers were removed (8.46 us..9.94 us)
LogicalExpressionFactoryBenchmark.AdvancedAntlrExpression: Default -> 1 outlier  was  removed (19.17 us)


## Evaluate vs Lambda

This showcase how lambda expressions are faster than manually evaluating the expression and why you should cache the
lambda instead of manually compiling everytime.


BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3672/23H2/2023Update/SunValley3)
13th Gen Intel Core i5-1335U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 8.0.300
[Host]     : .NET 8.0.5 (8.0.524.21615), X64 RyuJIT AVX2
DefaultJob : .NET 8.0.5 (8.0.524.21615), X64 RyuJIT AVX2

| Method                   | Mean           | Error       | StdDev      | Median         | Rank | Gen0   | Gen1   | Allocated |
|------------------------- |---------------:|------------:|------------:|---------------:|-----:|-------:|-------:|----------:|
| LambdaWithoutCompilation |      0.3309 ns |   0.0132 ns |   0.0117 ns |      0.3294 ns |    1 |      - |      - |         - |
| Evaluate                 |    694.6179 ns |  51.5536 ns | 152.0071 ns |    746.4118 ns |    2 | 0.2842 |      - |    1784 B |
| LambdaWithCompilation    | 11,633.8508 ns | 231.8332 ns | 541.9027 ns | 11,606.7932 ns |    3 | 0.7935 | 0.7629 |    5024 B |

// * Warnings *
MultimodalDistribution
EvaluateVsLambdaBenchmark.Evaluate: Default -> It seems that the distribution is bimodal (mValue = 3.83)

// * Hints *
Outliers
EvaluateVsLambdaBenchmark.LambdaWithoutCompilation: Default -> 1 outlier  was  removed (1.87 ns)
