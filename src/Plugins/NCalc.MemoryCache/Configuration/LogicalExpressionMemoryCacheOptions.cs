using System;

namespace NCalc.MemoryCache.Configuration;

public class LogicalExpressionMemoryCacheOptions
{
    /// <summary>
    /// The duration of the cache relative to now.
    /// </summary>
    public TimeSpan AbsoluteExpirationRelativeToNow { get; set; }
}