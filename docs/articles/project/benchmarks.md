# Benchmarks

The timing results below were recorded on 2026-09-05 using Parlot `2.0.0-preview-743` from the
[Parlot preview feed](https://f.feedz.io/sebastienros/parlot/nuget/index.json). The default parser is
[source-generated](architecture.md#source-generated-parser); no runtime parser compilation switch is needed.
All 28 current benchmark cases were rerun after merging `master` (`77674ff`) and porting the parser
to the current NCalc architecture, at `c7945fb`. This uses the published package without a local
override. Historical allocation data from the original PR grammar is explicitly labeled below.
The obsolete Antlr comparison was removed with the Antlr plugin; CPU-bound evaluation is now included.

To reproduce the run, install the .NET 8 and .NET 10 runtimes and run this command from the repository root:

```shell
UseSharedCompilation=false dotnet run --project test/NCalc.Benchmarks/NCalc.Benchmarks.csproj -c Release -- \
  --filter '*' --launchCount 1 --warmupCount 3 --iterationCount 15 \
  --artifacts ./BenchmarkDotNet.Artifacts
```

These are local, shared-workstation measurements, not an isolated performance-regression baseline.
Compare implementations within the same runtime; absolute timings are not directly comparable to older
results from different hardware. Compiler sharing was disabled and consumers rebuilt when switching
packages, so the compiler server could not reuse a generator loaded from a different package build.
The `UseSharedCompilation` environment variable also applies to BenchmarkDotNet's child builds.

```text
BenchmarkDotNet v0.15.8, macOS Sequoia 15.7.9 (24G830) [Darwin 24.6.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.400
  Host      : .NET 8.0.30, Arm64 RyuJIT armv8.0-a
  .NET 8.0  : .NET 8.0.30, Arm64 RyuJIT armv8.0-a
  .NET 10.0 : .NET 10.0.11, Arm64 RyuJIT armv8.0-a

IterationCount=15  LaunchCount=1  WarmupCount=3
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

## Runtime-built Parlot vs source-generated Parlot

`ParserGenerationBenchmark` compares both implementations of the **same NCalc grammar**, using the
same Parlot `2.0.0-preview-743` package, default `LogicalExpressionParserOptions`, invariant culture, comma separators,
and parse-context settings. "Runtime-built" means the normal Parlot combinator graph, not the old
optional runtime parser compilation mode.

The dynamic baseline invokes the original grammar factory through a delegate, bypassing C# call-site
interception. Reflection is used only to create that delegate during setup, never inside a measurement.
The generated factory delegate invokes the private interception wrapper, not the public parser cache.
Setup checks that the implementations
are distinct and that both produce the same serialized AST for each input.

To run only this comparison:

```shell
UseSharedCompilation=false dotnet run --project test/NCalc.Benchmarks/NCalc.Benchmarks.csproj -c Release -- \
  --filter '*ParserGenerationBenchmark*' --launchCount 1 --warmupCount 3 --iterationCount 15 \
  --artifacts ./BenchmarkDotNet.Artifacts
```

### Parsing with reused instances

Both parsers are constructed once during setup and reused. Each operation includes a new parse context
and AST construction, but **neither parser pays its factory-construction cost during parsing**.
Inputs are unchanged from the original comparison:

```text
Simple:   (3.2 < waterlevel AND 5.3 >= waterlevel)
Advanced: PageState == 'LIST' && a == 1 && customFunction() == true || in(1 + 1, 1, 2, 3) && Name == 'Sergio'
```

Times are mean +/- the half-width of the 99.9% confidence interval; speedup is dynamic mean / generated mean.

| Runtime   | Input    | Dynamic parser           | Generated parser         | Speedup | Dynamic allocated | Generated allocated |
|-----------|----------|-------------------------:|-------------------------:|--------:|------------------:|--------------------:|
| .NET 8.0  | Simple   | 2.9881 +/- 0.0194 us      | 2.0648 +/- 0.0120 us      |   1.45x |            1040 B |              1040 B |
| .NET 8.0  | Advanced | 8.3184 +/- 0.0447 us      | 6.1144 +/- 0.0487 us      |   1.36x |            2864 B |              2864 B |
| .NET 10.0 | Simple   | 2.6335 +/- 0.0076 us      | 2.1862 +/- 0.0159 us      |   1.20x |            1040 B |              1040 B |
| .NET 10.0 | Advanced | 7.6694 +/- 0.1297 us      | 6.4123 +/- 0.0281 us      |   1.20x |            2864 B |              2864 B |

Source generation reduced mean parse time by **16-31%** in these cases, even when the dynamic parser
was reused, with **identical per-parse allocations**. The upstream collection-allocation fix remains
effective with the current grammar, including the updated string and null-coalescing support.

Treat the exact speedups as indicative shared-workstation measurements, not fixed guarantees.
These results do not establish a speedup for every grammar or input. Timings and allocation totals
from before the `master` merge are not a regression baseline for this different grammar and API.

### Parser construction

These measurements construct a fresh parser with an already-created options object; they do not parse
an expression. Both factories are invoked through delegates. The dynamic factory builds the full
combinator graph, whereas the generated factory creates only a small options-bound parser instance.

| Runtime   | Dynamic construction      | Generated construction | Dynamic allocated | Generated allocated |
|-----------|--------------------------:|-----------------------:|------------------:|--------------------:|
| .NET 8.0  | 37.4129 +/- 0.4432 us      | 5.246 +/- 0.0627 ns    |          146656 B |                40 B |
| .NET 10.0 | 29.8569 +/- 0.2638 us      | 4.743 +/- 0.0630 ns    |          142336 B |                40 B |

Codegen avoids runtime graph construction and its roughly 139-143 KB allocation. These are warmed
factory-call measurements, **not process startup or first-use JIT timings**. The construction saving
must not be counted on every parse when an application caches its dynamic parser.

### Historical allocation trace: original PR grammar with preview 737

GC-verbose EventPipe traces of **preview 737, before the allocation fix**, located the difference in
Parlot's collection emitters, not in NCalc's AST nodes or parser-option binding. These captures used
the original PR grammar, before the `master` migration above. The runtime versions of
[`ZeroOrMany`](https://github.com/sebastienros/parlot/blob/c6c3a472abf2ce9d34c95f25a24adb606c216341/src/Parlot/Fluent/ZeroOrMany.cs)
and [`Separated`](https://github.com/sebastienros/parlot/blob/c6c3a472abf2ce9d34c95f25a24adb606c216341/src/Parlot/Fluent/Separated.cs)
used `HybridList<T>`, which stores up to four elements inline. Their generated implementations instead
allocated a `List<T>` and its backing array.

The allocation events report these object sizes on ARM64:

| Temporary collection | Runtime representation | Generated representation | Extra bytes |
|----------------------|-----------------------:|-------------------------:|------------:|
| Operator/operand tuples, up to four items | 96 B `HybridList<T>` | 32 B list + 88 B array | 24 B |
| Function arguments, up to four items | 64 B `HybridList<T>` | 32 B list + 56 B array | 24 B |

The simple expression creates three operator collections: `3 * 24 = 72 B` extra. The advanced
expression creates seven operator collections and one argument collection: `7 * 24 + 24 = 192 B`
extra. Both paths also allocate 48-byte enumerators while folding operator lists; those are common
costs, not the source of the difference.

Generated allocation stacks led from `List<T>.AddWithResize` into the generated repetition helpers.
Dynamic stacks led into `ZeroOrMany.Parse` / `Separated.Parse` and `HybridList<T>`. The four analyzed
.NET 10 traces each contain one million measured operations and report zero lost events. Analysis
includes only the benchmark thread between `BenchmarkDotNet.EngineEventSource` actual-workload
events 15 and 16, excluding setup, warmup, and harness overhead.

To capture allocation traces for the currently referenced package without changing the normal
benchmark configuration (the historical stacks above require preview 737):

```shell
UseSharedCompilation=false dotnet build test/NCalc.Benchmarks/NCalc.Benchmarks.csproj -c Release
UseSharedCompilation=false dotnet test/NCalc.Benchmarks/bin/Release/net8.0/NCalc.Benchmarks.dll \
  --allocation-profile --filter '*ParserGenerationBenchmark.Parse*' \
  --launchCount 1 --warmupCount 3 --iterationCount 1 \
  --invocationCount 1000000 --unrollFactor 1 \
  --artifacts ./BenchmarkDotNet.Artifacts/allocations
```

The opt-in profiler uses `EventPipeProfile.GcVerbose` in an extra profiling run and preserves
`.nettrace` files. The single-iteration configuration above is for allocation attribution only;
do not use its timing estimates as performance results. Allocation ticks are sampled, so per-type
byte estimates are approximate; the totals in the comparison tables come from `MemoryDiagnoser`.

### Published allocation fix: preview 743

[sebastienros/parlot#335](https://github.com/sebastienros/parlot/pull/335), merged and published in
`2.0.0-preview-743`, makes generated `ZeroOrMany`,
`OneOrMany`, and `Separated` use the same `HybridList<T>` as runtime parsers, returning the underlying
`List<T>` after growth beyond four elements. This fixes the collector allocation difference without
rewriting the NCalc grammar.

On the original PR grammar, the published-package measurements confirmed exact allocation parity on
both .NET 8 and .NET 10. This historical comparison predates the `master` merge: the before-fix column
is preview 737 and the after-fix column is preview 743, with no grammar change between those two runs:

| Input    | Dynamic parser | Generated, before fix | Generated, after fix | Extra allocation removed |
|----------|---------------:|----------------------:|---------------------:|-------------------------:|
| Simple   |         1040 B |                1112 B |               1040 B |                     72 B |
| Advanced |         2776 B |                2968 B |               2776 B |                    192 B |

The current grammar's results above independently confirm parity at **1040 B/simple** and
**2864 B/advanced** with preview 743. The advanced total differs from this historical table because
the `master` migration changed the grammar and AST implementation; it is not extra codegen allocation.

## CPU-bound synchronous vs asynchronous evaluation

This evaluates the same pre-parsed expression and synchronous custom functions through the synchronous
and asynchronous evaluation APIs. It is a CPU-bound workload, not a comparison of asynchronous I/O.

| Method        | Runtime   | Mean     | Error   | StdDev  | Rank | Gen0   | Gen1   | Allocated |
|---------------|-----------|---------:|--------:|--------:|-----:|-------:|-------:|----------:|
| SyncEvaluate  | .NET 10.0 | 482.6 ns | 4.71 ns | 4.40 ns |    1 | 0.3204 |      - |   2.63 KB |
| AsyncEvaluate | .NET 10.0 | 759.7 ns | 5.78 ns | 5.40 ns |    2 | 0.5016 | 0.0010 |    4.1 KB |
| SyncEvaluate  | .NET 8.0  | 575.7 ns | 2.92 ns | 2.59 ns |    1 | 0.3204 |      - |   2.63 KB |
| AsyncEvaluate | .NET 8.0  | 832.0 ns | 9.46 ns | 8.85 ns |    2 | 0.5016 | 0.0010 |    4.1 KB |

## Evaluate vs Lambda

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

## NCalc vs DataTable

[DataTable.Compute](https://learn.microsoft.com/en-us/dotnet/api/system.data.datatable.compute)
provides expression evaluation in .NET without a third-party library.

| Method            | Runtime   | Mean       | Error    | StdDev   | Rank | Gen0   | Allocated |
|-------------------|-----------|-----------:|---------:|---------:|-----:|-------:|----------:|
| EvaluateNCalc     | .NET 10.0 |   353.2 ns |  2.52 ns |  2.23 ns |    1 | 0.1421 |   1.16 KB |
| EvaluateNCalc     | .NET 8.0  |   417.7 ns |  7.27 ns |  6.80 ns |    2 | 0.1421 |   1.16 KB |
| EvaluateDataTable | .NET 10.0 | 1,587.7 ns |  8.34 ns |  7.80 ns |    3 | 0.6714 |   5.58 KB |
| EvaluateDataTable | .NET 8.0  | 1,838.8 ns | 19.62 ns | 17.40 ns |    4 | 0.6714 |   5.58 KB |

## Simple evaluation

This measures evaluation of a pre-parsed expression with a parameter and an `EvaluateParameter` handler.

| Method           | Runtime   | Mean     | Error   | StdDev  | Rank | Gen0   | Allocated |
|------------------|-----------|---------:|--------:|--------:|-----:|-------:|----------:|
| SimpleEvaluation | .NET 10.0 | 65.96 ns | 0.343 ns | 0.286 ns |    1 | 0.0545 |     456 B |
| SimpleEvaluation | .NET 8.0  | 70.98 ns | 1.070 ns | 1.001 ns |    2 | 0.0545 |     456 B |