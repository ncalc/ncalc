using NCalc.DependencyInjection;

namespace NCalc.Antlr.Configuration;

public static class NCalcServiceBuilderExtensions
{
    /// <summary>
    /// Replaces the default parser (Parlot) with Antlr, the original parser of NCalc.
    /// </summary>
    /// <returns></returns>
    public static NCalcServiceBuilder WithAntlr(this NCalcServiceBuilder builder)
    {
        builder.WithLogicalExpressionFactory<AntlrLogicalExpressionFactory>();
        return builder;
    }
}