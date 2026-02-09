using NCalc.Antlr.Configuration;
using NCalc.Cache.Configuration;
using NCalc.DependencyInjection;
using NCalc.Factories;

namespace NCalc.Tests.Fixtures;

public abstract class FactoriesFixtureBase
{
    public IExpressionFactory ExpressionFactory { get; protected set; }

    public ILogicalExpressionFactory LogicalExpressionFactory { get; protected set; }
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

public sealed class FactoriesWithAntlrFixture : FactoriesFixtureBase
{
    public FactoriesWithAntlrFixture()
    {
        var serviceProvider = new ServiceCollection()
            .AddNCalc()
            .WithAntlr()
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
            .WithMemoryCache()
            .Services.BuildServiceProvider();
        ExpressionFactory = serviceProvider.GetRequiredService<IExpressionFactory>();
        LogicalExpressionFactory = serviceProvider.GetRequiredService<ILogicalExpressionFactory>();
    }
}