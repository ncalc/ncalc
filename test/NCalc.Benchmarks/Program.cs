using BenchmarkDotNet.Running;
using NCalc.Benchmarks;

BenchmarkRunner.Run<LogicalExpressionFactoryBenchmark>(null, args);
BenchmarkRunner.Run<EvaluateVsLambdaBenchmark>(null, args);