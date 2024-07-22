using NCalc.Cache;
using NCalc.Domain;
using NCalc.Services;

namespace NCalc.Factories;

/// <summary>
/// Default <see cref="IExpressionFactory"/> implementation.
/// </summary>
public sealed class ExpressionFactory(
    ILogicalExpressionFactory logicalExpressionFactory,
    ILogicalExpressionCache cache,
    IEvaluationService evaluationService
) : IExpressionFactory
{
    public Expression Create(string expression, ExpressionContext? expressionContext = null)
    {
        return new Expression(expression, expressionContext ?? new(), logicalExpressionFactory, cache, evaluationService);
    }

    public Expression Create(LogicalExpression logicalExpression, ExpressionContext? expressionContext = null)
    {
        return new Expression(logicalExpression, expressionContext ?? new(), logicalExpressionFactory, cache, evaluationService);
    }
}