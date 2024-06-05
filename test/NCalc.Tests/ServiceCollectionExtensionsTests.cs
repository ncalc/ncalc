using Microsoft.Extensions.DependencyInjection;
using NCalc.Cache;
using NCalc.DependencyInjection;
using NCalc.Domain;
using NCalc.Factories;
using NCalc.Handlers;
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
        Assert.NotNull(serviceProvider.GetService<IEvaluationVisitor>());
        Assert.NotNull(serviceProvider.GetService<IParameterExtractionVisitor>());
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
    public void WithEvaluationVisitor_ShouldReplaceEvaluationVisitor()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithEvaluationVisitor<CustomEvaluationVisitor>();

        var serviceProvider = services.BuildServiceProvider();

        var visitor = serviceProvider.GetService<IEvaluationVisitor>();
        var expFactory = serviceProvider.GetRequiredService<IExpressionFactory>();

        var exp = expFactory.Create("1+1");
        Assert.Equal(42, exp.Evaluate());
        Assert.IsType<CustomEvaluationVisitor>(visitor);
    }

    [Fact]
    public void WithParameterExtractionVisitor_ShouldReplaceParameterExtractionVisitor()
    {
        var services = new ServiceCollection();

        services.AddNCalc()
            .WithParameterExtractionVisitor<CustomParameterExtractionVisitor>();

        var serviceProvider = services.BuildServiceProvider();

        var visitor = serviceProvider.GetService<IParameterExtractionVisitor>();
        Assert.IsType<CustomParameterExtractionVisitor>(visitor);
    }

    #region Custom Implementations Stubs

    private class CustomExpressionFactory : IExpressionFactory
    {
        public Expression Create(string expression, ExpressionContext expressionContext = null) => throw new NotImplementedException();

        public Expression Create(LogicalExpression logicalExpression, ExpressionContext expressionContext = null) => throw new NotImplementedException();
    }
    
    private class CustomCache : ILogicalExpressionCache
    {
        public bool TryGetValue(string expression, out LogicalExpression logicalExpression) =>
            throw new NotImplementedException();

        public void Set(string expression, LogicalExpression logicalExpression)
        {
            throw new NotImplementedException();
        }
    }

    private class CustomLogicalExpressionFactory : ILogicalExpressionFactory
    {
        public LogicalExpression Create(string expression,ExpressionContext context) =>
            throw new NotImplementedException();
    }

    private class CustomEvaluationVisitor : IEvaluationVisitor
    {
        public void Visit(TernaryExpression expression)
        {

        }

        public void Visit(BinaryExpression expression)
        {

        }

        public void Visit(UnaryExpression expression)
        {

        }

        public void Visit(ValueExpression expression)
        {

        }

        public void Visit(Function function)
        {

        }

        public void Visit(Identifier identifier)
        {

        }

        public event EvaluateFunctionHandler EvaluateFunction;
        public event EvaluateParameterHandler EvaluateParameter;
        public ExpressionContext Context { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public object Result => 42;
    }

    private class CustomParameterExtractionVisitor : IParameterExtractionVisitor
    {
        public void Visit(LogicalExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(TernaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(BinaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(UnaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(ValueExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Visit(Function function)
        {
            throw new NotImplementedException();
        }

        public void Visit(Identifier identifier)
        {
            throw new NotImplementedException();
        }

        public List<string> Parameters { get; }
    }

    #endregion
}