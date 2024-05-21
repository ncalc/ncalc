using System.Diagnostics;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Parser;

namespace NCalc.Factories;

/// <summary>
/// Class responsible to create LogicalExpression objects.
/// </summary>
public static class LogicalExpressionFactory
{
    private static bool _enableCache = true;
    private static readonly ConcurrentDictionary<string, WeakReference<LogicalExpression>> CompiledExpressions = new();
    
    internal static bool EnableCache
    {
        get => _enableCache;
        set
        {
            _enableCache = value;

            if (!EnableCache)
            {
                // Clears cache
                CompiledExpressions.Clear();
            }
        }
    }

    /// <summary>
    /// Removed unused entries from cached compiled expression
    /// </summary>
    private static void ClearCache()
    {
        foreach (var kvp in CompiledExpressions)
        {
            if (kvp.Value.TryGetTarget(out _)) 
                continue;

            if (CompiledExpressions.TryRemove(kvp.Key, out _))
                Trace.TraceInformation("Cache entry released: " + kvp.Key);
        }
    }

    public static LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None)
    {
        LogicalExpression logicalExpression;

        if (_enableCache && !options.HasOption(ExpressionOptions.NoCache))
        {
            if (CompiledExpressions.TryGetValue(expression, out var wr))
            {
                Trace.TraceInformation("Expression retrieved from cache: " + expression);

                if (wr.TryGetTarget(out var target))
                    return target;
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
        catch(Exception exception)
        {
            throw new NCalcParserException("Error parsing the expression.", exception);
        }

        if (!_enableCache || options.HasOption(ExpressionOptions.NoCache))
            return logicalExpression;
        
        CompiledExpressions[expression] = new WeakReference<LogicalExpression>(logicalExpression);
            
        ClearCache();

        Trace.TraceInformation("Expression added to cache: " + expression);
        
        return logicalExpression;
    }

}