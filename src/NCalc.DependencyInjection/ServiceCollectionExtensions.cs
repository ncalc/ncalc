﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCalc.Cache;
using NCalc.Factories;
using NCalc.Visitors;

namespace NCalc.DependencyInjection;

public static class ServiceCollectionExtensions
{
    internal static void ReplaceScoped<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
    {
        services.Replace(ServiceDescriptor.Scoped<TService, TImplementation>());
    }

    internal static void ReplaceTransient<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
    {
        services.Replace(ServiceDescriptor.Transient<TService, TImplementation>());
    }

    internal static void ReplaceSingleton<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
    {
        services.Replace(ServiceDescriptor.Singleton<TService, TImplementation>());
    }

    public static NCalcServiceBuilder AddNCalc(this IServiceCollection services)
    {
        services.AddLogging();

        services.AddFactories();

        services.AddCache();

        return new NCalcServiceBuilder(services);
    }

    private static void AddCache(this IServiceCollection services)
    {
        services.AddSingleton<ILogicalExpressionCache, LogicalExpressionCache>();
    }

    private static void AddFactories(this IServiceCollection services)
    {
        services.AddScoped<IExpressionFactory, ExpressionFactory>();
        services.AddScoped<IEvaluationVisitorFactory, EvaluationVisitorFactory>();
        services.AddScoped<IAsyncEvaluationVisitorFactory, AsyncEvaluationVisitorFactory>();
        services.AddScoped<IAsyncExpressionFactory, AsyncExpressionFactory>();

        services.AddSingleton<ILogicalExpressionFactory>(_ => LogicalExpressionFactory.GetInstance());
    }
}