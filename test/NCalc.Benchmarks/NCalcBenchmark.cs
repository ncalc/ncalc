using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using NCalc.Domain;


namespace NCalc.Benchmarks
{        
    [Config(typeof(Config))]
    [RankColumn]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class NCalcBenchmark
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                var currentVersionJob = Job.ShortRun
                   .WithToolchain(InProcessEmitToolchain.Instance);
                var actualNugetVersionJob = Job.ShortRun.WithNuGet("NCalcSync")
                    .WithToolchain(InProcessEmitToolchain.Instance);

                AddJob(currentVersionJob.WithRuntime(ClrRuntime.Net462));
                AddJob(currentVersionJob.WithRuntime(CoreRuntime.Core80));

                AddJob(actualNugetVersionJob.WithRuntime(ClrRuntime.Net462));
                AddJob(actualNugetVersionJob.WithRuntime(CoreRuntime.Core80));
            }
        }

        [Benchmark]
        public object? SimpleExpression()
        {
            string expression = "1 - ( 3 + 2.5 ) * 4 - 1 / 2 + 1 - ( 3 + 2.5 ) * 4 - 1 / 2 + 1 - ( 3 + 2.5 ) * 4 - 1 / 2";

            Expression.CacheEnabled = false;
            var expr = new Expression(expression);
            
            return expr.Evaluate();
        }

        [Benchmark]
        public object? EvaluateCustomFunction()
        {
            Expression.CacheEnabled = false;
            var expr = new Expression("SecretOperation(3, 6)");
            expr.EvaluateFunction += delegate (string name, FunctionArgs args)
            {
                if (name == "SecretOperation")
                    args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
            };

            return expr.Evaluate();
        }

        [Benchmark]
        public object? EvaluateParameters()
        {
            Expression.CacheEnabled = false;
            var expr = new Expression("Round(Pow([Pi], 2) + Pow([Pi2], 2) + [X], 2)");

            expr.Parameters["Pi2"] = new Expression("Pi * [Pi]");
            expr.Parameters["X"] = 10;

            expr.EvaluateParameter += delegate (string name, ParameterArgs args)
            {
                if (name == "Pi")
                    args.Result = 3.14;
            };

           return expr.Evaluate();
        }
    }
}
