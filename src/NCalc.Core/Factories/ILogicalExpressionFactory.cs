﻿using NCalc.Domain;

namespace NCalc.Factories;

public interface ILogicalExpressionFactory
{
    public LogicalExpression Create(string expression, ExpressionContext? expressionContext = null);
}