using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class BinaryExpression(
        BinaryExpressionType type, 
        LogicalExpression leftExpression,
        LogicalExpression rightExpression) : LogicalExpression
{
    public LogicalExpression LeftExpression { get;  } = leftExpression;

    public LogicalExpression RightExpression { get;  } = rightExpression;

    public BinaryExpressionType Type { get; } = type;

    public override void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}