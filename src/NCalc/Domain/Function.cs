using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class Function(Identifier identifier, LogicalExpression[] expressions) : LogicalExpression
{
    public Identifier Identifier { get;  } = identifier;

    public LogicalExpression[] Expressions { get; } = expressions;

    public override void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}