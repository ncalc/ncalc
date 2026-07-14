using NCalc.Handlers;

namespace NCalc;

/// <summary>
/// Per-evaluation runtime state, including parameters, functions, and event handlers.
/// </summary>
public sealed class ExpressionContext
{
    public IDictionary<string, object?> Parameters { get; init; }
    public IDictionary<string, ExpressionParameter> DynamicParameters { get; init; }
    public IDictionary<string, AsyncExpressionParameter> AsyncParameters { get; init; }
    public IDictionary<string, ExpressionFunction> Functions { get; init; }
    public IDictionary<string, AsyncExpressionFunction> AsyncFunctions { get; init; }

    public EvaluateBinaryHandler? EvaluateBinaryHandler { get; set; }
    public EvaluateBinaryAsyncHandler? EvaluateBinaryAsyncHandler { get; set; }
    public EvaluateParameterHandler? EvaluateParameterHandler { get; set; }
    public EvaluateAsyncParameterHandler? EvaluateAsyncParameterHandler { get; set; }
    public EvaluateFunctionHandler? EvaluateFunctionHandler { get; set; }
    public EvaluateAsyncFunctionHandler? EvaluateAsyncFunctionHandler { get; set; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public ExpressionContext(
        IDictionary<string, object?>? parameters = null,
        IDictionary<string, ExpressionParameter>? dynamicParameters = null,
        IDictionary<string, ExpressionFunction>? functions = null,
        IDictionary<string, AsyncExpressionFunction>? asyncFunctions = null,
        IDictionary<string, AsyncExpressionParameter>? asyncParameters = null)
    {
        Parameters = parameters ?? new Dictionary<string, object?>();
        DynamicParameters = dynamicParameters ?? new Dictionary<string, ExpressionParameter>();
        AsyncParameters = asyncParameters ?? new Dictionary<string, AsyncExpressionParameter>();
        Functions = functions ?? new Dictionary<string, ExpressionFunction>();
        AsyncFunctions = asyncFunctions ?? new Dictionary<string, AsyncExpressionFunction>();
    }
}
