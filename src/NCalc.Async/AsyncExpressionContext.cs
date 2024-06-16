using NCalc.Handlers;

namespace NCalc;

public class AsyncExpressionContext : ExpressionContextBase
{
    public AsyncEvaluateFunctionHandler? AsyncEvaluateFunctionHandler { get; set; }
    public AsyncEvaluateParameterHandler? AsyncEvaluateParameterHandler { get; set; }
    
    public IDictionary<string, AsyncExpressionParameter> DynamicParameters { get; set; } = new Dictionary<string, AsyncExpressionParameter>();

    public IDictionary<string, AsyncExpressionFunction> Functions { get; set; } = new Dictionary<string, AsyncExpressionFunction>(AsyncExpressionBuiltInFunctions.Values);

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