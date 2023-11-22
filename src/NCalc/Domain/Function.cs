namespace NCalc.Domain;

public class Function(Identifier identifier, LogicalExpression[] expressions) : LogicalExpression
{
    public Identifier Identifier { get; set; } = identifier;

    public LogicalExpression[] Expressions { get; set; } = expressions;

    public override void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}