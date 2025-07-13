using NCalc.Helpers;
using NCalc.Handlers;

namespace NCalc;

public record ExpressionContext
{
    public ExpressionOptions Options { get; set; } = ExpressionOptions.None;
    public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;
    public IDictionary<string, object?> StaticParameters { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, ExpressionParameter> DynamicParameters { get; set; } = new Dictionary<string, ExpressionParameter>();
    public IDictionary<string, ExpressionFunction> Functions { get; set; } = new Dictionary<string, ExpressionFunction>();

    public IDictionary<string, ExpressionAsyncParameter> AsyncDynamicParameters { get; set; } = new Dictionary<string, ExpressionAsyncParameter>();
    public IDictionary<string, ExpressionAsyncFunction> AsyncFunctions { get; set; } = new Dictionary<string, ExpressionAsyncFunction>();

    public EvaluateParameterHandler? EvaluateParameterHandler { get; set; }
    public EvaluateFunctionHandler? EvaluateFunctionHandler { get; set; }

    public ExpressionContext()
    {
    }

    public ExpressionContext(ExpressionOptions options, CultureInfo? cultureInfo)
    {
        Options = options;
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }

    public static implicit operator ExpressionContext(ExpressionOptions options) => new()
    {
        Options = options
    };

    public static implicit operator ExpressionContext(CultureInfo cultureInfo) => new()
    {
        CultureInfo = cultureInfo
    };

    public static implicit operator MathHelperOptions(ExpressionContext context)
    {
        return new MathHelperOptions(context.CultureInfo, context.Options);
    }

    public static implicit operator ComparisonOptions(ExpressionContext context)
    {
        return new ComparisonOptions(context.CultureInfo, context.Options);
    }
}