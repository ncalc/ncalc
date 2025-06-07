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

    public LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None)
    {
        try
        {
            return Create(expression, CultureInfo.CurrentCulture, options, null);
        }
        catch (Exception exception)
        {
            logger.LogErrorCreatingLogicalExpression(exception, expression);
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }

    LogicalExpression ILogicalExpressionFactory.Create(string expression, CultureInfo cultureInfo, ExpressionOptions options, AdvancedExpressionOptions? advancedOptions)
    {
        try
        {
            return Create(expression, cultureInfo, options, advancedOptions);
        }
        catch (Exception exception)
        {
            logger.LogErrorCreatingLogicalExpression(exception, expression);
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }

    public static LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None, AdvancedExpressionOptions? advancedOptions = null)
    {
        var parserContext = new LogicalExpressionParserContext(expression, options);
        parserContext.AdvancedOptions = advancedOptions;
        return LogicalExpressionParser.Parse(parserContext);
    }

    public static LogicalExpression Create(string expression, CultureInfo cultureInfo, ExpressionOptions options = ExpressionOptions.None, AdvancedExpressionOptions? advancedOptions = null)
    {
        var parserContext = new LogicalExpressionParserContext(expression, options, cultureInfo);
        parserContext.AdvancedOptions = advancedOptions;
        return LogicalExpressionParser.Parse(parserContext);
    }
}