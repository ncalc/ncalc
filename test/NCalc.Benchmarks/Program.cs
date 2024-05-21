using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace NCalc.Benchmarks;

public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<NCalcBenchmark>();
    }
}