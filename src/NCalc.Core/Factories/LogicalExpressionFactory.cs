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

    LogicalExpression ILogicalExpressionFactory.Create(string expression, ExpressionOptions options, CancellationToken cancellationToken)
    {
        try
        {
            return Create(expression, options, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogErrorCreatingLogicalExpression(exception, expression);
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }

    LogicalExpression ILogicalExpressionFactory.Create(string expression, CultureInfo cultureInfo, ExpressionOptions options, CancellationToken cancellationToken)
    {
        try
        {
            return Create(expression, cultureInfo, options, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogErrorCreatingLogicalExpression(exception, expression);
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }

    public static LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None, CancellationToken cancellationToken = default)
    {
        return Create(expression, CultureInfo.CurrentUICulture, options, cancellationToken);
    }

    public static LogicalExpression Create(string expression, CultureInfo cultureInfo, ExpressionOptions options = ExpressionOptions.None, CancellationToken cancellationToken = default)
    {
        var parserContext = new LogicalExpressionParserContext(expression, LogicalExpressionParserOptions.Create(options, cultureInfo), cancellationToken);
        return LogicalExpressionParser.Parse(parserContext);
    }
}