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

    LogicalExpression ILogicalExpressionFactory.Create(string expression, ExpressionOptions options)
    {
        try
        {
            return Create(expression, options);
        }
        catch (Exception exception)
        {
            logger.LogErrorCreatingLogicalExpression(exception, expression);
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }

    LogicalExpression ILogicalExpressionFactory.Create(string expression, CultureInfo cultureInfo, ExpressionOptions options)
    {
        try
        {
            return Create(expression, cultureInfo, options);
        }
        catch (Exception exception)
        {
            logger.LogErrorCreatingLogicalExpression(exception, expression);
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }

    public static LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None)
    {
        var parserContext = new LogicalExpressionParserContext(expression, options);
        return LogicalExpressionParser.Parse(parserContext);
    }

    public static LogicalExpression Create(string expression, CultureInfo cultureInfo, ExpressionOptions options = ExpressionOptions.None)
    {
        var parserContext = new LogicalExpressionParserContext(expression, options, cultureInfo);
        return LogicalExpressionParser.Parse(parserContext);
    }
}