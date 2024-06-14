using NCalc.Helpers;

namespace NCalc;

public abstract class ExpressionContextBase
{
    public ExpressionOptions Options { get; set; } = ExpressionOptions.None;
    public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;
    public Dictionary<string, object?> StaticParameters { get; set; } = new();
    
    public static implicit operator MathHelperOptions(ExpressionContextBase context)
    {
        return new(context.CultureInfo,
            context.Options.HasFlag(ExpressionOptions.AllowBooleanCalculation),
            context.Options.HasFlag(ExpressionOptions.DecimalAsDefault));
    }
}