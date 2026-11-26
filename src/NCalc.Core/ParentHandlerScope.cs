using NCalc.Handlers;

namespace NCalc;

/// <summary>
/// Attaches a parent context's event handlers to a child context for the duration of a nested evaluation and
/// detaches them again when disposed, so the child is left with only its own handlers.
/// </summary>
internal readonly struct ParentHandlerScope : IDisposable
{
    private readonly ExpressionContext _child;
    private readonly EvaluateFunctionHandler? _function;
    private readonly EvaluateAsyncFunctionHandler? _asyncFunction;
    private readonly EvaluateParameterHandler? _parameter;
    private readonly EvaluateAsyncParameterHandler? _asyncParameter;
    private readonly EvaluateBinaryHandler? _binary;
    private readonly EvaluateBinaryAsyncHandler? _binaryAsync;

    public ParentHandlerScope(ExpressionContext child, ExpressionContext parent)
    {
        _child = child;
        _function = parent.EvaluateFunctionHandler;
        _asyncFunction = parent.EvaluateAsyncFunctionHandler;
        _parameter = parent.EvaluateParameterHandler;
        _asyncParameter = parent.EvaluateAsyncParameterHandler;
        _binary = parent.EvaluateBinaryHandler;
        _binaryAsync = parent.EvaluateBinaryAsyncHandler;

        // Combining or removing a null delegate is a no-op, so unset parent handlers need no special casing.
        child.EvaluateFunctionHandler += _function;
        child.EvaluateAsyncFunctionHandler += _asyncFunction;
        child.EvaluateParameterHandler += _parameter;
        child.EvaluateAsyncParameterHandler += _asyncParameter;
        child.EvaluateBinaryHandler += _binary;
        child.EvaluateBinaryAsyncHandler += _binaryAsync;
    }

    public void Dispose()
    {
        _child.EvaluateFunctionHandler -= _function;
        _child.EvaluateAsyncFunctionHandler -= _asyncFunction;
        _child.EvaluateParameterHandler -= _parameter;
        _child.EvaluateAsyncParameterHandler -= _asyncParameter;
        _child.EvaluateBinaryHandler -= _binary;
        _child.EvaluateBinaryAsyncHandler -= _binaryAsync;
    }
}
