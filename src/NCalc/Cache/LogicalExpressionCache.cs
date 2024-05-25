using System.Diagnostics;
using NCalc.Domain;

namespace NCalc.Cache;

public sealed class LogicalExpressionCache : ILogicalExpressionCache
{
    private readonly ConcurrentDictionary<string, WeakReference<LogicalExpression>> _compiledExpressions = new();
    
    private static LogicalExpressionCache? _instance;
    
    private LogicalExpressionCache()
    {
        
    }
    public static LogicalExpressionCache GetInstance()
    {
        return _instance ??= new LogicalExpressionCache();
    }
    
    public bool TryGetValue(string expression, out LogicalExpression? logicalExpression)
    {
        logicalExpression = null;
        if (_compiledExpressions.TryGetValue(expression, out var wr))
        {
            if (wr.TryGetTarget(out logicalExpression))
            {
                Trace.TraceInformation("Expression retrieved from cache: " + expression);
                return true;
            }
        }
        
        return false;
    }

    public void Set(string expression, LogicalExpression logicalExpression)
    {
        _compiledExpressions[expression] = new WeakReference<LogicalExpression>(logicalExpression);
        ClearCache();
        Trace.TraceInformation("Expression added to cache: " + expression);
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
