﻿using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class TernaryExpression(
    LogicalExpression leftExpression,
    LogicalExpression middleExpression,
    LogicalExpression rightExpression)
    : LogicalExpression
{
    public LogicalExpression LeftExpression { get; set; } = leftExpression;

    public LogicalExpression MiddleExpression { get; set; } = middleExpression;

    public LogicalExpression RightExpression { get; set; } = rightExpression;

    public override void Accept(ILogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}