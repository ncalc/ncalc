using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NCalc.Logging;

namespace NCalc.Cache;

public sealed class LogicalExpressionCache : ILogicalExpressionCache
{
    private const string DefaultCapacitySwitchName = "NCalc.LogicalExpressionCache.DefaultCapacity";
    private const int DefaultCapacity = 128;

    private readonly Dictionary<string, LinkedListNode<CacheEntry>> _compiledExpressions = new(StringComparer.Ordinal);
    private readonly LinkedList<CacheEntry> _lru = [];
    #if NET10_0_OR_GREATER
    private readonly Lock _lock = new();
    #else
    private readonly object _lock = new();
    #endif
    private readonly ILogger<LogicalExpressionCache> _logger;
    private readonly int _capacity;

    private static readonly LogicalExpressionCache Instance;

    static LogicalExpressionCache()
    {
        Instance = new LogicalExpressionCache(NullLoggerFactory.Instance.CreateLogger<LogicalExpressionCache>(), GetDefaultCapacity());
    }

    public LogicalExpressionCache(ILogger<LogicalExpressionCache>? logger = null)
    {
        _capacity = GetDefaultCapacity();
        _logger = logger ?? NullLogger<LogicalExpressionCache>.Instance;
    }

    internal LogicalExpressionCache(ILogger<LogicalExpressionCache>? logger, int capacity)
    {
        _capacity = capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity));
        _logger = logger ?? NullLogger<LogicalExpressionCache>.Instance;
    }

    private static int GetDefaultCapacity()
    {
#if NET
        var configuredCapacityValue = AppContext.GetData(DefaultCapacitySwitchName) as string;
#else
        const string? configuredCapacityValue = null;
#endif
        if (string.IsNullOrWhiteSpace(configuredCapacityValue))
            return DefaultCapacity;

        return int.TryParse(configuredCapacityValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var configuredCapacity) &&
               configuredCapacity > 0
            ? configuredCapacity
            : throw new InvalidOperationException(
                $"The AppContext switch '{DefaultCapacitySwitchName}' must contain a positive integer value.");
    }

    public static LogicalExpressionCache GetInstance() => Instance;

    public bool TryGetValue(string expression, out LogicalExpression? logicalExpression)
    {
        lock (_lock)
        {
            if (!_compiledExpressions.TryGetValue(expression, out var node))
            {
                logicalExpression = null;
                return false;
            }

            _lru.Remove(node);
            _lru.AddFirst(node);
            logicalExpression = node.Value.LogicalExpression;
        }

        _logger.LogRetrievedFromCache(expression);

        return true;
    }

    public void Set(string expression, LogicalExpression logicalExpression)
    {
        lock (_lock)
        {
            if (_compiledExpressions.TryGetValue(expression, out var existingNode))
            {
                _lru.Remove(existingNode);
                _compiledExpressions.Remove(expression);
            }

            var node = new LinkedListNode<CacheEntry>(new CacheEntry(expression, logicalExpression));
            _lru.AddFirst(node);
            _compiledExpressions[expression] = node;

            if (_compiledExpressions.Count > _capacity)
                RemoveLeastRecentlyUsed();
        }

        _logger.LogAddedToCache(expression);
    }

    private void RemoveLeastRecentlyUsed()
    {
        var node = _lru.Last;
        if (node is null)
            return;

        _lru.RemoveLast();
        _compiledExpressions.Remove(node.Value.Expression);
        _logger.LogRemovedFromCache(node.Value.Expression);
    }

    private sealed record CacheEntry(string Expression, LogicalExpression LogicalExpression);
}
