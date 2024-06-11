using Microsoft.Extensions.DependencyInjection;
using NCalc.Cache;
using NCalc.Factories;
using NCalc.Visitors;

namespace NCalc.DependencyInjection;

public class NCalcServiceBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
    
    public NCalcServiceBuilder WithExpressionFactory<TExpressionFactory>()  where TExpressionFactory : class, IExpressionFactory
    {
        Services.ReplaceScoped<IExpressionFactory, TExpressionFactory>();
        return this;
    }
    
    public NCalcServiceBuilder WithAsyncExpressionFactory<TAsyncExpressionFactory>()  where TAsyncExpressionFactory : class, IAsyncExpressionFactory
    {
        Services.ReplaceScoped<IAsyncExpressionFactory, TAsyncExpressionFactory>();
        return this;
    }
    
    public NCalcServiceBuilder WithCache<TCache>()  where TCache : class, ILogicalExpressionCache
    {
        Services.ReplaceSingleton<ILogicalExpressionCache,TCache>();
        return this;
    }

    public NCalcServiceBuilder WithLogicalExpressionFactory<TLogicalExpressionFactory>() where TLogicalExpressionFactory : class, ILogicalExpressionFactory
    {
        Services.ReplaceSingleton<ILogicalExpressionFactory,TLogicalExpressionFactory>();
        return this;
    }
    
    public NCalcServiceBuilder WithEvaluationVisitor<TEvaluationVisitor>() where TEvaluationVisitor : class, IEvaluationVisitor
    {
        Services.ReplaceTransient<IEvaluationVisitor,TEvaluationVisitor>();
        return this;
    }
    
    public NCalcServiceBuilder WithAsyncEvaluationVisitor<TAsyncEvaluationVisitor>() where TAsyncEvaluationVisitor : class, IAsyncEvaluationVisitor
    {
        Services.ReplaceTransient<IAsyncEvaluationVisitor,TAsyncEvaluationVisitor>();
        return this;
    }
    
    public NCalcServiceBuilder WithParameterExtractionVisitor<TParameterExtractionVisitor>() where TParameterExtractionVisitor : class, IParameterExtractionVisitor
    {
        Services.ReplaceTransient<IParameterExtractionVisitor,TParameterExtractionVisitor>();
        return this;
    }
}