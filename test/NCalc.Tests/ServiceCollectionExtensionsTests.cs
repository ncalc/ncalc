using Microsoft.Extensions.DependencyInjection;
using NCalc.Cache;
using NCalc.DependencyInjection;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Services;

namespace NCalc.Tests;

[Trait("Category", "DependencyInjection")]
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNCalc_ShouldRegisterServices()
    {
        var services = new ServiceCollection();

        services.AddNCalc();

        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<IExpressionFactory>());
        Assert.NotNull(serviceProvider.GetService<ILogicalExpressionCache>());
        Assert.NotNull(serviceProvider.GetService<ILogicalExpressionFactory>());
        Assert.NotNull(serviceProvider.GetService<IEvaluationService>());
        Assert.NotNull(serviceProvider.GetService<IAsyncEvaluationService>());
    }

    [Fact]
    public void WithExpressionFactory_ShouldReplaceExpressionFactory()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithExpressionFactory<CustomExpressionFactory>();

        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetService<IExpressionFactory>();
        Assert.IsType<CustomExpressionFactory>(factory);
    }


    [Fact]
    public void WithCache_ShouldReplaceCache()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithCache<CustomCache>();

        var serviceProvider = services.BuildServiceProvider();

        var cache = serviceProvider.GetService<ILogicalExpressionCache>();
        Assert.IsType<CustomCache>(cache);
    }

    [Fact]
    public void WithLogicalExpressionFactory_ShouldReplaceFactory()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithLogicalExpressionFactory<CustomLogicalExpressionFactory>();

        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetService<ILogicalExpressionFactory>();
        Assert.IsType<CustomLogicalExpressionFactory>(factory);
    }

    [Fact]
    public void WithEvaluationService_ShouldReplaceEvaluationService()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithEvaluationService<CustomEvaluationService>();

        var serviceProvider = services.BuildServiceProvider();

        var visitor = serviceProvider.GetService<IEvaluationService>();
        var expFactory = serviceProvider.GetRequiredService<IExpressionFactory>();

        var exp = expFactory.Create("1+1");
        Assert.Equal(42, exp.Evaluate());
        Assert.IsType<CustomEvaluationService>(visitor);
    }

    [Fact]
    public async Task WithAsyncEvaluationService_ShouldReplaceEvaluationService()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithAsyncEvaluationService<CustomAsyncEvaluationService>();

        var serviceProvider = services.BuildServiceProvider();

        var visitor = serviceProvider.GetService<IAsyncEvaluationService>();
        var expFactory = serviceProvider.GetRequiredService<IAsyncExpressionFactory>();

        var exp = expFactory.Create("1+1");
        Assert.Equal(42, await exp.EvaluateAsync());
        Assert.IsType<CustomAsyncEvaluationService>(visitor);
    }

    #region Custom Implementations Stubs

    private class CustomExpressionFactory : IExpressionFactory
    {
        public Expression Create(string expression, ExpressionContext expressionContext = null) => throw new NCalcException("Stub method intented for testing.");

        public Expression Create(LogicalExpression logicalExpression, ExpressionContext expressionContext = null) => throw new NCalcException("Stub method intented for testing.");
    }

    private class CustomCache : ILogicalExpressionCache
    {
        public bool TryGetValue(string expression, out LogicalExpression logicalExpression) => throw new NCalcException("Stub method intented for testing.");


        public void Set(string expression, LogicalExpression logicalExpression)
        {
        }
    }

    private class CustomLogicalExpressionFactory : ILogicalExpressionFactory
    {
        public LogicalExpression Create(string expression, ExpressionOptions options) => throw new NCalcException("Stub method intented for testing.");
    }

    private class CustomEvaluationService : IEvaluationService
    {
        public event EvaluateFunctionHandler EvaluateFunction;
        public event EvaluateParameterHandler EvaluateParameter;

        public object Evaluate(LogicalExpression expression, ExpressionContext context)
        {
            return 42;
        }
    }

    private class CustomAsyncEvaluationService : IAsyncEvaluationService
    {
        public event AsyncEvaluateFunctionHandler EvaluateFunctionAsync;
        public event AsyncEvaluateParameterHandler EvaluateParameterAsync;

        public ValueTask<object> EvaluateAsync(LogicalExpression expression, AsyncExpressionContext context)
        {
            return new(42);
        }
    }
    #endregion
}