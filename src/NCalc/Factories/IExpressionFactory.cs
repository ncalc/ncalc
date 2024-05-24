using NCalc.Domain;

namespace NCalc.Factories;

public interface IExpressionFactory
{
    public Expression Create(
        string expression,
        Action<ExpressionContext>? configure = null);
    
    public Expression Create(
        LogicalExpression logicalExpression,
        Action<ExpressionContext>? configure = null);
}