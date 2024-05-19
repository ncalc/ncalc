using System;
using NCalc.Visitors;

namespace NCalc.Domain;

[Serializable]
public sealed class UnaryExpression(UnaryExpressionType type, LogicalExpression expression) : LogicalExpression
{
    public LogicalExpression Expression { get; } = expression;

    public UnaryExpressionType Type { get; } = type;

    public override void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}