using Microsoft.Extensions.DependencyInjection;
using NCalc.Factories;
using NCalc.Visitors;

namespace NCalc.DependencyInjection.Configuration;

public class NCalcServiceBuilder(IServiceCollection services)
{
    public NCalcServiceBuilder WithCache<TCache>() where TCache : class
    {
        return this;
    }

    public NCalcServiceBuilder WithParser<TParser>() where TParser : class
    {
        return this;
    }

    public NCalcServiceBuilder WithLogicalExpressionFactory<TEvaluationVisitor>() where TEvaluationVisitor : class, ILogicalExpressionFactory
    {
        return this;
    }
    
    public NCalcServiceBuilder WithEvaluationVisitor<TEvaluationVisitor>() where TEvaluationVisitor : class, IEvaluationVisitor
    {
        return this;
    }
}