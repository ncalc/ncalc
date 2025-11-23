using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCalc.Cache;
using NCalc.Factories;

namespace NCalc.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal void ReplaceScoped<TService, TImplementation>() where TService : class where TImplementation : class, TService
        {
            services.Replace(ServiceDescriptor.Scoped<TService, TImplementation>());
        }

        internal void ReplaceTransient<TService, TImplementation>() where TService : class where TImplementation : class, TService
        {
            services.Replace(ServiceDescriptor.Transient<TService, TImplementation>());
        }

        internal void ReplaceSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService
        {
            services.Replace(ServiceDescriptor.Singleton<TService, TImplementation>());
        }

        public NCalcServiceBuilder AddNCalc()
        {
            services.AddLogging();

            services.AddFactories();

            services.AddCache();

            return new NCalcServiceBuilder(services);
        }

        private void AddCache()
        {
            services.AddSingleton<ILogicalExpressionCache, LogicalExpressionCache>();
        }

        private void AddFactories()
        {
            services.AddScoped<IExpressionFactory, ExpressionFactory>();
            services.AddScoped<IEvaluationVisitorFactory, EvaluationVisitorFactory>();
            services.AddScoped<IAsyncEvaluationVisitorFactory, AsyncEvaluationVisitorFactory>();
            services.AddScoped<IAsyncExpressionFactory, AsyncExpressionFactory>();

            services.AddSingleton<ILogicalExpressionFactory>(_ => LogicalExpressionFactory.GetInstance());
        }
    }
}