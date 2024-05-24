﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NCalc.Cache;
using NCalc.Domain;
using NCalc.MemoryCache.Configuration;

namespace NCalc.MemoryCache;

internal sealed class LogicalExpressionMemoryCache(
    IMemoryCache memoryCache,
    IOptionsSnapshot<LogicalExpressionMemoryCacheOptions> optionsSnapshot) : ILogicalExpressionCache
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