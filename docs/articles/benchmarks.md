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

> BenchmarkDotNet v0.15.4, Linux Ubuntu 24.04.3 LTS (Noble Numbat)  
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores  
.NET SDK 9.0.306  
  [Host]     : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3  
  DefaultJob : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3  


| Method                   | Mean      | Error     | StdDev    | Rank | Gen0   | Gen1   | Allocated |
|------------------------- |----------:|----------:|----------:|-----:|-------:|-------:|----------:|
| SimpleParlotExpression   |  7.743 us | 0.0074 us | 0.0062 us |    1 | 0.0610 |      - |   1.16 KB |
| SimpleAntlrExpression    | 11.548 us | 0.0869 us | 0.0812 us |    2 | 0.8087 | 0.0305 |  13.38 KB |
| AdvancedParlotExpression | 19.621 us | 0.0499 us | 0.0442 us |    3 | 0.1831 |      - |   3.08 KB |
| AdvancedAntlrExpression  | 34.699 us | 0.1335 us | 0.1183 us |    4 | 2.3193 | 0.2441 |  38.58 KB |

## Evaluate vs Lambda

This showcase how lambda expressions are faster than manually evaluating the expression and why you should cache the
lambda instead of manually compiling everytime.


> BenchmarkDotNet v0.15.4, Linux Ubuntu 24.04.3 LTS (Noble Numbat)  
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores  
.NET SDK 9.0.306  
  [Host]     : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3  
  DefaultJob : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3  


| Method                   | Mean        | Error     | StdDev    | Rank | Gen0   | Allocated |
|------------------------- |------------:|----------:|----------:|-----:|-------:|----------:|
| LambdaWithoutCompilation |   0.3785 ns | 0.0050 ns | 0.0047 ns |    1 |      - |         - |
| LambdaWithCompilation    | 759.5692 ns | 2.5919 ns | 2.2976 ns |    2 | 0.0248 |     424 B |
| Evaluate                 | 768.3650 ns | 8.9148 ns | 8.3389 ns |    2 | 0.1030 |    1728 B |

## NCalc vs DataTable

[DataTable.Compute](https://learn.microsoft.com/en-us/dotnet/api/system.data.datatable.compute) it's the only way to evaluate expressions at .NET without a third-party library.

> BenchmarkDotNet v0.15.4, Linux Ubuntu 24.04.3 LTS (Noble Numbat)  
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores  
.NET SDK 9.0.306  
  [Host]     : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3  
  DefaultJob : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3  


| Method            | Mean     | Error     | StdDev    | Rank | Gen0   | Allocated |
|------------------ |---------:|----------:|----------:|-----:|-------:|----------:|
| EvaluateNCalc     | 1.748 us | 0.0174 us | 0.0154 us |    1 | 0.1431 |   2.36 KB |
| EvaluateDataTable | 4.409 us | 0.0305 us | 0.0238 us |    2 | 0.3357 |   5.58 KB |

# Parlot parser compilation

Parlot parser, which is used as a default parser in NCalc, supports parser compilation. It can improve the performance by 20%. You can find the benchmark results in Parlot [repository](https://github.com/sebastienros/parlot#performance).
By default, it is disabled in NCalc, you can enable it by using AppContext switch:

`AppContext.SetSwitch("NCalc.EnableParlotParserCompilation", true)`