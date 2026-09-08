# Benchmarks

The parser comparison was recorded on 2026-09-08 using published Parlot and Parlot.SourceGenerator
`2.0.0-preview-763`, without local package overrides. All eight parser cases were rerun from the
standalone-parser migration working tree based on `570cc7d`. The default parser is
[source-generated](architecture.md#source-generated-parser), with no runtime Parlot dependency.
The separate evaluation tables below retain their earlier measurements and are explicitly labeled.

To reproduce the parser comparison, install the .NET 8 and .NET 10 runtimes and run this command
from the repository root:

```shell
UseSharedCompilation=false dotnet run --project test/NCalc.Benchmarks/NCalc.Benchmarks.csproj -c Release -- \
  --filter '*ParserGenerationBenchmark*' --launchCount 1 --warmupCount 10 --iterationCount 15 \
  --artifacts ./BenchmarkDotNet.Artifacts
```

These are local, shared-workstation measurements, not an isolated performance-regression baseline.
Compare implementations within the same runtime; absolute timings are not directly comparable to older
results from different hardware. Compiler sharing was disabled and consumers rebuilt when switching
packages, so the compiler server could not reuse a generator loaded from a different package build.
The `UseSharedCompilation` environment variable also applies to BenchmarkDotNet's child builds.
An initial three-warmup run flagged a bimodal .NET 8 advanced generated-parser result. All eight
cases were repeated with ten warmups; the table below uses that entire repeat, not selected cases
from the two runs. The repeat had no multimodal-distribution warning.

```text
BenchmarkDotNet v0.15.8, macOS Sequoia 15.7.9 (24G830) [Darwin 24.6.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.400
  Host      : .NET 8.0.30, Arm64 RyuJIT armv8.0-a
  .NET 8.0  : .NET 8.0.30, Arm64 RyuJIT armv8.0-a
  .NET 10.0 : .NET 10.0.11, Arm64 RyuJIT armv8.0-a

IterationCount=15  LaunchCount=1  WarmupCount=10
```

## Legends

```text
Mean      : Arithmetic mean of all measurements
Error     : Half of 99.9% confidence interval
StdDev    : Standard deviation of all measurements
Rank      : Relative position of current benchmark mean among all benchmarks (Arabic style)
Gen0      : GC Generation 0 collects per 1000 operations
Gen1      : GC Generation 1 collects per 1000 operations
Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
1 us      : 1 Microsecond (0.000001 sec)
1 ns      : 1 Nanosecond (0.000000001 sec)
```

## Fluent vs source-generated parsing

`ParserGenerationBenchmark` compares both implementations of the **same NCalc grammar**, using the
same published Parlot version, default `LogicalExpressionParserOptions`, invariant culture, comma
separators, and `CancellationToken.None`. "Fluent" means the normal runtime-built Parlot combinator
graph, not the old optional runtime parser compilation mode.

The benchmark project links the production `.parlot.cs` grammar and its normal C# conversion helpers.
`PARLOT_FLUENT` changes only their containing class and excludes generator attributes, so the Fluent
baseline does not use a copied grammar. Only the benchmark project references the Parlot runtime.

The generated side calls the production `LogicalExpressionParser.Parse` entrypoint directly.
There are no factory delegates, interceptors, or reflection in the measured operations. Setup
checks that the baseline is a Fluent graph, the production parser assembly has no Parlot reference,
and both implementations produce the same serialized AST for each input.

The Fluent graph is constructed once during setup and reused. The generated implementation has no
parser instance to construct. Each operation includes a fresh parsing context and AST construction;
**Fluent graph construction is excluded from the parsing baseline**.
Inputs are unchanged from the original comparison:

```text
Simple:   (3.2 < waterlevel AND 5.3 >= waterlevel)
Advanced: PageState == 'LIST' && a == 1 && customFunction() == true || in(1 + 1, 1, 2, 3) && Name == 'Sergio'
```

Times are mean +/- the half-width of the 99.9% confidence interval; speedup is Fluent mean / generated mean.

| Runtime   | Input    | Fluent parser            | Generated parser         | Speedup | Fluent allocated | Generated allocated |
|-----------|----------|-------------------------:|-------------------------:|--------:|-----------------:|--------------------:|
| .NET 8.0  | Simple   | 3.1106 +/- 0.0283 us      | 1.9263 +/- 0.0654 us      |   1.61x |           1032 B |              1048 B |
| .NET 8.0  | Advanced | 8.3904 +/- 0.0626 us      | 5.5174 +/- 0.0590 us      |   1.52x |           2856 B |              2872 B |
| .NET 10.0 | Simple   | 2.6384 +/- 0.0294 us      | 1.8535 +/- 0.0665 us      |   1.42x |           1032 B |              1048 B |
| .NET 10.0 | Advanced | 7.3103 +/- 0.0270 us      | 4.9326 +/- 0.0625 us      |   1.48x |           2856 B |              2872 B |

Source generation reduced mean parse time by **approximately 30-38%** in these cases, even with a
reused Fluent graph. The direct generated parser allocates **16 B more per parse** on this ARM64
runtime: its per-call context stores the options and culture references that the Fluent graph
captures during setup. Allocation parity from the previous parser-instance model no longer applies.

Treat the exact speedups as indicative shared-workstation measurements, not fixed guarantees.
These results do not establish a speedup for every grammar or input. They measure parsing, not
process startup or first-use JIT.

The former construction comparison is removed because the standalone generated parser has no
runtime factory or options-bound parser instance. This is not a claim of zero startup/JIT cost,
and graph-construction savings must not be counted on each parse of a reused Fluent graph.

## Earlier evaluation results (2026-09-05)

The following 16 evaluation cases were measured at `c7945fb` with Parlot `2.0.0-preview-743`, on the
same hardware and runtime versions, using one launch, three warmups, and fifteen measured iterations.
They were **not rerun for the standalone-parser migration** and should not be read as preview 763
measurements. Use `--filter '*'` to run the whole current benchmark suite.

### CPU-bound synchronous vs asynchronous evaluation

This evaluates the same pre-parsed expression and synchronous custom functions through the synchronous
and asynchronous evaluation APIs. It is a CPU-bound workload, not a comparison of asynchronous I/O.

| Method        | Runtime   | Mean     | Error   | StdDev  | Rank | Gen0   | Gen1   | Allocated |
|---------------|-----------|---------:|--------:|--------:|-----:|-------:|-------:|----------:|
| SyncEvaluate  | .NET 10.0 | 482.6 ns | 4.71 ns | 4.40 ns |    1 | 0.3204 |      - |   2.63 KB |
| AsyncEvaluate | .NET 10.0 | 759.7 ns | 5.78 ns | 5.40 ns |    2 | 0.5016 | 0.0010 |    4.1 KB |
| SyncEvaluate  | .NET 8.0  | 575.7 ns | 2.92 ns | 2.59 ns |    1 | 0.3204 |      - |   2.63 KB |
| AsyncEvaluate | .NET 8.0  | 832.0 ns | 9.46 ns | 8.85 ns |    2 | 0.5016 | 0.0010 |    4.1 KB |

### Evaluate vs Lambda

This compares evaluation with compiling a lambda on each call and reusing a compiled lambda.
The expression is parsed during setup, so these measurements do not include parsing.

| Method                   | Runtime   | Mean                    | Error      | StdDev     | Rank | Gen0   | Allocated |
|--------------------------|-----------|------------------------:|-----------:|-----------:|-----:|-------:|----------:|
| LambdaWithoutCompilation | .NET 10.0 | Below resolution        |          - |          - |    1 |      - |         - |
| LambdaWithoutCompilation | .NET 8.0  | Below resolution        |          - |          - |    1 |      - |         - |
| Evaluate                 | .NET 10.0 |             210.7546 ns |  0.9084 ns |  0.8497 ns |    2 | 0.1223 |    1024 B |
| Evaluate                 | .NET 8.0  |             232.7171 ns |  2.5935 ns |  2.4259 ns |    3 | 0.1223 |    1024 B |
| LambdaWithCompilation    | .NET 10.0 |             271.6130 ns |  2.6814 ns |  2.3770 ns |    4 | 0.0525 |     440 B |
| LambdaWithCompilation    | .NET 8.0  |             294.3329 ns |  2.7965 ns |  2.6159 ns |    5 | 0.0525 |     440 B |

BenchmarkDotNet reported `0.0000 ns` for both cached-lambda jobs after subtracting harness overhead.
This means the work was too small to resolve in this run, not that invoking a lambda is free.

### NCalc vs DataTable

[DataTable.Compute](https://learn.microsoft.com/en-us/dotnet/api/system.data.datatable.compute)
provides expression evaluation in .NET without a third-party library.

| Method            | Runtime   | Mean       | Error    | StdDev   | Rank | Gen0   | Allocated |
|-------------------|-----------|-----------:|---------:|---------:|-----:|-------:|----------:|
| EvaluateNCalc     | .NET 10.0 |   353.2 ns |  2.52 ns |  2.23 ns |    1 | 0.1421 |   1.16 KB |
| EvaluateNCalc     | .NET 8.0  |   417.7 ns |  7.27 ns |  6.80 ns |    2 | 0.1421 |   1.16 KB |
| EvaluateDataTable | .NET 10.0 | 1,587.7 ns |  8.34 ns |  7.80 ns |    3 | 0.6714 |   5.58 KB |
| EvaluateDataTable | .NET 8.0  | 1,838.8 ns | 19.62 ns | 17.40 ns |    4 | 0.6714 |   5.58 KB |

### Simple evaluation

This measures evaluation of a pre-parsed expression with a parameter and an `EvaluateParameter` handler.

| Method           | Runtime   | Mean     | Error   | StdDev  | Rank | Gen0   | Allocated |
|------------------|-----------|---------:|--------:|--------:|-----:|-------:|----------:|
| SimpleEvaluation | .NET 10.0 | 65.96 ns | 0.343 ns | 0.286 ns |    1 | 0.0545 |     456 B |
| SimpleEvaluation | .NET 8.0  | 70.98 ns | 1.070 ns | 1.001 ns |    2 | 0.0545 |     456 B |