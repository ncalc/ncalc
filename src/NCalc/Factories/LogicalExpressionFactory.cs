using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Parser;

namespace NCalc.Factories;

public static class LogicalExpressionFactory
{
    private static bool _enableCache = true;
    private static readonly ConcurrentDictionary<string, WeakReference<LogicalExpression>> CompiledExpressions = new();
    
    public static bool EnableCache
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

    public static LogicalExpression Create(string expression, ExpressionOptions options)
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
        }
        catch(Exception ex)
        {
            //TODO: Handle errors like the old version.
            var message = new StringBuilder(ex.Message);
            throw new NCalcParserException(message.ToString(), ex);
        }

        if (!_enableCache || options.HasOption(ExpressionOptions.NoCache))
            return logicalExpression;
        
        CompiledExpressions[expression] = new WeakReference<LogicalExpression>(logicalExpression);
            
        ClearCache();

        Trace.TraceInformation("Expression added to cache: " + expression);
        
        return logicalExpression;
    }

}