namespace NCalc.Domain;

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
    Exponentiation,
    In,
    NotIn,
    Like,
    NotLike,
    Unknown = -1
}