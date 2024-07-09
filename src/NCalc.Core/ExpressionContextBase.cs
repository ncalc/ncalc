using NCalc.Helpers;

namespace NCalc;

public abstract record ExpressionContextBase
{
    public ExpressionOptions Options { get; set; } = ExpressionOptions.None;
    public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;
    public IDictionary<string, object?> StaticParameters { get; set; } = new Dictionary<string, object?>();

    public static implicit operator MathHelperOptions(ExpressionContextBase context)
    {
        return new MathHelperOptions(context.CultureInfo, context.Options);
    }

    public static implicit operator ComparisonOptions(ExpressionContextBase context)
    {
        return new ComparisonOptions(context.CultureInfo, context.Options);
    }
}