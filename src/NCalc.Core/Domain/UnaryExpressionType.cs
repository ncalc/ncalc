namespace NCalc.Domain;

public enum UnaryExpressionType
{
    Not,
    Negate,
    BitwiseNot,
    Positive,
    SqRoot,
#if NET8_0_OR_GREATER
    CbRoot,
#endif
    FourthRoot
}