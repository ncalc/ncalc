using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NCalc.Exceptions;
using NCalc.Extensions;
using NCalc.Logging;
using NCalc.Parser;

namespace NCalc.Factories;

/// <summary>
/// Class responsible to create <see cref="LogicalExpression"/> objects. Parlot is used for parsing strings.
/// </summary>
public sealed class LogicalExpressionFactory(ILogger<LogicalExpressionFactory>? logger = null) : ILogicalExpressionFactory
{
    private readonly ILogger<LogicalExpressionFactory> _logger = logger ?? NullLogger<LogicalExpressionFactory>.Instance;
    private static readonly LogicalExpressionFactory Instance;

    static LogicalExpressionFactory()
    {
        Instance = new LogicalExpressionFactory(NullLoggerFactory.Instance.CreateLogger<LogicalExpressionFactory>());
    }

    public static LogicalExpressionFactory GetInstance() => Instance;

    LogicalExpression ILogicalExpressionFactory.Create(string expression, LogicalExpressionParserOptions? options, CultureInfo? cultureInfo, CancellationToken cancellationToken)
    {
        try
        {
            return Create(expression, options, cultureInfo, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogErrorCreatingLogicalExpression(exception, expression);
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }

    public static LogicalExpression Create(string expression, LogicalExpressionParserOptions? options = null, CultureInfo? cultureInfo = null, CancellationToken cancellationToken = default)
    {
        var parserContext = new LogicalExpressionParserContext(expression, options ?? new LogicalExpressionParserOptions(), cancellationToken);
        return LogicalExpressionParser.Parse(parserContext, cultureInfo);
    }
}
