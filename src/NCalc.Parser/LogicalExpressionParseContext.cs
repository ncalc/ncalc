namespace NCalc;

public sealed class LogicalExpressionParseContext(
    string text,
    LogicalExpressionParserOptions? options = null,
    CancellationToken cancellationToken = default)
{
    public string Text { get; } = text ?? throw new ArgumentNullException(nameof(text));
    public LogicalExpressionParserOptions Options { get; } = options ?? new();
    public CancellationToken CancellationToken { get; } = cancellationToken;
}