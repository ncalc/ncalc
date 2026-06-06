namespace NCalc.Visitors;

/// <summary>
/// Defines methods to visit different types of logical expressions in an abstract syntax tree (AST).
/// </summary>
/// <typeparam name="T">The type of result returned from each visit method.</typeparam>
public interface ILogicalExpressionVisitor<out T>
{
    T Visit(TernaryExpression expression, CancellationToken cancellationToken = default);
    T Visit(BinaryExpression expression, CancellationToken cancellationToken = default);
    T Visit(UnaryExpression expression, CancellationToken cancellationToken = default);
    T Visit(ValueExpression expression, CancellationToken cancellationToken = default);
    T Visit(Function function, CancellationToken cancellationToken = default);
    T Visit(Identifier identifier, CancellationToken cancellationToken = default);
    T Visit(LogicalExpressionList list, CancellationToken cancellationToken = default);
}
