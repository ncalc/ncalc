using NCalc.Domain;

namespace NCalc.Visitors;

/// <summary>
/// Defines methods to visit different types of logical expressions in an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="T">The type of result returned from each visit method.</typeparam>
public interface ILogicalExpressionVisitor<out T>
{
    T Visit(TernaryExpression expression, CancellationToken ct = default);
    T Visit(BinaryExpression expression, CancellationToken ct = default);
    T Visit(UnaryExpression expression, CancellationToken ct = default);
    T Visit(ValueExpression expression, CancellationToken ct = default);
    T Visit(Function function, CancellationToken ct = default);
    T Visit(Identifier identifier, CancellationToken ct = default);
    T Visit(LogicalExpressionList list, CancellationToken ct = default);
}
