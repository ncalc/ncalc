using NCalc.Domain;
using NCalc.Visitors;

namespace NCalc.Services;

/// <inheritdoc cref="IAsyncEvaluationService"/>
public class AsyncEvaluationService : IAsyncEvaluationService
{
    public ValueTask<object?> EvaluateAsync(LogicalExpression expression, AsyncExpressionContext context)
    {
        var visitor = new AsyncEvaluationVisitor(context);
        return expression.Accept(visitor);
    }
}