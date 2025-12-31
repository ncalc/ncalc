using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.Logging;
using NCalc.Antlr;
using NCalc.Domain;
using NCalc.Factories;

namespace NCalc.Benchmarks;

[CategoriesColumn]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class LogicalExpressionFactoryBenchmark
{
#pragma warning disable CA1859
    private ILogicalExpressionFactory AntlrFactory { get; set; }
#pragma warning restore CA1859
    private ILogicalExpressionFactory ParlotFactory { get; set; }

    private const string SimpleExpression = "(3.2 < waterlevel AND 5.3 >= waterlevel)";

    private const string AdvancedExpression =
        "PageState == 'LIST' && a == 1 && customFunction() == true || in(1 + 1, 1, 2, 3) && Name == 'Sergio'";

    [GlobalSetup]
    public void Setup()
    {
        AntlrFactory = new AntlrLogicalExpressionFactory();
        ParlotFactory =
            new LogicalExpressionFactory(LoggerFactory.Create(_ => { }).CreateLogger<LogicalExpressionFactory>());
    }

    [Benchmark]
    public LogicalExpression SimpleAntlrExpression()
    {
        return AntlrFactory.Create(SimpleExpression);
    }

    [Benchmark]
    public LogicalExpression SimpleParlotExpression()
    {
        return ParlotFactory.Create(SimpleExpression);
    }

    [Benchmark]
    public LogicalExpression AdvancedAntlrExpression()
    {
        return AntlrFactory.Create(AdvancedExpression);
    }

    [Benchmark]
    public LogicalExpression AdvancedParlotExpression()
    {
        return ParlotFactory.Create(AdvancedExpression);
    }
}