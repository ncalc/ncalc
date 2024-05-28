
using NCalc.Domain;

namespace NCalc.Visitors;

public interface ILogicalExpressionVisitor
{
    public void Visit(LogicalExpression expression);
    public void Visit(TernaryExpression expression);
    public void Visit(BinaryExpression expression);
    public void Visit(UnaryExpression expression);
    public void Visit(ValueExpression expression);
    public void Visit(Function function);
    public void Visit(Identifier identifier);
}