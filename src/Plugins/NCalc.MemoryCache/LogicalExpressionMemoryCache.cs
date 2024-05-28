using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NCalc.Cache.Configuration;
using NCalc.Domain;

namespace NCalc.Cache;

internal sealed class LogicalExpressionMemoryCache(
    IMemoryCache memoryCache,
    IOptions<LogicalExpressionMemoryCacheOptions> optionsSnapshot) : ILogicalExpressionCache
{
    public bool TryGetValue(string expression, out LogicalExpression logicalExpression)
    {
        return memoryCache.TryGetValue(expression, out logicalExpression);
    }

    public void Set(string expression, LogicalExpression logicalExpression)
    {
        memoryCache.Set(expression, logicalExpression, optionsSnapshot.Value.AbsoluteExpirationRelativeToNow);
    }
}