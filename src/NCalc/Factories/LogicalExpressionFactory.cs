using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Parser;

namespace NCalc.Factories;

/// <summary>
/// Class responsible to create <see cref="LogicalExpression"/> objects. Parlot is used for parsing strings.
/// </summary>
public static class LogicalExpressionFactory
{
    public static LogicalExpression Create(string expression, ExpressionContext? expressionContext = null)
    {
        LogicalExpression? logicalExpression;
        try
        {
            var options = expressionContext?.Options ?? ExpressionOptions.None;
            var parserContext = new LogicalExpressionParserContext(expression)
            {
                UseDecimalsAsDefault = options.HasOption(ExpressionOptions.DecimalAsDefault)
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