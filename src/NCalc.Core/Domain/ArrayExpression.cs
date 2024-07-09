using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class ArrayExpression(LogicalExpression[] values) : LogicalExpression
{
    public LogicalExpression[] Values { get; set; } = values;
    public override T Accept<T>(ILogicalExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}