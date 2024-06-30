using NCalc.Domain;

namespace NCalc.Visitors;

public interface ILogicalExpressionVisitor<out T>
{
    T Visit(TernaryExpression expression);
    T Visit(BinaryExpression expression);
    T Visit(UnaryExpression expression);
    T Visit(ValueExpression expression);
    T Visit(Function function);
    T Visit(Identifier identifier);
}
