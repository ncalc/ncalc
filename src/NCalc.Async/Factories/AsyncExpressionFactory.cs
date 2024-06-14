using NCalc.Cache;
using NCalc.Domain;
using NCalc.Services;

namespace NCalc.Factories;

/// <summary>
/// Default <see cref="IAsyncExpressionFactory"/> implementation.
/// </summary>
public sealed class AsyncExpressionFactory(
    ILogicalExpressionFactory logicalExpressionFactory,
    ILogicalExpressionCache cache,
    IAsyncEvaluationService evaluationService
) : IAsyncExpressionFactory
{
    public AsyncExpression Create(string expression, AsyncExpressionContext? expressionContext = null)
    {
        return new AsyncExpression(expression, expressionContext ?? new(), logicalExpressionFactory, cache, evaluationService);
    }

    public AsyncExpression Create(LogicalExpression logicalExpression, AsyncExpressionContext? expressionContext = null)
    {
        return new AsyncExpression(logicalExpression, expressionContext ?? new(), logicalExpressionFactory, cache, evaluationService);
    }
}