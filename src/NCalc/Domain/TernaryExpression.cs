using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class TernaryExpression(LogicalExpression leftExpression, LogicalExpression middleExpression,
        LogicalExpression rightExpression)
    : LogicalExpression
{
    public LogicalExpression LeftExpression { get; } = leftExpression;

    public LogicalExpression MiddleExpression { get; } = middleExpression;

    public LogicalExpression RightExpression { get; } = rightExpression;

    public override void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}