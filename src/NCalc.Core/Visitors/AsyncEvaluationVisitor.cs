using NCalc.Handlers;
using NCalc.Helpers;
using static NCalc.Helpers.EvaluationHelper;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to asynchronous evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
/// <param name="context">Contextual parameters of the <see cref="LogicalExpression"/>, like custom functions and parameters.</param>
public class AsyncEvaluationVisitor(ExpressionContext context) : ILogicalExpressionVisitor<Task<object?>>
{
    public virtual async Task<object?> Visit(TernaryExpression expression, CancellationToken cancellationToken = default)
    {
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(await expression.LeftExpression.Accept(this, cancellationToken), context.CultureInfo);

        if (left)
        {
            return await expression.MiddleExpression.Accept(this, cancellationToken);
        }

        return await expression.RightExpression.Accept(this, cancellationToken);
    }

    public virtual async Task<object?> Visit(BinaryExpression expression, CancellationToken cancellationToken = default)
    {
        var binaryEventArgs = new BinaryEventArgs(expression, new EvaluationVisitor(context), this, cancellationToken);
        await OnEvaluateBinaryAsync(binaryEventArgs);

        if (binaryEventArgs.HasResult)
            return binaryEventArgs.Result;

        if (expression.Type == BinaryExpressionType.And)
        {
            return Convert.ToBoolean(await binaryEventArgs.LeftValueAsync(), context.CultureInfo) &&
                   Convert.ToBoolean(await binaryEventArgs.RightValueAsync(), context.CultureInfo);
        }

        if (expression.Type == BinaryExpressionType.Or)
        {
            return Convert.ToBoolean(await binaryEventArgs.LeftValueAsync(), context.CultureInfo) || Convert.ToBoolean(await binaryEventArgs.RightValueAsync(), context.CultureInfo);
        }

        var left = await binaryEventArgs.LeftValueAsync();
        var right = await binaryEventArgs.RightValueAsync();

        return EvaluationVisitorHelper.EvaluateBinary(expression.Type, left, right, context);
    }

    public virtual async Task<object?> Visit(UnaryExpression expression, CancellationToken cancellationToken = default)
    {
        // Recursively evaluates the underlying expression
        var result = await expression.Expression.Accept(this, cancellationToken);

        return Unary(expression, result, context);
    }

    public virtual async Task<object?> Visit(Function function, CancellationToken cancellationToken = default)
    {
        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        var functionName = function.Identifier.Name;
        var syncEvaluationVisitor = new EvaluationVisitor(context);
        var functionData = new FunctionData(
            function.Identifier.Id,
            function.Parameters,
            context,
            syncEvaluationVisitor,
            this,
            cancellationToken);
        var functionArgs = new FunctionEventArgs(functionData);

        await OnEvaluateFunctionAsync(functionName, functionArgs);

        if (functionArgs.HasResult)
            return functionArgs.Result;

        if (context.Functions.TryGetValue(functionName, out var expressionFunction))
            return expressionFunction(functionData);

        if (context.AsyncFunctions.TryGetValue(functionName, out var asyncExpressionFunction))
            return await asyncExpressionFunction(functionData);

        return await BuiltInFunctionHelper.EvaluateAsync(functionName, functionData);
    }

    public virtual async Task<object?> Visit(Identifier identifier, CancellationToken cancellationToken = default)
    {
        var value = EvaluationVisitorHelper.GetIdentifierValue(identifier, context, cancellationToken);

        return value is Expression expression
            ? await expression.EvaluateAsync(cancellationToken)
            : value;
    }

    public virtual Task<object?> Visit(ValueExpression expression, CancellationToken cancellationToken = default) => Task.FromResult(expression.Value);

    public virtual async Task<object?> Visit(LogicalExpressionList list, CancellationToken cancellationToken = default)
    {
        List<object?> result = [];

        foreach (var value in list)
        {
            result.Add(await EvaluateAsync(value, cancellationToken));
        }

        return result;
    }

    protected bool Compare(object? a, object? b, ComparisonType comparisonType)
    {
        return EvaluationVisitorHelper.Compare(a, b, comparisonType, context);
    }

    protected Task OnEvaluateFunctionAsync(string name, FunctionEventArgs args)
    {
        context.EvaluateFunctionHandler?.Invoke(name, args);
        if (args.HasResult)
            return Task.CompletedTask;

        return context.EvaluateAsyncFunctionHandler?.Invoke(name, args) ?? Task.CompletedTask;
    }
    protected Task OnEvaluateBinaryAsync(BinaryEventArgs args)
    {
        context.EvaluateBinaryHandler?.Invoke(args);
        if (args.HasResult)
            return Task.CompletedTask;

        return context.EvaluateBinaryAsyncHandler?.Invoke(args) ?? Task.CompletedTask;
    }

    protected Task<object?> EvaluateAsync(LogicalExpression expression, CancellationToken cancellationToken = default)
    {
        return expression.Accept(this, cancellationToken);
    }
}
