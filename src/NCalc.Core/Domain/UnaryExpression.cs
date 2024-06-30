using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class UnaryExpression(UnaryExpressionType type, LogicalExpression expression) : LogicalExpression
{
    public LogicalExpression Expression { get; set; } = expression;

    public UnaryExpressionType Type { get; set; } = type;

    public override T Accept<T>(ILogicalExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}