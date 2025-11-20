using BenchmarkDotNet.Running;
using NCalc.Benchmarks;

BenchmarkRunner.Run<LogicalExpressionFactoryBenchmark>(null, args);
BenchmarkRunner.Run<SimpleEvaluationBenchmark>(null, args);
BenchmarkRunner.Run<EvaluateVsLambdaBenchmark>(null, args);
BenchmarkRunner.Run<NCalcVsDataTableBenchmark>(null, args);
BenchmarkRunner.Run<WithoutCompilationBenchmark>(null, args);
BenchmarkRunner.Run<WithCompilationBenchmark>(null, args);