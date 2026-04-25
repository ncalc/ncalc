using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NCalc.Domain;
using NCalc.Logging;

namespace NCalc.Cache;

public sealed class LogicalExpressionCache(ILogger<LogicalExpressionCache>? logger = null) : ILogicalExpressionCache
{
    private readonly ConcurrentDictionary<string, WeakReference<LogicalExpression>> _compiledExpressions = new();
    private readonly ILogger<LogicalExpressionCache> _logger = logger ?? NullLogger<LogicalExpressionCache>.Instance;

    private static readonly LogicalExpressionCache Instance;

    static LogicalExpressionCache()
    {
        Instance = new LogicalExpressionCache(NullLoggerFactory.Instance.CreateLogger<LogicalExpressionCache>());
    }

    public static LogicalExpressionCache GetInstance() => Instance;

    public bool TryGetValue(string expression, out LogicalExpression? logicalExpression)
    {
        logicalExpression = null;

        if (!_compiledExpressions.TryGetValue(expression, out var wr))
            return false;
        if (!wr.TryGetTarget(out logicalExpression))
            return false;

        _logger.LogRetrievedFromCache(expression);

        return true;
    }

    public void Set(string expression, LogicalExpression logicalExpression)
    {
        _compiledExpressions[expression] = new WeakReference<LogicalExpression>(logicalExpression);
        ClearCache();
        _logger.LogAddedToCache(expression);
    }

    private void ClearCache()
    {
        foreach (var kvp in _compiledExpressions)
        {
            if (kvp.Value.TryGetTarget(out _))
                continue;

            if (_compiledExpressions.TryRemove(kvp.Key, out _))
            {
                _logger.LogRemovedFromCache(kvp.Key);
            }
        }
    }
}
