using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using NCalc.Extensions;
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

    private Func<LogicalExpressionParserOptions, CultureInfo, Parser<LogicalExpression>> DynamicFactory { get; set; }
    private Func<LogicalExpressionParserOptions, CultureInfo, Parser<LogicalExpression>> GeneratedFactory { get; set; }
    private Parser<LogicalExpression> DynamicParser { get; set; }
    private Parser<LogicalExpression> GeneratedParser { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // The grammar delegate bypasses interception; the wrapper delegate constructs a fresh generated parser.
        DynamicFactory = CreateFactory("CreateParserGrammar");
        GeneratedFactory = CreateFactory("CreateExpressionParser");
        DynamicParser = ConstructDynamic();
        GeneratedParser = ConstructGenerated();

        if (DynamicParser.GetType().Assembly != typeof(Parser<LogicalExpression>).Assembly ||
            GeneratedParser.GetType().DeclaringType != typeof(LogicalExpressionParser))
        {
            throw new InvalidOperationException("The comparison requires a runtime combinator graph and a source-generated parser.");
        }

        foreach (var text in new[] { SimpleExpression, AdvancedExpression })
        {
            if (Parse(DynamicParser, text).ToExpressionString() != Parse(GeneratedParser, text).ToExpressionString())
            {
                throw new InvalidOperationException("The dynamic and generated parsers produced different expressions.");
            }
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Construction")]
    public Parser<LogicalExpression> ConstructDynamic() => DynamicFactory(ParserOptions, ParserCulture);

    [Benchmark]
    [BenchmarkCategory("Construction")]
    public Parser<LogicalExpression> ConstructGenerated() => GeneratedFactory(ParserOptions, ParserCulture);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Simple")]
    public LogicalExpression ParseSimpleDynamic() => Parse(DynamicParser, SimpleExpression);

    [Benchmark]
    [BenchmarkCategory("Simple")]
    public LogicalExpression ParseSimpleGenerated() => Parse(GeneratedParser, SimpleExpression);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Advanced")]
    public LogicalExpression ParseAdvancedDynamic() => Parse(DynamicParser, AdvancedExpression);

    [Benchmark]
    [BenchmarkCategory("Advanced")]
    public LogicalExpression ParseAdvancedGenerated() => Parse(GeneratedParser, AdvancedExpression);

    private static Func<LogicalExpressionParserOptions, CultureInfo, Parser<LogicalExpression>> CreateFactory(string name)
    {
        var factory = typeof(LogicalExpressionParser).GetMethod(
            name,
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(LogicalExpressionParserOptions), typeof(CultureInfo)],
            modifiers: null)
            ?? throw new InvalidOperationException($"The parser factory '{name}' was not found.");

        return factory.CreateDelegate<Func<LogicalExpressionParserOptions, CultureInfo, Parser<LogicalExpression>>>();
    }

    private LogicalExpression Parse(Parser<LogicalExpression> parser, string text)
    {
        var context = new LogicalExpressionParseContext(text, ParserOptions, CancellationToken.None);
        if (parser.TryParse(context, out var result, out var error))
        {
            return result;
        }

        throw new InvalidOperationException($"Failed to parse the benchmark expression: {error?.Message}");
    }
}
