# Benchmarks

The timing results below were recorded on 2026-09-05 using Parlot `2.0.0-preview-743` from the
[Parlot preview feed](https://f.feedz.io/sebastienros/parlot/nuget/index.json). The default parser is
[source-generated](architecture.md#source-generated-parser); no runtime parser compilation switch is needed.
All 32 benchmark cases were rerun with the published package, without a local package override.
Historical allocation data from preview 737 is explicitly labeled below.

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
.NET SDK 11.0.100-rc.1.26413.103
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
same Parlot `2.0.0-preview-743` package, `ExpressionOptions.None`, invariant culture, comma separators,
and parse-context settings. "Runtime-built" means the normal Parlot combinator graph, not the old
optional runtime parser compilation mode.

The dynamic baseline invokes the original grammar factory through a delegate, bypassing C# call-site
interception. Reflection is used only to create that delegate during setup, never inside a measurement.
The generated factory goes through NCalc's intercepted entry point. Setup checks that the implementations
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
Inputs are the same simple and advanced expressions used in the Parlot-vs-Antlr comparison below.
Times are mean +/- the half-width of the 99.9% confidence interval; speedup is dynamic mean / generated mean.

| Runtime   | Input    | Dynamic parser           | Generated parser         | Speedup | Dynamic allocated | Generated allocated |
|-----------|----------|-------------------------:|-------------------------:|--------:|------------------:|--------------------:|
| .NET 8.0  | Simple   | 2.7397 +/- 0.0705 us      | 2.0293 +/- 0.0576 us      |   1.35x |            1040 B |              1040 B |
| .NET 8.0  | Advanced | 7.4108 +/- 0.8314 us      | 5.4738 +/- 0.3356 us      |   1.35x |            2776 B |              2776 B |
| .NET 10.0 | Simple   | 2.4524 +/- 0.0451 us      | 2.0117 +/- 0.0260 us      |   1.22x |            1040 B |              1040 B |
| .NET 10.0 | Advanced | 6.4434 +/- 1.0935 us      | 5.3057 +/- 0.2992 us      |   1.21x |            2776 B |              2776 B |

Source generation reduced mean parse time by **18-26%** in these cases, even when the dynamic parser
was reused, with **identical per-parse allocations**. Preview 743 removes the earlier 72-byte/simple
and 192-byte/advanced allocation penalty without changing the NCalc grammar.

The advanced-input measurements have wider confidence intervals, particularly the .NET 10 dynamic
parser; its interval overlaps the generated parser's interval. Together with the shared-workstation
conditions, this is a reason to treat the exact speedups as indicative rather than fixed guarantees.
These results do not establish a speedup for every grammar or input.

### Parser construction

These measurements construct a fresh parser with an already-created options object; they do not parse
an expression. Both factories are invoked through delegates. The dynamic factory builds the full
combinator graph, whereas the generated factory creates only a small options-bound parser instance.

| Runtime   | Dynamic construction      | Generated construction | Dynamic allocated | Generated allocated |
|-----------|--------------------------:|-----------------------:|------------------:|--------------------:|
| .NET 8.0  | 41.5050 +/- 3.4262 us      | 4.059 +/- 0.2487 ns    |          140224 B |                40 B |
| .NET 10.0 | 32.6929 +/- 1.3795 us      | 4.204 +/- 0.0719 ns    |          135960 B |                40 B |

Codegen avoids runtime graph construction and its roughly 133-137 KB allocation. These are warmed
factory-call measurements, **not process startup or first-use JIT timings**. The construction saving
must not be counted on every parse when an application caches its dynamic parser.
BenchmarkDotNet flagged a bimodal distribution for .NET 10 dynamic construction.

### Historical allocation trace: preview 737

GC-verbose EventPipe traces of **preview 737, before the allocation fix**, located the difference in
Parlot's collection emitters, not in NCalc's AST nodes or parser-option binding. The runtime versions of
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

The published-package measurements confirm exact allocation parity on both .NET 8 and .NET 10.
The before-fix column is the historical preview 737 measurement; the after-fix column is preview 743:

| Input    | Dynamic parser | Generated, before fix | Generated, after fix | Extra allocation removed |
|----------|---------------:|----------------------:|---------------------:|-------------------------:|
| Simple   |         1040 B |                1112 B |               1040 B |                     72 B |
| Advanced |         2776 B |                2968 B |               2776 B |                    192 B |

The timing comparisons above use the same published preview 743 package for both implementations.
Differences from earlier timing runs should not all be attributed to the allocation patch.

## Parlot vs Antlr

This compares the source-generated default parser with the Antlr plugin (the default parser in older
NCalc versions). Each operation parses through `ILogicalExpressionFactory`; it does not use the AST cache.

| Method                   | Runtime   | Mean      | Error     | StdDev    | Rank | Gen0   | Gen1   | Allocated |
|--------------------------|-----------|----------:|----------:|----------:|-----:|-------:|-------:|----------:|
| SimpleParlotExpression   | .NET 8.0  |  1.794 us | 0.0189 us | 0.0167 us |    1 | 0.1316 |      - |   1.09 KB |
| SimpleParlotExpression   | .NET 10.0 |  1.859 us | 0.0298 us | 0.0264 us |    1 | 0.1316 |      - |   1.09 KB |
| SimpleAntlrExpression    | .NET 10.0 |  3.936 us | 0.0211 us | 0.0197 us |    2 | 1.6327 | 0.0687 |  13.38 KB |
| SimpleAntlrExpression    | .NET 8.0  |  4.247 us | 0.0261 us | 0.0218 us |    3 | 1.6327 | 0.0687 |  13.38 KB |
| AdvancedParlotExpression | .NET 10.0 |  4.614 us | 0.0661 us | 0.0618 us |    4 | 0.3357 |      - |   2.78 KB |
| AdvancedParlotExpression | .NET 8.0  |  4.711 us | 0.0330 us | 0.0309 us |    4 | 0.3357 |      - |   2.78 KB |
| AdvancedAntlrExpression  | .NET 10.0 | 11.952 us | 0.1902 us | 0.1779 us |    5 | 4.6997 | 0.5035 |  38.45 KB |
| AdvancedAntlrExpression  | .NET 8.0  | 13.479 us | 0.1822 us | 0.1705 us |    6 | 4.7150 | 0.5188 |  38.58 KB |

## Evaluate vs Lambda

This compares evaluation with compiling a lambda on each call and reusing a compiled lambda.
The expression is parsed during setup, so these measurements do not include parsing.

| Method                   | Runtime   | Mean                    | Error      | StdDev     | Rank | Gen0   | Allocated |
|--------------------------|-----------|------------------------:|-----------:|-----------:|-----:|-------:|----------:|
| LambdaWithoutCompilation | .NET 10.0 | Below resolution        |          - |          - |    1 |      - |         - |
| LambdaWithoutCompilation | .NET 8.0  | Below resolution        |          - |          - |    1 |      - |         - |
| LambdaWithCompilation    | .NET 10.0 |             314.7639 ns | 17.1973 ns | 16.0863 ns |    2 | 0.0505 |     424 B |
| Evaluate                 | .NET 10.0 |             346.3925 ns | 13.6512 ns | 12.7693 ns |    2 | 0.2141 |    1792 B |
| Evaluate                 | .NET 8.0  |             399.7502 ns | 13.0336 ns | 11.5540 ns |    3 | 0.2141 |    1792 B |
| LambdaWithCompilation    | .NET 8.0  |             442.3133 ns | 109.9068 ns | 91.7772 ns |   3 | 0.0505 |     424 B |

BenchmarkDotNet reported `0.0000 ns` for both cached-lambda jobs after subtracting harness overhead.
This means the work was too small to resolve in this run, not that invoking a lambda is free.
The .NET 10 `Evaluate` distribution was bimodal, and .NET 8 lambda compilation had a wide confidence
interval; do not interpret small differences or cross-runtime ordering as a reliable ranking.

## NCalc vs DataTable

[DataTable.Compute](https://learn.microsoft.com/en-us/dotnet/api/system.data.datatable.compute)
provides expression evaluation in .NET without a third-party library.

| Method            | Runtime   | Mean       | Error    | StdDev   | Rank | Gen0   | Gen1   | Allocated |
|-------------------|-----------|-----------:|---------:|---------:|-----:|-------:|-------:|----------:|
| EvaluateNCalc     | .NET 10.0 |   633.3 ns | 60.62 ns | 50.62 ns |    1 | 0.2899 |      - |   2.38 KB |
| EvaluateNCalc     | .NET 8.0  |   760.5 ns | 44.84 ns | 35.01 ns |    2 | 0.2975 |      - |   2.44 KB |
| EvaluateDataTable | .NET 10.0 | 1,804.2 ns | 16.46 ns | 14.59 ns |    3 | 0.6828 | 0.0076 |   5.58 KB |
| EvaluateDataTable | .NET 8.0  | 1,948.8 ns | 48.52 ns | 45.39 ns |    4 | 0.6828 | 0.0038 |   5.58 KB |

## Simple evaluation

This measures evaluation of a pre-parsed expression with a parameter and an `EvaluateParameter` handler.

| Method           | Runtime   | Mean     | Error   | StdDev  | Rank | Gen0   | Allocated |
|------------------|-----------|---------:|--------:|--------:|-----:|-------:|----------:|
| SimpleEvaluation | .NET 8.0  | 113.4 ns |  2.31 ns | 1.80 ns |    1 | 0.0870 |     728 B |
| SimpleEvaluation | .NET 10.0 | 120.9 ns | 10.53 ns | 9.85 ns |    1 | 0.0870 |     728 B |