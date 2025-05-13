﻿using BenchmarkDotNet.Running;
using NCalc.Benchmarks;

// BenchmarkRunner.Run<LogicalExpressionFactoryBenchmark>(null, args);
// BenchmarkRunner.Run<SimpleEvaluationBenchmark>(null, args);
// BenchmarkRunner.Run<EvaluateVsLambdaBenchmark>(null, args);
BenchmarkRunner.Run<InvokeCompiledBenchmark>(null, args);

#if DEBUG

var bm = new EvaluateVsLambdaBenchmark();
bm.Setup();

#endif