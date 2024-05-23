using NCalc.Domain;

namespace NCalc.Factories.Abstractions;

public interface ILogicalExpressionFactory
{
    public LogicalExpression Create(string expression, ExpressionOptions expressionOptions);
}