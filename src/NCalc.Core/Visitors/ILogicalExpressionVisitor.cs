using NCalc.Domain;

namespace NCalc.Visitors;

/// <summary>
/// Defines methods to visit different types of logical expressions in an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="T">The type of result returned from each visit method.</typeparam>
public interface ILogicalExpressionVisitor<out T>
{
    T Visit(TernaryExpression expression);
    T Visit(BinaryExpression expression);
    T Visit(UnaryExpression expression);
    T Visit(ValueExpression expression);
    T Visit(Function function);
    T Visit(Identifier identifier);
    T Visit(LogicalExpressionList list);
}
