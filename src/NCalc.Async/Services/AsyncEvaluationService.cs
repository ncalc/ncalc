using NCalc.Domain;
using NCalc.Extensions;
using NCalc.Handlers;
using NCalc.Visitors;

namespace NCalc.Services;

/// <inheritdoc cref="IAsyncEvaluationService"/>
public class AsyncEvaluationService : IAsyncEvaluationService
{
    public event AsyncEvaluateFunctionHandler? EvaluateFunctionAsync;
    public event AsyncEvaluateParameterHandler? EvaluateParameterAsync;

    public async Task<object?> EvaluateAsync(LogicalExpression expression, AsyncExpressionContext context)
    {
        var visitor = new AsyncEvaluationVisitor(context);
        visitor.EvaluateFunctionAsync += EvaluateFunctionAsync;
        visitor.EvaluateParameterAsync += EvaluateParameterAsync;
        await expression.AcceptAsync(visitor);
        return visitor.Result;
    }
}