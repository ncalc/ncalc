using NCalc.Visitors;

namespace NCalc;

public sealed class Identifier(string name) : LogicalExpression
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = name;

    public override T Accept<T>(ILogicalExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}