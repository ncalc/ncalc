using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class Function(Identifier identifier, LogicalExpressionList parameters) : LogicalExpression
{
    public Identifier Identifier { get; set; } = identifier;

    public LogicalExpressionList Parameters { get; set; } = parameters;

    public override T Accept<T>(ILogicalExpressionVisitor<T> visitor, CancellationToken ct = default)
    {
        return visitor.Visit(this, ct);
    }
}