using NCalc.Handlers;
using NCalc.Helpers;

namespace NCalc;

public record ExpressionContext
{
    public IDictionary<string, object?> StaticParameters { get; set; }
    public IDictionary<string, ExpressionParameter> DynamicParameters { get; set; }
    public IDictionary<string, ExpressionFunction> Functions { get; set; }
    public IDictionary<string, AsyncExpressionFunction> AsyncFunctions { get; set; }

    public EvaluateBinaryHandler? EvaluateBinaryHandler { get; set; }
    public EvaluateBinaryAsyncHandler? EvaluateBinaryAsyncHandler { get; set; }
    public EvaluateParameterHandler? EvaluateParameterHandler { get; set; }
    public EvaluateFunctionHandler? EvaluateFunctionHandler { get; set; }
    public EvaluateAsyncFunctionHandler? EvaluateAsyncFunctionHandler { get; set; }

    public ExpressionOptions Options { get; set; }
    public CultureInfo CultureInfo { get; set; }

    public ExpressionContext() : this(ExpressionOptions.None, CultureInfo.CurrentCulture)
    {
    }

    public ExpressionContext(
        IDictionary<string, object?>? staticParameters,
        IDictionary<string, ExpressionParameter>? dynamicParameters = null,
        IDictionary<string, ExpressionFunction>? functions = null,
        IDictionary<string, AsyncExpressionFunction>? asyncFunctions = null)
        : this(
            ExpressionOptions.None,
            CultureInfo.CurrentCulture,
            staticParameters,
            dynamicParameters,
            functions,
            asyncFunctions)
    {
    }

    public ExpressionContext(
        ExpressionOptions options,
        CultureInfo? cultureInfo,
        IDictionary<string, object?>? staticParameters = null,
        IDictionary<string, ExpressionParameter>? dynamicParameters = null,
        IDictionary<string, ExpressionFunction>? functions = null,
        IDictionary<string, AsyncExpressionFunction>? asyncFunctions = null)
    {
        Options = options;
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;

        StaticParameters = staticParameters ?? new Dictionary<string, object?>();
        DynamicParameters = dynamicParameters ?? new Dictionary<string, ExpressionParameter>();
        Functions = functions ?? new Dictionary<string, ExpressionFunction>();
        AsyncFunctions = asyncFunctions ?? new Dictionary<string, AsyncExpressionFunction>();
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
