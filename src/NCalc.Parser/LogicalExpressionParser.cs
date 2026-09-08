using NCalc.Exceptions;

namespace NCalc;

/// <summary>
/// Parses expressions using the build-time-generated NCalc grammar.
/// </summary>
public static partial class LogicalExpressionParser
{
    private static readonly LogicalExpressionParserOptions DefaultOptions = new();

    public static LogicalExpression Parse(
        string text,
        LogicalExpressionParserOptions? options = null,
        CultureInfo? culture = null,
        CancellationToken cancellationToken = default)
    {
        if (TryParseCore(text, options ?? DefaultOptions, culture ?? CultureInfo.CurrentCulture, cancellationToken, out var result))
            return result;

        throw new NCalcParserException("Invalid token in expression");
    }

    public static LogicalExpression Parse(LogicalExpressionParseContext context, CultureInfo? culture = null)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        return Parse(context.Text, context.Options, culture, context.CancellationToken);
    }

    private static partial bool TryParseCore(
        string text,
        LogicalExpressionParserOptions options,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken,
        out LogicalExpression value);
}
