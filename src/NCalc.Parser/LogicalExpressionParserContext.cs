using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public sealed class LogicalExpressionParserContext : ParseContext
{
    public LogicalExpressionParserContext(string text, CancellationToken ct = default) : this(text, new LogicalExpressionParserOptions(), ct)
    {
    }

    public LogicalExpressionParserContext(string text,
        LogicalExpressionParserOptions parserOptions,
        CancellationToken ct = default) : base(new Scanner(text), useNewLines:false, disableLoopDetection:true, ct)
    {
        ParserOptions = parserOptions;
    }

    /// <summary>
    /// Parser options containing culture info and argument separator settings.
    /// </summary>
    public LogicalExpressionParserOptions ParserOptions { get; }
}