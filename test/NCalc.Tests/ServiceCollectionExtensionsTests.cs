using Microsoft.Extensions.DependencyInjection;
using NCalc.Cache;
using NCalc.DependencyInjection;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Visitors;

namespace NCalc.Tests;

[Property("Category", "DependencyInjection")]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public async Task AddNCalc_ShouldRegisterServices()
    {
        var services = new ServiceCollection();

        services.AddNCalc();

        var serviceProvider = services.BuildServiceProvider();

        await Assert.That(serviceProvider.GetService<IExpressionFactory>()).IsNotNull();
        await Assert.That(serviceProvider.GetService<ILogicalExpressionCache>()).IsNotNull();
        await Assert.That(serviceProvider.GetService<ILogicalExpressionFactory>()).IsNotNull();
        await Assert.That(serviceProvider.GetService<IEvaluationVisitorFactory>()).IsNotNull();
        await Assert.That(serviceProvider.GetService<IAsyncEvaluationVisitorFactory>()).IsNotNull();
    }

    [Test]
    public async Task WithExpressionFactory_ShouldReplaceExpressionFactory()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithExpressionFactory<CustomExpressionFactory>();

        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetService<IExpressionFactory>();
        await Assert.That(factory).IsTypeOf<CustomExpressionFactory>();
    }

    [Test]
    public async Task WithCache_ShouldReplaceCache()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithCache<CustomCache>();

        var serviceProvider = services.BuildServiceProvider();

        var cache = serviceProvider.GetService<ILogicalExpressionCache>();
        await Assert.That(cache).IsTypeOf<CustomCache>();
    }

    [Test]
    public async Task WithLogicalExpressionFactory_ShouldReplaceFactory()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithLogicalExpressionFactory<CustomLogicalExpressionFactory>();

        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetService<ILogicalExpressionFactory>();
        await Assert.That(factory).IsTypeOf<CustomLogicalExpressionFactory>();
    }

    [Test]
    public async Task WithEvaluationService_ShouldReplaceEvaluationService(CancellationToken cancellationToken)
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithEvaluationVisitorFactory<CustomEvaluationVisitorFactory>();

        var serviceProvider = services.BuildServiceProvider();

        var customVisitorFactory = serviceProvider.GetService<IEvaluationVisitorFactory>();
        var expFactory = serviceProvider.GetRequiredService<IExpressionFactory>();

        var exp = expFactory.Create("42");
        await Assert.That(exp.Evaluate(cancellationToken)).IsEqualTo("The answer");
        await Assert.That(customVisitorFactory).IsTypeOf<CustomEvaluationVisitorFactory>();
    }

    [Test]
    public async Task WithAsyncEvaluationService_ShouldReplaceEvaluationService(CancellationToken cancellationToken)
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithAsyncEvaluationVisitorFactory<CustomAsyncEvaluationVisitorFactory>();

        var serviceProvider = services.BuildServiceProvider();

        var customVisitorFactory = serviceProvider.GetService<IAsyncEvaluationVisitorFactory>();
        var expFactory = serviceProvider.GetRequiredService<IAsyncExpressionFactory>();

        var exp = expFactory.Create("42");
        await Assert.That(await exp.EvaluateAsync(cancellationToken)).IsEqualTo("The answer");
        await Assert.That(customVisitorFactory).IsTypeOf<CustomAsyncEvaluationVisitorFactory>();
    }

    #region Custom Implementations Stubs

    private class CustomExpressionFactory : IExpressionFactory    {
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