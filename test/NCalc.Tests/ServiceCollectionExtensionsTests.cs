using NCalc.Cache;
using NCalc.DependencyInjection;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Visitors;
using System.Threading.Tasks;

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
        public Expression Create(string expression, ExpressionContext expressionContext = null)
        {
            return new CustomExpression(expression, expressionContext ?? new(), logicalExpressionFactory, cache);
        }

        public Expression Create(LogicalExpression logicalExpression, ExpressionContext expressionContext = null)
        {
            return new CustomExpression(logicalExpression, expressionContext ?? new(), logicalExpressionFactory, cache);
        }
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
        public LogicalExpression Create(string expression, ExpressionOptions options, CancellationToken cancellationToken = default) => throw new NCalcException("Stub method intented for testing.");

        public LogicalExpression Create(string expression, CultureInfo cultureInfo, ExpressionOptions options = ExpressionOptions.None, CancellationToken cancellationToken = default)
            => throw new NCalcException("Stub method intented for testing.");
    }

    private class CustomExpression(
        string expression,
        ExpressionContext context,
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache cache) : Expression(expression, context, logicalExpressionFactory, cache)
    {
        public CustomExpression(
            LogicalExpression logicalExpression,
            ExpressionContext context,
            ILogicalExpressionFactory logicalExpressionFactory,
            ILogicalExpressionCache cache)
            : this(string.Empty, context, logicalExpressionFactory, cache)
        {
            LogicalExpression = logicalExpression;
        }

        protected override EvaluationVisitor CreateEvaluationVisitor(CancellationToken cancellationToken = default)
        {
            return new CustomSyncVisitor(Context, cancellationToken);
        }

        protected override AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(CancellationToken cancellationToken = default)
        {
            return new CustomAsyncVisitor(Context, cancellationToken);
        }
    }

    private class CustomEvaluationVisitorFactory : IEvaluationVisitorFactory
    {
        public EvaluationVisitor CreateEvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default)
        {
            return new CustomSyncVisitor(context, cancellationToken, this);
        }

        public AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default)
        {
            return new CustomAsyncVisitor(context, cancellationToken, this);
        }
    }

    private class CustomSyncVisitor(ExpressionContext context, CancellationToken cancellationToken, IEvaluationVisitorFactory? evaluationVisitorFactory = null) : EvaluationVisitor(context, evaluationVisitorFactory, cancellationToken)
    {
        public override object Visit(ValueExpression expression)
        {
            if (expression.Value is 42)
                return "The answer";

            return base.Visit(expression);
        }
    }

    private class CustomAsyncVisitor(ExpressionContext context, CancellationToken cancellationToken, IEvaluationVisitorFactory? evaluationVisitorFactory = null) : AsyncEvaluationVisitor(context, evaluationVisitorFactory, cancellationToken)
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
