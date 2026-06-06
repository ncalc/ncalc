using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public sealed class LogicalExpressionParserContext : ParseContext
{
    public LogicalExpressionParserContext(string text, CancellationToken cancellationToken = default) : this(text, new LogicalExpressionParserOptions(), cancellationToken)
    {
    }

    public LogicalExpressionParserContext(string text,
        LogicalExpressionParserOptions parserOptions,
        CancellationToken cancellationToken = default) : base(new Scanner(text), useNewLines:false, disableLoopDetection:true, cancellationToken)
    {
        ParserOptions = parserOptions;
    }

    /// <summary>
    /// Parser options containing culture info and argument separator settings.
    /// </summary>
    public LogicalExpressionParserOptions ParserOptions { get; }
}