using NCalc.Factories;
using NCalc.Helpers;

namespace NCalc;

public abstract class ExpressionContextBase
{
    public ExpressionOptions Options { get; set; } = ExpressionOptions.None;
    public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;
    public IDictionary<string, object?> StaticParameters { get; set; } = new Dictionary<string, object?>();
    
    public static implicit operator MathHelperOptions(ExpressionContextBase context)
    {
        return new(context.CultureInfo,
            context.Options.HasFlag(ExpressionOptions.AllowBooleanCalculation),
            context.Options.HasFlag(ExpressionOptions.DecimalAsDefault),
            context.Options.HasFlag(ExpressionOptions.OverflowProtection));
    }
    
    public static implicit operator LogicalExpressionOptions(ExpressionContextBase context)
    {
        return new()
        {
            NumbersAsDecimal = context.Options.HasFlag(ExpressionOptions.DecimalAsDefault)
        };
    }
}