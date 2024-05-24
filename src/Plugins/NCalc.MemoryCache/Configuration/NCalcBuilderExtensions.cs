using System;
using Microsoft.Extensions.DependencyInjection;
using NCalc.DependencyInjection.Configuration;

namespace NCalc.MemoryCache.Configuration;

public static class NCalcBuilderExtensions
{
    /// <summary>
    /// Replaces NCalc default cache with Microsoft.Extensions.MemoryCache.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureOptions">The options of the cache.</param>
    /// <returns></returns>
    public static NCalcServiceBuilder WithMemoryCache(this NCalcServiceBuilder builder, Action<LogicalExpressionMemoryCacheOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.WithCache<LogicalExpressionMemoryCache>();
        return builder;
    }
}