using NCalc.Cache;
using NCalc.DependencyInjection;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Visitors;

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
        Assert.NotNull(serviceProvider.GetService<IEvaluationVisitorFactory>());
        Assert.NotNull(serviceProvider.GetService<IAsyncEvaluationVisitorFactory>());
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
            .WithEvaluationVisitorFactory<CustomEvaluationVisitorFactory>();

        var serviceProvider = services.BuildServiceProvider();

        var customVisitorFactory = serviceProvider.GetService<IEvaluationVisitorFactory>();
        var expFactory = serviceProvider.GetRequiredService<IExpressionFactory>();

        var exp = expFactory.Create("42");
        Assert.Equal("The answer", exp.Evaluate(TestContext.Current.CancellationToken));
        Assert.IsType<CustomEvaluationVisitorFactory>(customVisitorFactory);
    }

    [Fact]
    public async Task WithAsyncEvaluationService_ShouldReplaceEvaluationService()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithAsyncEvaluationVisitorFactory<CustomAsyncEvaluationVisitorFactory>();

        var serviceProvider = services.BuildServiceProvider();

        var customVisitorFactory = serviceProvider.GetService<IAsyncEvaluationVisitorFactory>();
        var expFactory = serviceProvider.GetRequiredService<IAsyncExpressionFactory>();

        var exp = expFactory.Create("42");
        Assert.Equal("The answer", await exp.EvaluateAsync(TestContext.Current.CancellationToken));
        Assert.IsType<CustomAsyncEvaluationVisitorFactory>(customVisitorFactory);
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
        public LogicalExpression Create(string expression, ExpressionOptions options, CancellationToken ct = default) => throw new NCalcException("Stub method intented for testing.");

        public LogicalExpression Create(string expression, CultureInfo cultureInfo, ExpressionOptions options = ExpressionOptions.None, CancellationToken ct = default)
            => throw new NCalcException("Stub method intented for testing.");
    }

    private class CustomVisitor(ExpressionContext context) : EvaluationVisitor(context)
    {
        public override object Visit(ValueExpression expression, CancellationToken ct = default)
        {
            if (expression.Value is 42)
                return "The answer";

            return base.Visit(expression, ct);
        }
    }

    private class CustomEvaluationVisitorFactory : IEvaluationVisitorFactory
    {
        public EvaluationVisitor Create(ExpressionContext context)
        {
            return new CustomVisitor(context);
        }
    }

    private class CustomAsyncVisitor(AsyncExpressionContext context) : AsyncEvaluationVisitor(context)
    {
        public override ValueTask<object> Visit(ValueExpression expression, CancellationToken ct = default)
        {
            if (expression.Value is 42)
                return new("The answer");

            return base.Visit(expression, ct);
        }
    }

    private class CustomAsyncEvaluationVisitorFactory : IAsyncEvaluationVisitorFactory
    {
        public AsyncEvaluationVisitor Create(AsyncExpressionContext context)
        {
            return new CustomAsyncVisitor(context);
        }
    }

    #endregion
}