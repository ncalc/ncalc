using NCalc.Visitors;

namespace NCalc.Handlers;

/// <summary>
/// Provides data for binary expression evaluation events.
/// </summary>
public class BinaryEventArgs(BinaryExpression expression, ILogicalExpressionVisitor<ValueTask<object?>> visitor, CancellationToken cancellationToken) : EventArgs
{
    /// <summary>
    /// Gets or sets the evaluation result of the binary expression.
    /// </summary>
    public object? Result
    {
        get;
        set
        {
            field = value;
            HasResult = true;
        }
    }

    public BinaryExpression BinaryExpression { get; } = expression;

    public bool HasResult { get; private set; }

    /// <summary>
    /// The cancellation token for the operation.
    /// </summary>
    public CancellationToken CancellationToken { get; } = cancellationToken;

    private object? _leftResolvedValue;
    private object? _rightResolvedValue;

    /// <summary>
    /// Lazily evaluates and returns the left side expression. Resolved only once.
    /// </summary>
    public object? LeftValue()
    {
        if (_leftResolvedValue != null)
            return _leftResolvedValue;
        var valueTask = BinaryExpression.LeftExpression.Accept(visitor, CancellationToken);
        if (valueTask.IsCompletedSuccessfully)
            return _leftResolvedValue = valueTask.Result;

        return _leftResolvedValue = valueTask.AsTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Lazily evaluates and returns the left side expression. Resolved only once.
    /// </summary>
    public async ValueTask<object?> LeftValueAsync()
    {
        _leftResolvedValue ??= await BinaryExpression.LeftExpression.Accept(visitor, CancellationToken);

        return _leftResolvedValue;
    }
    /// <summary>
    /// Lazily evaluates and returns the right side expression. Resolved only once.
    /// </summary>
    public object? RightValue()
    {
        if (_rightResolvedValue != null)
            return _rightResolvedValue;

        var valueTask = BinaryExpression.RightExpression.Accept(visitor, CancellationToken);
        if (valueTask.IsCompletedSuccessfully)
            return _rightResolvedValue = valueTask.Result;

        return _rightResolvedValue = valueTask.AsTask().GetAwaiter().GetResult();
    }
    public async ValueTask<object?> RightValueAsync()
    {
        _rightResolvedValue ??= await BinaryExpression.RightExpression.Accept(visitor, CancellationToken);

        return _rightResolvedValue;
    }
}