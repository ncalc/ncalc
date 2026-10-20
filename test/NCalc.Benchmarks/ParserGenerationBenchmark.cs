using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using NCalc.Extensions;
using Parlot;
using Parlot.Fluent;

namespace NCalc.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory, BenchmarkLogicalGroupRule.ByJob)]
[CategoriesColumn]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ParserGenerationBenchmark
{
    private const string SimpleExpression = "(3.2 < waterlevel AND 5.3 >= waterlevel)";

    private const string AdvancedExpression =
        "PageState == 'LIST' && a == 1 && customFunction() == true || in(1 + 1, 1, 2, 3) && Name == 'Sergio'";

    private LogicalExpressionParserOptions ParserOptions { get; } = new();
    private CultureInfo ParserCulture { get; } = CultureInfo.InvariantCulture;

    private Parser<LogicalExpression> FluentParser { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        FluentParser = FluentExpressionParser.Create(ParserOptions, ParserCulture);

        if (FluentParser.GetType().Assembly != typeof(Parser<LogicalExpression>).Assembly ||
            typeof(LogicalExpressionParser).Assembly.GetReferencedAssemblies().Any(reference => reference.Name == "Parlot"))
        {
            throw new InvalidOperationException("The comparison requires a Fluent graph and a dependency-free generated parser.");
        }

        foreach (var text in new[] { SimpleExpression, AdvancedExpression })
        {
            if (ParseFluent(text).ToExpressionString() != ParseGenerated(text).ToExpressionString())
            {
                throw new InvalidOperationException("The Fluent and generated parsers produced different expressions.");
            }
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Simple")]
    public LogicalExpression ParseSimpleFluent() => ParseFluent(SimpleExpression);

    [Benchmark]
    [BenchmarkCategory("Simple")]
    public LogicalExpression ParseSimpleGenerated() => ParseGenerated(SimpleExpression);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Advanced")]
    public LogicalExpression ParseAdvancedFluent() => ParseFluent(AdvancedExpression);

    [Benchmark]
    [BenchmarkCategory("Advanced")]
    public LogicalExpression ParseAdvancedGenerated() => ParseGenerated(AdvancedExpression);

    private LogicalExpression ParseFluent(string text)
    {
        var context = new ParseContext(new Scanner(text), useNewLines: false, disableLoopDetection: true, CancellationToken.None);
        if (FluentParser.TryParse(context, out var result, out var error))
        {
            return result;
        }

        throw new InvalidOperationException($"Failed to parse the benchmark expression: {error?.Message}");
    }

    private LogicalExpression ParseGenerated(string text) =>
        LogicalExpressionParser.Parse(text, ParserOptions, ParserCulture, CancellationToken.None);
}
