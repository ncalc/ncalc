using BenchmarkDotNet.Attributes;
using NCalc.Domain;
using NCalc.Parser;

namespace NCalc.Benchmarks
{
    [MemoryDiagnoser]
    public class WithoutCompilationBenchmark
    {
        private const string Expression = "(a + b * 2) > 10 and value in (20, 30, 40)";

        [GlobalSetup]
        public void Setup() => AppContext.SetSwitch("NCalc.EnableParlotParserCompilation", false);

        [Benchmark]
        public LogicalExpression ParseWithoutCompilation() =>
            LogicalExpressionParser.Parse(new LogicalExpressionParserContext(Expression, ExpressionOptions.NoCache));
    }

    [MemoryDiagnoser]
    public class WithCompilationBenchmark
    {
        private const string Expression = "(a + b * 2) > 10 and value in (20, 30, 40)";

        [GlobalSetup]
        public void Setup() => AppContext.SetSwitch("NCalc.EnableParlotParserCompilation", true);

        [Benchmark]
        public LogicalExpression ParseWithCompilation() =>
            LogicalExpressionParser.Parse(new LogicalExpressionParserContext(Expression, ExpressionOptions.NoCache));
    }
}
