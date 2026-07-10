using NCalc.Cache;
using NCalc.DependencyInjection;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Visitors;
using System.Threading.Tasks;
using NCalc.Parser;

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
    public async Task WithExpressionFactory_ShouldUseCustomEvaluationVisitors()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithExpressionFactory<CustomExpressionFactory>();

        var serviceProvider = services.BuildServiceProvider();

        var expFactory = serviceProvider.GetRequiredService<IExpressionFactory>();

        var exp = expFactory.Create("42");
        await Assert.That(exp.Evaluate(CancellationToken.None)).IsEqualTo("The answer");
        await Assert.That(await exp.EvaluateAsync(CancellationToken.None)).IsEqualTo("The answer async");
    }

    [Test]
    public async Task WithEvaluationVisitorFactory_ShouldPropagateToNestedExpressions()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithEvaluationVisitorFactory<CustomEvaluationVisitorFactory>();

        var serviceProvider = services.BuildServiceProvider();

        var expFactory = serviceProvider.GetRequiredService<IExpressionFactory>();

        var exp = expFactory.Create("value");
        exp.Parameters["value"] = new Expression("42");

        await Assert.That(exp.Evaluate(CancellationToken.None)).IsEqualTo("The answer");
        await Assert.That(await exp.EvaluateAsync(CancellationToken.None)).IsEqualTo("The answer async");
    }

    #region Custom Implementations Stubs

    private class CustomExpressionFactory(
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache cache) : IExpressionFactory
    {
        public Expression Create(
            string expression,
            ExpressionConfiguration? configuration = null)
        {
            return new CustomExpression(
                expression,
                configuration ?? new ExpressionConfiguration(),
                logicalExpressionFactory,
                cache);
        }

        public Expression Create(
            LogicalExpression logicalExpression,
            ExpressionConfiguration? configuration = null)
        {
            return new CustomExpression(
                logicalExpression,
                configuration ?? new ExpressionConfiguration(),
                logicalExpressionFactory,
                cache);
        }
    }

    private class CustomLogicalExpressionFactory : ILogicalExpressionFactory
    {
        public LogicalExpression Create(
            string expression,
            LogicalExpressionParserOptions? options = null,
            CultureInfo? cultureInfo = null,
            CancellationToken cancellationToken = default)
            => throw new NCalcException("Stub method intended for testing.");
    }

    private class CustomCache : ILogicalExpressionCache
    {
        public bool TryGetValue(string expression, out LogicalExpression? logicalExpression)
        {
            logicalExpression = null;
            return false;
        }

        public void Set(string expression, LogicalExpression logicalExpression)
        {
        }
    }

    private class CustomExpression(
        string expression,
        ExpressionConfiguration configuration,
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache cache)
        : Expression(expression, configuration, logicalExpressionFactory, cache)
    {
        public CustomExpression(
            LogicalExpression logicalExpression,
            ExpressionConfiguration configuration,
            ILogicalExpressionFactory logicalExpressionFactory,
            ILogicalExpressionCache cache)
            : this(string.Empty, configuration, logicalExpressionFactory, cache)
        {
            LogicalExpression = logicalExpression;
        }

        protected override EvaluationVisitor CreateEvaluationVisitor(
            ExpressionContext context,
            CancellationToken cancellationToken = default)
        {
            return new CustomSyncVisitor(
                context,
                EvaluationOptions,
                CultureInfo,
                cancellationToken);
        }

        protected override AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(
            ExpressionContext context,
            CancellationToken cancellationToken = default)
        {
            return new CustomAsyncVisitor(
                context,
                EvaluationOptions,
                CultureInfo,
                cancellationToken);
        }
    }

    private class CustomEvaluationVisitorFactory : IEvaluationVisitorFactory
    {
        public EvaluationVisitor CreateEvaluationVisitor(
            ExpressionContext context,
            ExpressionEvaluationOptions options,
            CultureInfo cultureInfo,
            CancellationToken cancellationToken = default)
        {
            return new CustomSyncVisitor(
                context,
                options,
                cultureInfo,
                cancellationToken,
                this);
        }

        public AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(
            ExpressionContext context,
            ExpressionEvaluationOptions options,
            CultureInfo cultureInfo,
            CancellationToken cancellationToken = default)
        {
            return new CustomAsyncVisitor(
                context,
                options,
                cultureInfo,
                cancellationToken,
                this);
        }
    }

    private class CustomSyncVisitor(
        ExpressionContext context,
        ExpressionEvaluationOptions options,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken,
        IEvaluationVisitorFactory? evaluationVisitorFactory = null)
        : EvaluationVisitor(
            context,
            options,
            cultureInfo,
            evaluationVisitorFactory,
            cancellationToken)
    {
        public override object Visit(ValueExpression expression)
        {
            if (expression.Value is 42)
                return "The answer";

            return base.Visit(expression);
        }
    }

    private class CustomAsyncVisitor(
        ExpressionContext context,
        ExpressionEvaluationOptions options,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken,
        IEvaluationVisitorFactory? evaluationVisitorFactory = null)
        : AsyncEvaluationVisitor(
            context,
            options,
            cultureInfo,
            evaluationVisitorFactory,
            cancellationToken)
    {
        public override Task<object> Visit(ValueExpression expression)
        {
            if (expression.Value is 42)
                return Task.FromResult<object>("The answer async");

            return base.Visit(expression);
        }
    }

    #endregion
}
