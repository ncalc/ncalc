
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;
using NCalc.Visitors;

namespace NCalc;

/// <summary>
/// Represents an abstract syntax tree (AST) node for logical expressions.
/// </summary>
#if NET
[JsonPolymorphic]
[JsonDerivedType(typeof(BinaryExpression), typeDiscriminator: "binary")]
[JsonDerivedType(typeof(Function), typeDiscriminator: "function")]
[JsonDerivedType(typeof(Identifier), typeDiscriminator: "identifier")]
[JsonDerivedType(typeof(LogicalExpressionList), typeDiscriminator: "list")]
[JsonDerivedType(typeof(TernaryExpression), typeDiscriminator: "ternary")]
[JsonDerivedType(typeof(UnaryExpression), typeDiscriminator: "unary")]
[JsonDerivedType(typeof(ValueExpression), typeDiscriminator: "value")]
#endif
public abstract class LogicalExpression
{
    [Pure]
    public abstract T Accept<T>(ILogicalExpressionVisitor<T> visitor, CancellationToken ct = default);
}