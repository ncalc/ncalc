using BenchmarkDotNet.Running;
using NCalc.Benchmarks;

BenchmarkRunner.Run<LogicalExpressionFactoryBenchmark>(null, args);
BenchmarkRunner.Run<SimpleEvaluationBenchmark>(null, args);
BenchmarkRunner.Run<CpuBoundEvaluationBenchmark>(null, args);
BenchmarkRunner.Run<EvaluateVsLambdaBenchmark>(null, args);
BenchmarkRunner.Run<NCalcVsDataTableBenchmark>(null, args);
