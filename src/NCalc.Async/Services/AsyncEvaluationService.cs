using NCalc.Domain;
using NCalc.Extensions;
using NCalc.Visitors;

namespace NCalc.Services;

/// <inheritdoc cref="IAsyncEvaluationService"/>
public class AsyncEvaluationService : IAsyncEvaluationService
{
    public async Task<object?> EvaluateAsync(LogicalExpression expression, AsyncExpressionContext context)
    {
        var visitor = new AsyncEvaluationVisitor(context);
        await expression.AcceptAsync(visitor);
        return visitor.Result;
    }
}