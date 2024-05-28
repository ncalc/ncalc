using NCalc.Domain;

namespace NCalc.Visitors;

[Obsolete("Please use ILogicalExpressionVisitor.")]
public abstract class LogicalExpressionVisitor : ILogicalExpressionVisitor
{
    public abstract void Visit(LogicalExpression expression);
    public abstract void Visit(TernaryExpression expression);
    public abstract void Visit(BinaryExpression expression);
    public abstract void Visit(UnaryExpression expression);
    public abstract void Visit(ValueExpression expression);
    public abstract void Visit(Function function);
    public abstract void Visit(Identifier identifier);
}