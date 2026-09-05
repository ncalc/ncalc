using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using NCalc.Domain;
using NCalc.Parser;
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
    private LogicalExpressionParserOptions ParserOptions { get; } =
        LogicalExpressionParserOptions.FromCultureInfo(CultureInfo.InvariantCulture);

    private Func<ExpressionOptions, LogicalExpressionParserOptions, Parser<LogicalExpression>> DynamicFactory { get; set; }
    private Func<ExpressionOptions, LogicalExpressionParserOptions, Parser<LogicalExpression>> GeneratedFactory { get; set; }
    private Parser<LogicalExpression> DynamicParser { get; set; }
    private Parser<LogicalExpression> GeneratedParser { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // A delegate to the original grammar bypasses call-site interception. Reflection runs only in setup.
        var factory = typeof(LogicalExpressionParser).GetMethod(
            "CreateGeneratedExpressionParser", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The original Parlot grammar factory was not found.");

        DynamicFactory = factory.CreateDelegate<Func<ExpressionOptions, LogicalExpressionParserOptions, Parser<LogicalExpression>>>();
        GeneratedFactory = LogicalExpressionParser.CreateExpressionParser;
        DynamicParser = ConstructDynamic();
        GeneratedParser = ConstructGenerated();

        if (DynamicParser.GetType().Assembly != typeof(Parser<LogicalExpression>).Assembly ||
            GeneratedParser.GetType().DeclaringType != typeof(LogicalExpressionParser))
        {
            throw new InvalidOperationException("The comparison requires a runtime combinator graph and a source-generated parser.");
        }

        foreach (var text in new[]
        {
            LogicalExpressionFactoryBenchmark.SimpleExpression,
            LogicalExpressionFactoryBenchmark.AdvancedExpression
        })
        {
            if (Parse(DynamicParser, text).ToString() != Parse(GeneratedParser, text).ToString())
            {
                throw new InvalidOperationException("The dynamic and generated parsers produced different expressions.");
            }
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Construction")]
    public Parser<LogicalExpression> ConstructDynamic() => DynamicFactory(ExpressionOptions.None, ParserOptions);

    [Benchmark]
    [BenchmarkCategory("Construction")]
    public Parser<LogicalExpression> ConstructGenerated() => GeneratedFactory(ExpressionOptions.None, ParserOptions);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Simple")]
    public LogicalExpression ParseSimpleDynamic() => Parse(DynamicParser, LogicalExpressionFactoryBenchmark.SimpleExpression);

    [Benchmark]
    [BenchmarkCategory("Simple")]
    public LogicalExpression ParseSimpleGenerated() => Parse(GeneratedParser, LogicalExpressionFactoryBenchmark.SimpleExpression);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Advanced")]
    public LogicalExpression ParseAdvancedDynamic() => Parse(DynamicParser, LogicalExpressionFactoryBenchmark.AdvancedExpression);

    [Benchmark]
    [BenchmarkCategory("Advanced")]
    public LogicalExpression ParseAdvancedGenerated() => Parse(GeneratedParser, LogicalExpressionFactoryBenchmark.AdvancedExpression);

    private LogicalExpression Parse(Parser<LogicalExpression> parser, string text)
    {
        var context = new LogicalExpressionParserContext(text, ExpressionOptions.None, ParserOptions);
        if (parser.TryParse(context, out var result, out var error))
        {
            return result;
        }

        throw new InvalidOperationException($"Failed to parse the benchmark expression: {error?.Message}");
    }
}
