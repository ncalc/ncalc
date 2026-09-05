using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using NCalc.Benchmarks;

if (args.Contains("--allocation-profile"))
{
    var config = ManualConfig.Create(DefaultConfig.Instance)
        .AddDiagnoser(new EventPipeProfiler(EventPipeProfile.GcVerbose));
    var benchmarkArgs = args.Where(arg => arg != "--allocation-profile").ToArray();
    BenchmarkRunner.Run<ParserGenerationBenchmark>(config, benchmarkArgs);
    return;
}

BenchmarkRunner.Run<ParserGenerationBenchmark>(null, args);
BenchmarkRunner.Run<SimpleEvaluationBenchmark>(null, args);
BenchmarkRunner.Run<CpuBoundEvaluationBenchmark>(null, args);
BenchmarkRunner.Run<EvaluateVsLambdaBenchmark>(null, args);
BenchmarkRunner.Run<NCalcVsDataTableBenchmark>(null, args);
