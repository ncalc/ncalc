using Microsoft.Extensions.DependencyInjection;

namespace NCalc.DependencyInjection.Configuration;

public static class ServiceCollectionExtensions
{
    public static NCalcServiceBuilder AddNCalc(this IServiceCollection services)
    {
        return new NCalcServiceBuilder(services);
    }
}