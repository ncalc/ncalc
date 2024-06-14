namespace NCalc;

public class ExpressionContext : ExpressionContextBase
{
    public Dictionary<string, ExpressionParameter> DynamicParameters { get; set; } = new();

    public Dictionary<string, ExpressionFunction> Functions { get; set; } = new(ExpressionBuiltInFunctions.Values);

    public ExpressionContext()
    {
    }

    public ExpressionContext(ExpressionOptions options, CultureInfo? cultureInfo)
    {
        Options = options;
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }
    
    public static implicit operator ExpressionContext(ExpressionOptions options) => new() { Options = options };

    public static implicit operator ExpressionContext(CultureInfo cultureInfo) => new() { CultureInfo = cultureInfo };
}