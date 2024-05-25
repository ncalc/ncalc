﻿using Microsoft.Extensions.DependencyInjection;
using NCalc.DependencyInjection.Configuration;
using NCalc.Factories;
using NCalc.MemoryCache.Configuration;

namespace NCalc.Tests.Fixtures;

public abstract class FactoriesFixtureBase
{
    public IExpressionFactory ExpressionFactory { get; protected set; }
    
    public ILogicalExpressionFactory LogicalExpressionFactory { get;protected set; }
}

public sealed class FactoriesFixture : FactoriesFixtureBase
{
    public FactoriesFixture() 
    {
        var serviceProvider = new ServiceCollection()
            .AddNCalc()
            .Services.BuildServiceProvider();
        ExpressionFactory = serviceProvider.GetRequiredService<IExpressionFactory>();
        LogicalExpressionFactory = serviceProvider.GetRequiredService<ILogicalExpressionFactory>();
    }
}

public sealed class FactoriesWithMemoryCacheFixture : FactoriesFixtureBase
{
    public FactoriesWithMemoryCacheFixture() 
    {
        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .AddNCalc()
            .WithMemoryCache(options =>
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            })
            .Services.BuildServiceProvider();
        ExpressionFactory = serviceProvider.GetRequiredService<IExpressionFactory>();
        LogicalExpressionFactory = serviceProvider.GetRequiredService<ILogicalExpressionFactory>();
    }
}