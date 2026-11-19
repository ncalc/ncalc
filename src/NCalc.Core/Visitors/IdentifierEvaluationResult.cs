namespace NCalc.Visitors;

internal readonly struct IdentifierEvaluationResult(object? value, ExpressionContext? childContext = null)
{
    public object? Value { get; } = value;

    public ExpressionContext? ChildContext { get; } = childContext;
}
