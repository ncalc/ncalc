using BenchmarkDotNet.Running;

namespace NCalc.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssemblies(new[] { typeof(Program).Assembly }).Run(args);
        }
    }
}
