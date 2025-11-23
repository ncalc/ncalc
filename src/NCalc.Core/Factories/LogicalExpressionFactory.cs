using Microsoft.Extensions.Logging;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Logging;
using NCalc.Parser;

namespace NCalc.Factories;

/// <summary>
/// Class responsible to create <see cref="LogicalExpression"/> objects. Parlot is used for parsing strings.
/// </summary>
public sealed class LogicalExpressionFactory(ILogger<LogicalExpressionFactory> logger) : ILogicalExpressionFactory
{
    private static readonly LogicalExpressionFactory Instance;

    static LogicalExpressionFactory()
    {
        Instance = new LogicalExpressionFactory(DefaultLoggerFactory.Value.CreateLogger<LogicalExpressionFactory>());
    }

    public static LogicalExpressionFactory GetInstance() => Instance;

    LogicalExpression ILogicalExpressionFactory.Create(string expression, ExpressionOptions options, CancellationToken ct)
    {
        try
        {
            return Create(expression, options, ct);
        }
        catch (Exception exception)
        {
            logger.LogErrorCreatingLogicalExpression(exception, expression);
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }

    LogicalExpression ILogicalExpressionFactory.Create(string expression, CultureInfo cultureInfo, ExpressionOptions options, CancellationToken ct)
    {
        try
        {
            return Create(expression, cultureInfo, options, ct);
        }
        catch (Exception exception)
        {
            logger.LogErrorCreatingLogicalExpression(exception, expression);
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }

    public static LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None, CancellationToken ct = default)
    {
        var parserContext = new LogicalExpressionParserContext(expression, options, ct);
        return LogicalExpressionParser.Parse(parserContext);
    }

    public static LogicalExpression Create(string expression, CultureInfo cultureInfo, ExpressionOptions options = ExpressionOptions.None, CancellationToken ct = default)
    {
        var parserContext = new LogicalExpressionParserContext(expression, options, cultureInfo, ct);
        return LogicalExpressionParser.Parse(parserContext);
    }
}