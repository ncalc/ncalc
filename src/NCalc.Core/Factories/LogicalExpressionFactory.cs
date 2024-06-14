using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Parser;

namespace NCalc.Factories;

/// <summary>
/// Class responsible to create <see cref="LogicalExpression"/> objects. Parlot is used for parsing strings.
/// </summary>
public sealed class LogicalExpressionFactory : ILogicalExpressionFactory
{
    private static LogicalExpressionFactory? _instance;

    public static LogicalExpressionFactory GetInstance()
    {
        return _instance ??= new LogicalExpressionFactory();
    }

    LogicalExpression ILogicalExpressionFactory.Create(string expression, LogicalExpressionOptions? options)
    {
        return Create(expression, options);
    }

    public static LogicalExpression Create(string expression, LogicalExpressionOptions? options = null)
    {
        LogicalExpression? logicalExpression;
        try
        {
            var parserContext = new LogicalExpressionParserContext(expression)
            {
                ParseNumbersAsDecimal = options?.NumbersAsDecimal ?? false
            };
            logicalExpression = LogicalExpressionParser.Parse(parserContext);

            if (logicalExpression is null)
                throw new ArgumentNullException(nameof(logicalExpression));
        }
        catch (Exception exception)
        {
            throw new NCalcParserException("Error parsing the expression.", exception);
        }

        return logicalExpression;
    }
}