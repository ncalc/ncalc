using NCalc.Handlers;

namespace NCalc;

public record AsyncExpressionContext : ExpressionContextBase
{
    public IDictionary<string, AsyncExpressionParameter> DynamicParameters { get; set; } =
        new Dictionary<string, AsyncExpressionParameter>();

    public IDictionary<string, AsyncExpressionFunction> Functions { get; set; } =
        new Dictionary<string, AsyncExpressionFunction>();

    public AsyncEvaluateParameterHandler? AsyncEvaluateParameterHandler { get; set; }
    public AsyncEvaluateFunctionHandler? AsyncEvaluateFunctionHandler { get; set; }

    public AsyncExpressionContext()
    {
    }

    public AsyncExpressionContext(ExpressionOptions options, CultureInfo? cultureInfo)
    {
        Options = options;
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }

    public static implicit operator AsyncExpressionContext(ExpressionOptions options) => new()
    {
        Options = options
    };

    public static implicit operator AsyncExpressionContext(CultureInfo cultureInfo) => new()
    {
        CultureInfo = cultureInfo
    };
}