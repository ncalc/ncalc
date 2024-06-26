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
        return new MathHelperOptions()
        {
            CultureInfo = context.CultureInfo,
            EnableBooleanCalculation = context.Options.HasFlag(ExpressionOptions.AllowBooleanCalculation),
            UseDecimals = context.Options.HasFlag(ExpressionOptions.DecimalAsDefault),
            OverflowProtection = context.Options.HasFlag(ExpressionOptions.OverflowProtection)
        };
    }

    public static implicit operator ComparisonOptions(ExpressionContextBase context)
    {
        return new ComparisonOptions
        {
            CultureInfo = context.CultureInfo,
            IsCaseInsensitive = context.Options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer),
            IsOrdinal = context.Options.HasFlag(ExpressionOptions.OrdinalStringComparer)
        };
    }
    
    public static implicit operator LogicalExpressionOptions(ExpressionContextBase context)
    {
        return new()
        {
            NumbersAsDecimal = context.Options.HasFlag(ExpressionOptions.DecimalAsDefault)
        };
    }
}