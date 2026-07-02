using NCalc.Handlers;
using NCalc.Helpers;

namespace NCalc;

public record ExpressionContext
{
    private MathHelperOptions? _mathHelperOptions;
    private ComparisonOptions? _comparisonOptions;

    public IDictionary<string, object?> StaticParameters { get; set; }
    public IDictionary<string, ExpressionParameter> DynamicParameters { get; set; }
    public IDictionary<string, AsyncExpressionParameter> AsyncParameters { get; set; }
    public IDictionary<string, ExpressionFunction> Functions { get; set; }
    public IDictionary<string, AsyncExpressionFunction> AsyncFunctions { get; set; }

    public EvaluateBinaryHandler? EvaluateBinaryHandler { get; set; }
    public EvaluateBinaryAsyncHandler? EvaluateBinaryAsyncHandler { get; set; }
    public EvaluateParameterHandler? EvaluateParameterHandler { get; set; }
    public EvaluateAsyncParameterHandler? EvaluateAsyncParameterHandler { get; set; }
    public EvaluateFunctionHandler? EvaluateFunctionHandler { get; set; }
    public EvaluateAsyncFunctionHandler? EvaluateAsyncFunctionHandler { get; set; }

    public ExpressionOptions Options
    {
        get;
        set
        {
            field = value;
            ResetHelperOptions();
        }
    }

    public CultureInfo CultureInfo
    {
        get;
        set
        {
            field = value;
            ResetHelperOptions();
        }
    } = CultureInfo.CurrentCulture;

    public MathHelperOptions MathHelperOptions => _mathHelperOptions ??= new MathHelperOptions(CultureInfo, Options);
    public ComparisonOptions ComparisonOptions => _comparisonOptions ??= new ComparisonOptions(CultureInfo, Options);

    public ExpressionContext() : this(ExpressionOptions.None, CultureInfo.CurrentCulture)
    {
    }

    public ExpressionContext(
        IDictionary<string, object?>? staticParameters,
        IDictionary<string, ExpressionParameter>? dynamicParameters = null,
        IDictionary<string, ExpressionFunction>? functions = null,
        IDictionary<string, AsyncExpressionFunction>? asyncFunctions = null,
        IDictionary<string, AsyncExpressionParameter>? asyncParameters = null)
        : this(
            ExpressionOptions.None,
            CultureInfo.CurrentCulture,
            staticParameters,
            dynamicParameters,
            functions,
            asyncFunctions,
            asyncParameters)
    {
    }

    public ExpressionContext(
        ExpressionOptions options,
        CultureInfo? cultureInfo,
        IDictionary<string, object?>? staticParameters = null,
        IDictionary<string, ExpressionParameter>? dynamicParameters = null,
        IDictionary<string, ExpressionFunction>? functions = null,
        IDictionary<string, AsyncExpressionFunction>? asyncFunctions = null,
        IDictionary<string, AsyncExpressionParameter>? asyncParameters = null)
    {
        Options = options;
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;

        StaticParameters = staticParameters ?? new Dictionary<string, object?>();
        DynamicParameters = dynamicParameters ?? new Dictionary<string, ExpressionParameter>();
        AsyncParameters = asyncParameters ?? new Dictionary<string, AsyncExpressionParameter>();
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

    [Obsolete("Use ExpressionContext.MathHelperOptions instead.")]
    public static implicit operator MathHelperOptions(ExpressionContext context)
    {
        return context.MathHelperOptions;
    }

    [Obsolete("Use ExpressionContext.ComparisonOptions instead.")]
    public static implicit operator ComparisonOptions(ExpressionContext context)
    {
        return context.ComparisonOptions;
    }

    private void ResetHelperOptions()
    {
        _mathHelperOptions = null;
        _comparisonOptions = null;
    }
}
