using NCalc.Exceptions;
using NCalc.Visitors;

namespace NCalc.Handlers;

/// <summary>
/// Provides data for binary expression evaluation events.
/// </summary>
public class BinaryEventArgs(
    BinaryExpression expression,
    ILogicalExpressionVisitor<object?> syncVisitor,
    ILogicalExpressionVisitor<Task<object?>> asyncVisitor,
    CancellationToken cancellationToken)
    : EventArgs
{
    private readonly ILogicalExpressionVisitor<object?>? _syncVisitor = syncVisitor;
    private readonly ILogicalExpressionVisitor<Task<object?>>? _asyncVisitor = asyncVisitor;

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

        return _leftResolvedValue = BinaryExpression.LeftExpression.Accept(_syncVisitor!, CancellationToken);
    }

    /// <summary>
    /// Lazily evaluates and returns the left side expression. Resolved only once.
    /// </summary>
    public async Task<object?> LeftValueAsync()
    {
        if (_asyncVisitor is null)
            throw new NCalcEvaluationException("Asynchronous binary value evaluation is not available in this context.");

        _leftResolvedValue ??= await BinaryExpression.LeftExpression.Accept(_asyncVisitor, CancellationToken);

        return _leftResolvedValue;
    }
    /// <summary>
    /// Lazily evaluates and returns the right side expression. Resolved only once.
    /// </summary>
    public object? RightValue()
    {
        if (_rightResolvedValue != null)
            return _rightResolvedValue;

        return _rightResolvedValue = BinaryExpression.RightExpression.Accept(_syncVisitor!, CancellationToken);
    }
    public async Task<object?> RightValueAsync()
    {
        if (_asyncVisitor is null)
            throw new NCalcEvaluationException("Asynchronous binary value evaluation is not available in this context.");

        _rightResolvedValue ??= await BinaryExpression.RightExpression.Accept(_asyncVisitor, CancellationToken);

        return _rightResolvedValue;
    }
}
