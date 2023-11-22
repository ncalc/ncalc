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

public enum BinaryExpressionType
{
    And,
    Or,
    NotEqual,
    LesserOrEqual,
    GreaterOrEqual,
    Lesser,
    Greater,
    Equal,
    Minus,
    Plus,
    Modulo,
    Div,
    Times,
    BitwiseOr,
    BitwiseAnd,
    BitwiseXOr,
    LeftShift,
    RightShift,
    Unknown,
    Exponentiation
}