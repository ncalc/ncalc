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
    private static readonly ILogicalExpressionCache _cache = LogicalExpressionCache.GetInstance();

    internal static bool EnableCache
    {
        get => _cache.Enable;
        set => _cache.Enable = value;
    }

    public static LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None)
    {
        LogicalExpression? logicalExpression;

        if (_cache.Enable && !options.HasOption(ExpressionOptions.NoCache))
        {
            if (_cache.TryGetValue(expression, out logicalExpression))
            {
                return logicalExpression!;
            }
        }

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

        if (_cache.Enable && !options.HasOption(ExpressionOptions.NoCache))
        {
            _cache.Set(expression, logicalExpression);
        }

        return logicalExpression;
    }
}