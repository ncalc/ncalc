using NCalc.Domain;

namespace NCalc.Factories;

public interface IAsyncExpressionFactory
{
    public AsyncExpression Create(
        string expression,
        ExpressionContext? expressionContext = null);

    public AsyncExpression Create(
        LogicalExpression logicalExpression,
        ExpressionContext? expressionContext = null);
}