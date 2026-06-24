using NCalc.Handlers;
using NCalc.Helpers;
using static NCalc.Helpers.EvaluationHelper;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to asynchronous evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
/// <param name="context">Contextual parameters of the <see cref="LogicalExpression"/>, like custom functions and parameters.</param>
public class AsyncEvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default, NCalc.Factories.IEvaluationVisitorFactory? evaluationVisitorFactory = null) : ILogicalExpressionVisitor<Task<object?>>
{
    protected CancellationToken CancellationToken { get; } = cancellationToken;
    protected NCalc.Factories.IEvaluationVisitorFactory? EvaluationVisitorFactory { get; } = evaluationVisitorFactory;

    protected EvaluationVisitor CreateEvaluationVisitor()
    {
        return EvaluationVisitorFactory?.CreateEvaluationVisitor(context, CancellationToken)
               ?? new EvaluationVisitor(context, CancellationToken);
    }

    public virtual async Task<object?> Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(await expression.LeftExpression.Accept(this), context.CultureInfo);

        if (left)
        {
            return await expression.MiddleExpression.Accept(this);
        }

        return await expression.RightExpression.Accept(this);
    }

    public virtual async Task<object?> Visit(BinaryExpression expression)
    {
        var binaryEventArgs = new BinaryEventArgs(expression, CreateEvaluationVisitor(), this, CancellationToken);
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

    public virtual async Task<object?> Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        var result = await expression.Expression.Accept(this);

        return Unary(expression, result, context);
    }

    public virtual async Task<object?> Visit(Function function)
    {
        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        var functionName = function.Identifier.Name;
        var syncEvaluationVisitor = CreateEvaluationVisitor();
        var functionData = new FunctionData(
            function.Identifier.Id,
            function.Parameters,
            context,
            syncEvaluationVisitor,
            this,
            CancellationToken);
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

    public virtual async Task<object?> Visit(Identifier identifier)
    {
        var value = EvaluationVisitorHelper.GetIdentifierValue(identifier, context, CancellationToken, EvaluationVisitorFactory);

        return value is Expression expression
            ? await expression.EvaluateAsync(CancellationToken)
            : value;
    }

    public virtual Task<object?> Visit(ValueExpression expression) => Task.FromResult(expression.Value);

    public virtual async Task<object?> Visit(LogicalExpressionList list)
    {
        if (list.Count == 0) return Array.Empty<object?>();

        var listCount = list.Count;
        var result = new object?[listCount];

        for (var index = 0; index < listCount; index++)
        {
            result[index] = await EvaluateAsync(list[index]);
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

    protected Task<object?> EvaluateAsync(LogicalExpression expression)
    {
        return expression.Accept(this);
    }
}
