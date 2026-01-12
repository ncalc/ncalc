using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NCalc.Cache.Configuration;
using NCalc.Domain;

namespace NCalc.Cache;

internal sealed class LogicalExpressionMemoryCache(
    IMemoryCache memoryCache,
    IOptions<LogicalExpressionMemoryCacheOptions> optionsSnapshot) : ILogicalExpressionCache
{
    public bool TryGetValue(LogicalExpressionCacheKey key, out LogicalExpression? logicalExpression)
    {
        return memoryCache.TryGetValue(key, out logicalExpression);
    }

    public void Set(LogicalExpressionCacheKey key, LogicalExpression logicalExpression)
    {
        memoryCache.Set(key, logicalExpression, optionsSnapshot.Value.AbsoluteExpirationRelativeToNow);
    }
}