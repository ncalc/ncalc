using Microsoft.Extensions.DependencyInjection;
using NCalc.Cache;
using NCalc.Factories;

namespace NCalc.DependencyInjection;

public class NCalcServiceBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    public NCalcServiceBuilder WithExpressionFactory<TExpressionFactory>()
        where TExpressionFactory : class, IExpressionFactory
    {
        Services.ReplaceScoped<IExpressionFactory, TExpressionFactory>();
        return this;
    }

    public NCalcServiceBuilder WithAsyncExpressionFactory<TAsyncExpressionFactory>()
        where TAsyncExpressionFactory : class, IAsyncExpressionFactory
    {
        Services.ReplaceScoped<IAsyncExpressionFactory, TAsyncExpressionFactory>();
        return this;
    }

    public NCalcServiceBuilder WithCache<TCache>() where TCache : class, ILogicalExpressionCache
    {
        Services.ReplaceSingleton<ILogicalExpressionCache, TCache>();
        return this;
    }

    public NCalcServiceBuilder WithLogicalExpressionFactory<TLogicalExpressionFactory>()
        where TLogicalExpressionFactory : class, ILogicalExpressionFactory
    {
        Services.ReplaceSingleton<ILogicalExpressionFactory, TLogicalExpressionFactory>();
        return this;
    }

    public NCalcServiceBuilder WithEvaluationVisitorFactory<TEvaluationVisitorFactory>()
        where TEvaluationVisitorFactory : class, IEvaluationVisitorFactory
    {
        Services.ReplaceTransient<IEvaluationVisitorFactory, TEvaluationVisitorFactory>();
        return this;
    }

    public NCalcServiceBuilder WithAsyncEvaluationVisitorFactory<TAsyncEvaluationVisitorFactory>()
        where TAsyncEvaluationVisitorFactory : class, IAsyncEvaluationVisitorFactory
    {
        Services.ReplaceTransient<IAsyncEvaluationVisitorFactory, TAsyncEvaluationVisitorFactory>();
        return this;
    }
}