using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class Function(Identifier identifier, LogicalExpression[] expressions) : LogicalExpression
{
    public Identifier Identifier { get; set; } = identifier;

    public LogicalExpression[] Expressions { get; set; } = expressions;

    public override void Accept(ILogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}