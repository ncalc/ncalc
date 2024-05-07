namespace NCalc.Domain;

public class BinaryExpression(
        BinaryExpressionType type, 
        LogicalExpression leftExpression,
        LogicalExpression rightExpression)
    
    : LogicalExpression
{
    public LogicalExpression LeftExpression { get; set; } = leftExpression;

    public LogicalExpression RightExpression { get; set; } = rightExpression;

    public BinaryExpressionType Type { get; set; } = type;

    public override void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}