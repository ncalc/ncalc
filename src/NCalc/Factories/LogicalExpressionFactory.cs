using NCalc;
using NCalc.Cache;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Parser;

namespace NCalc.Factories;

/// <summary>
/// Class responsible to create LogicalExpression objects.
/// </summary>
public static class LogicalExpressionFactory
{
    public static LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None)
    {
        LogicalExpression? logicalExpression;
        try
        {
            var context = new LogicalExpressionParserContext(expression)
            {
                UseDecimalsAsDefault = options.HasOption(ExpressionOptions.DecimalAsDefault)
            };
            logicalExpression = LogicalExpressionParser.Parse(context);

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