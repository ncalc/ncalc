using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class Identifier(string name) : LogicalExpression
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = name;

    public override T Accept<T>(ILogicalExpressionVisitor<T> visitor, CancellationToken ct = default)
    {
        return visitor.Visit(this, ct);
    }
}