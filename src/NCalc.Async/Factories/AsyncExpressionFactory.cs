using NCalc.Cache;
using NCalc.Domain;
using NCalc.Visitors;

namespace NCalc.Factories;

/// <summary>
/// Default <see cref="IAsyncExpressionFactory"/> implementation.
/// </summary>
public sealed class AsyncExpressionFactory(
    ILogicalExpressionFactory logicalExpressionFactory,
    ILogicalExpressionCache cache,
    IAsyncEvaluationVisitor asyncEvaluationVisitor,
    IParameterExtractionVisitor parameterExtractionVisitor
) : IAsyncExpressionFactory
{
    public AsyncExpression Create(string expression, AsyncExpressionContext? expressionContext = null)
    {
        return new AsyncAdvancedExpression(logicalExpressionFactory, cache, asyncEvaluationVisitor,
            parameterExtractionVisitor, expression, expressionContext);
    }

    public AsyncExpression Create(LogicalExpression logicalExpression, AsyncExpressionContext? expressionContext = null)
    {
        return new AsyncAdvancedExpression(logicalExpressionFactory, cache, asyncEvaluationVisitor,
            parameterExtractionVisitor, logicalExpression, expressionContext);
    }
}