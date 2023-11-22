namespace NCalc.Domain;

public class UnaryExpression(UnaryExpressionType type, LogicalExpression expression) : LogicalExpression
{
    public LogicalExpression Expression { get; set; } = expression;

    public UnaryExpressionType Type { get; set; } = type;

    public override void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public enum UnaryExpressionType
{
    Not,
    Negate,
    BitwiseNot,
    Positive
}