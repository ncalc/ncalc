using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class Function(Identifier identifier, LogicalExpressionList parameters) : LogicalExpression
{
    public Identifier Identifier { get; set; } = identifier;

    public LogicalExpressionList Parameters { get; set; } = parameters;

    [Obsolete("Please use Parameters property.")]
    public LogicalExpression[] Expressions
    {
        get => Parameters.ToArray();
        set => Parameters = new LogicalExpressionList(value);
    }

    public override T Accept<T>(ILogicalExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}