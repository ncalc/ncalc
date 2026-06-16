using NCalc.Visitors;

namespace NCalc.Handlers;

/// <summary>
/// Provides data for binary expression evaluation events.
/// </summary>
public class BinaryEventArgs(BinaryExpression exp, ILogicalExpressionVisitor<ValueTask<object?>> visitor, CancellationToken ct) : EventArgs
{
    /// <summary>
    /// Gets or sets the evaluation result of the binary expression.
    /// </summary>
    public object? Result
    {
        get => field;
        set
        {
            field = value;
            HasResult = true;
        }
    }

    public BinaryExpression BinaryExpression { get; } = exp;

    public bool HasResult { get; private set; }

    /// <summary>
    /// The cancellation token for the operation.
    /// </summary>
    public CancellationToken Ct { get; } = ct;

    private object? _leftResolvedValue;
    private object? _rightResolvedValue;

    /// <summary>
    /// Lazily evaluates and returns the left side expression. Resolved only once.
    /// </summary>
    public object? LeftValue()
    {
        if (_leftResolvedValue == null)
        {
            var valueTask = BinaryExpression.LeftExpression.Accept(visitor, Ct);
            if (valueTask.IsCompletedSuccessfully)
                return _leftResolvedValue = valueTask.Result;

            return _leftResolvedValue = valueTask.AsTask().GetAwaiter().GetResult();
        }

        return _leftResolvedValue;
    }

    /// <summary>
    /// Lazily evaluates and returns the left side expression. Resolved only once.
    /// </summary>
    public async ValueTask<object?> LeftValueAsync()
    {
        if (_leftResolvedValue == null)
        {
            _leftResolvedValue = await BinaryExpression.LeftExpression.Accept(visitor, Ct);
        }

        return _leftResolvedValue;
    }
    /// <summary>
    /// Lazily evaluates and returns the right side expression. Resolved only once.
    /// </summary>
    public object? RightValue()
    {
        if (_rightResolvedValue == null)
        {
            var valueTask = BinaryExpression.RightExpression.Accept(visitor, Ct);
            if (valueTask.IsCompletedSuccessfully)
                return _rightResolvedValue = valueTask.Result;

            return _rightResolvedValue = valueTask.AsTask().GetAwaiter().GetResult();
        }

        return _rightResolvedValue;
    }
    public async ValueTask<object?> RightValueAsync()
    {
        if (_rightResolvedValue == null)
        {
            _rightResolvedValue = await BinaryExpression.RightExpression.Accept(visitor, Ct);
        }

        return _rightResolvedValue;
    }
}