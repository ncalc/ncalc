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

BenchmarkDotNet v0.13.12, Ubuntu 22.04.4 LTS (Jammy Jellyfish)
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET SDK 8.0.303
  [Host]     : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2


| Method                   | Mean      | Error     | StdDev    | Rank | Gen0   | Allocated |
|------------------------- |----------:|----------:|----------:|-----:|-------:|----------:|
| SimpleParlotExpression   |  7.117 μs | 0.0553 μs | 0.0490 μs |    1 |      - |   1.02 KB |
| SimpleAntlrExpression    | 11.420 μs | 0.0582 μs | 0.0454 μs |    2 | 0.1526 |  13.38 KB |
| AdvancedParlotExpression | 19.016 μs | 0.0922 μs | 0.0817 μs |    3 | 0.0305 |   2.51 KB |
| AdvancedAntlrExpression  | 34.538 μs | 0.1062 μs | 0.0887 μs |    4 | 0.4272 |  38.47 KB |

## Evaluate vs Lambda

This showcase how lambda expressions are faster than manually evaluating the expression and why you should cache the
lambda instead of manually compiling everytime.


BenchmarkDotNet v0.13.12, Ubuntu 22.04.4 LTS (Jammy Jellyfish)
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET SDK 8.0.303
  [Host]     : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2


| Method                   | Mean           | Error       | StdDev      | Rank | Gen0   | Allocated |
|------------------------- |---------------:|------------:|------------:|-----:|-------:|----------:|
| LambdaWithoutCompilation |      0.3045 ns |   0.0123 ns |   0.0115 ns |    1 |      - |         - |
| Evaluate                 |    705.6432 ns |   2.7688 ns |   2.1617 ns |    2 | 0.0200 |    1680 B |
| LambdaWithCompilation    | 22,926.9179 ns | 161.2515 ns | 142.9452 ns |    3 |      - |    4992 B |


## NCalc vs DataTable

[DataTable.Compute](https://learn.microsoft.com/en-us/dotnet/api/system.data.datatable.compute) it's the only way to evaluate expressions at .NET without a third-party library.

BenchmarkDotNet v0.14.0, Ubuntu 24.04 LTS (Noble Numbat)
AMD Ryzen 5 3600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.108
[Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

| Method            | Mean     | Error     | StdDev    | Median   | Rank | Gen0   | Allocated |
|------------------ |---------:|----------:|----------:|---------:|-----:|-------:|----------:|
| EvaluateNCalc     | 1.463 us | 0.0292 us | 0.0671 us | 1.434 us |    1 | 0.2766 |   2.27 KB |
| EvaluateDataTable | 4.438 us | 0.0295 us | 0.0276 us | 4.440 us |    2 | 0.6790 |   5.58 KB |

# Parlot parser compilation

Parlot parser, which is used as a default parser in NCalc, supports parser compilation. It can improve the performance by 20%. You can find the benchmark results in Parlot [repository](https://github.com/sebastienros/parlot#performance).
By default, it is disabled in NCalc, you can enable it by using AppContext switch:

`AppContext.SetSwitch("NCalc.EnableParlotParserCompilation", true)`