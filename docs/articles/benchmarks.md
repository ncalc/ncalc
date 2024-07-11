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


| Method                   | Mean           | Error         | StdDev        | Median         | Rank | Gen0   | Gen1   | Allocated |
|------------------------- |---------------:|--------------:|--------------:|---------------:|-----:|-------:|-------:|----------:|
| LambdaWithoutCompilation |      0.3677 ns |     0.0183 ns |     0.0171 ns |      0.3630 ns |    1 |      - |      - |         - |
| Evaluate                 |    450.3889 ns |     1.8740 ns |     1.6612 ns |    450.0070 ns |    2 | 0.2842 | 0.0005 |    1784 B |
| LambdaWithCompilation    | 15,692.3791 ns | 2,925.1680 ns | 8,624.9239 ns | 10,760.3775 ns |    3 | 0.7935 | 0.7629 |    5024 B |

// * Hints *
Outliers
EvaluateVsLambdaBenchmark.Evaluate: Default -> 1 outlier  was  removed (460.11 ns)

# Parlot parser compilation

Parlot parser, which is used as a default parser in NCalc, supports parser compilation. It can improve the performance by 20%. You can find the benchmark results in Parlot [repository](https://github.com/sebastienros/parlot#performance).
By default, it is disabled in NCalc, you can enable it by using AppContext switch:

`AppContext.SetSwitch("NCalc.EnableParlotParserCompilation", true)`