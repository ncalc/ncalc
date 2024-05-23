using System.Diagnostics;
using NCalc.Domain;

namespace NCalc.Cache;

internal sealed class LogicalExpressionCache : ILogicalExpressionCache
{
    private readonly ConcurrentDictionary<string, WeakReference<LogicalExpression>> _compiledExpressions = new();
    
    public bool Enable { get; set; }
    
    private static LogicalExpressionCache? _instance;

    public static LogicalExpressionCache GetInstance()
    {
        return _instance ??= new LogicalExpressionCache();
    }
    
    public bool TryGetValue(string expression, out LogicalExpression? logicalExpression)
    {
        logicalExpression = null;
        if (Enable)
        {
            if (_compiledExpressions.TryGetValue(expression, out var wr))
            {
                if (wr.TryGetTarget(out logicalExpression))
                {
                    Trace.TraceInformation("Expression retrieved from cache: " + expression);
                    return true;
                }
            }
        }
        return false;
    }

    public bool Set(string expression, LogicalExpression logicalExpression)
    {
        if (Enable)
        {
            _compiledExpressions[expression] = new WeakReference<LogicalExpression>(logicalExpression);
            ClearCache();
            Trace.TraceInformation("Expression added to cache: " + expression);
            return true;
        }
        return false;
    }

    private void ClearCache()
    {
        foreach (var kvp in _compiledExpressions)
        {
            if (!kvp.Value.TryGetTarget(out _))
            {
                if (_compiledExpressions.TryRemove(kvp.Key, out _))
                {
                    Trace.TraceInformation("Cache entry released: " + kvp.Key);
                }
            }
        }
    }
}
