using NCalc.Domain;
using NCalc.Handlers;
using NCalc.Visitors;

namespace NCalc.Services;

/// <inheritdoc cref="IAsyncEvaluationService"/>
public class AsyncEvaluationService : IAsyncEvaluationService
{
    public event AsyncEvaluateFunctionHandler? EvaluateFunctionAsync;
    public event AsyncEvaluateParameterHandler? EvaluateParameterAsync;

    public Task<object?> EvaluateAsync(LogicalExpression expression, AsyncExpressionContext context)
    {
        var visitor = new AsyncEvaluationVisitor(context);
        visitor.EvaluateFunctionAsync += EvaluateFunctionAsync;
        visitor.EvaluateParameterAsync += EvaluateParameterAsync;
        return expression.Accept(visitor);
    }
}