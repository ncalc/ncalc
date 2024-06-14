namespace NCalc;

public class AsyncExpressionContext : ExpressionContextBase
{
    public Dictionary<string, AsyncExpressionParameter> DynamicParameters { get; set; } = new();

    public Dictionary<string, AsyncExpressionFunction> Functions { get; set; } = new(AsyncExpressionBuiltInFunctions.Values);

    public AsyncExpressionContext()
    {
    }

    public AsyncExpressionContext(ExpressionOptions options, CultureInfo? cultureInfo)
    {
        Options = options;
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }
    
    public static implicit operator AsyncExpressionContext(ExpressionOptions options) => new() { Options = options };

    public static implicit operator AsyncExpressionContext(CultureInfo cultureInfo) => new() { CultureInfo = cultureInfo };
}