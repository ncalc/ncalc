namespace NCalc;

public sealed class LogicalExpressionParseContext
{
    public string Text { get; }
    public LogicalExpressionParserOptions Options { get; }
    public CancellationToken CancellationToken { get; }

    public LogicalExpressionParseContext(
    string text,
    LogicalExpressionParserOptions? options = null,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);

        Text = text;
        Options = options ?? new();
        CancellationToken = cancellationToken;
    }
}