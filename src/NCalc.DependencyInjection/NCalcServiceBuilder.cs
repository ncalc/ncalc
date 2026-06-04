using Microsoft.Extensions.DependencyInjection;
using NCalc.Cache;
using NCalc.Factories;

#if NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace NCalc.DependencyInjection;

public class NCalcServiceBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    public NCalcServiceBuilder WithExpressionFactory<
        #if NET
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        #endif
        TExpressionFactory>()
        where TExpressionFactory : class, IExpressionFactory
    {
        Services.ReplaceScoped<IExpressionFactory, TExpressionFactory>();
        return this;
    }

    public NCalcServiceBuilder WithAsyncExpressionFactory<
        #if NET
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        #endif
        TAsyncExpressionFactory>()
        where TAsyncExpressionFactory : class, IAsyncExpressionFactory
    {
        Services.ReplaceScoped<IAsyncExpressionFactory, TAsyncExpressionFactory>();
        return this;
    }

    public NCalcServiceBuilder WithCache<
        #if NET
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        #endif
        TCache>() where TCache : class, ILogicalExpressionCache
    {
        Services.ReplaceSingleton<ILogicalExpressionCache, TCache>();
        return this;
    }

    public NCalcServiceBuilder WithLogicalExpressionFactory<
        #if NET
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        #endif
        TLogicalExpressionFactory>()
        where TLogicalExpressionFactory : class, ILogicalExpressionFactory
    {
        Services.ReplaceSingleton<ILogicalExpressionFactory, TLogicalExpressionFactory>();
        return this;
    }

    public NCalcServiceBuilder WithEvaluationVisitorFactory<
        #if NET
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        #endif
        TEvaluationVisitorFactory>()
        where TEvaluationVisitorFactory : class, IEvaluationVisitorFactory
    {
        Services.ReplaceTransient<IEvaluationVisitorFactory, TEvaluationVisitorFactory>();
        return this;
    }

    public NCalcServiceBuilder WithAsyncEvaluationVisitorFactory<
        #if NET
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        #endif
        TAsyncEvaluationVisitorFactory>()
        where TAsyncEvaluationVisitorFactory : class, IAsyncEvaluationVisitorFactory
    {
        Services.ReplaceTransient<IAsyncEvaluationVisitorFactory, TAsyncEvaluationVisitorFactory>();
        return this;
    }
}