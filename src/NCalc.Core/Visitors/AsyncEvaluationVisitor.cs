using System.Runtime.CompilerServices;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Exceptions;
using static NCalc.Helpers.EvaluationHelper;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to asynchronous evaluating <see cref="LogicalExpression"/>.
/// </summary>
public class AsyncEvaluationVisitor(
    ExpressionContext context,
    ExpressionEvaluationOptions options,
    CultureInfo cultureInfo,
    IEvaluationVisitorFactory? evaluationVisitorFactory = null,
    CancellationToken cancellationToken = default) : ILogicalExpressionVisitor<Task<object?>>
{
    protected CancellationToken CancellationToken { get; } = cancellationToken;
    protected IEvaluationVisitorFactory? EvaluationVisitorFactory { get; } = evaluationVisitorFactory;

    protected EvaluationVisitor CreateEvaluationVisitor()
    {
        return EvaluationVisitorFactory?.CreateEvaluationVisitor(context, options, cultureInfo, CancellationToken)
               ?? new EvaluationVisitor(context, options, cultureInfo, cancellationToken: CancellationToken);
    }

    public virtual async Task<object?> Visit(TernaryExpression expression)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(await expression.LeftExpression.Accept(this), cultureInfo);

        if (left)
        {
            return await expression.MiddleExpression.Accept(this);
        }

        return await expression.RightExpression.Accept(this);
    }

    public virtual async Task<object?> Visit(BinaryExpression expression)
    {
		RuntimeHelpers.EnsureSufficientExecutionStack();

        if (context.EvaluateBinaryHandler != null ||
            context.EvaluateBinaryAsyncHandler != null ||
            GetType() != typeof(AsyncEvaluationVisitor))
        {
            var binaryEventArgs = new BinaryEventArgs(expression, CreateEvaluationVisitor(), this, CancellationToken);
            await OnEvaluateBinaryAsync(binaryEventArgs);

            if (binaryEventArgs.HasResult)
                return binaryEventArgs.Result;

            return await EvaluateBinaryCoreAsync(expression.Type, binaryEventArgs);
        }

        return await EvaluateBinaryCoreAsync(expression.Type, expression.LeftExpression, expression.RightExpression);
    }

    private async Task<object?> EvaluateBinaryCoreAsync(BinaryExpressionType type, BinaryEventArgs args)
    {
        return type switch
        {
            BinaryExpressionType.And => BoxedValues.Box(Convert.ToBoolean(await args.LeftValueAsync(), cultureInfo) && Convert.ToBoolean(await args.RightValueAsync(), cultureInfo)),
            BinaryExpressionType.Or => BoxedValues.Box(Convert.ToBoolean(await args.LeftValueAsync(), cultureInfo) || Convert.ToBoolean(await args.RightValueAsync(), cultureInfo)),
            BinaryExpressionType.Coalesce => await args.LeftValueAsync() ?? await args.RightValueAsync(),
                _ => await EvaluateStandardBinaryAsync(type, args)
        };
    }

    private async Task<object?> EvaluateBinaryCoreAsync(BinaryExpressionType type, LogicalExpression left, LogicalExpression right)
    {
        return type switch
        {
            BinaryExpressionType.And => BoxedValues.Box(Convert.ToBoolean(await left.Accept(this), cultureInfo) && Convert.ToBoolean(await right.Accept(this), cultureInfo)),
            BinaryExpressionType.Or => BoxedValues.Box(Convert.ToBoolean(await left.Accept(this), cultureInfo) || Convert.ToBoolean(await right.Accept(this), cultureInfo)),
            BinaryExpressionType.Coalesce => await left.Accept(this) ?? await right.Accept(this),
                _ => await EvaluateStandardBinaryAsync(type, left, right)
        };
    }

    private async Task<object?> EvaluateStandardBinaryAsync(BinaryExpressionType type, BinaryEventArgs args)
    {
        if (options.ConcurrentAsyncEvaluation)
        {
            var leftTask = args.LeftValueAsync();
            var rightTask = args.RightValueAsync();
            await Task.WhenAll(leftTask, rightTask);
            return EvaluationVisitorHelper.EvaluateBinary(type, leftTask.Result, rightTask.Result, options, cultureInfo);
        }

        var leftValue = await args.LeftValueAsync();
        var rightValue = await args.RightValueAsync();
        return EvaluationVisitorHelper.EvaluateBinary(type, leftValue, rightValue, options, cultureInfo);
    }

    private async Task<object?> EvaluateStandardBinaryAsync(BinaryExpressionType type, LogicalExpression left, LogicalExpression right)
    {
        if (options.ConcurrentAsyncEvaluation)
        {
            var leftTask = left.Accept(this);
            var rightTask = right.Accept(this);
            await Task.WhenAll(leftTask, rightTask);
            return EvaluationVisitorHelper.EvaluateBinary(type, leftTask.Result, rightTask.Result, options, cultureInfo);
        }

        var leftValue = await left.Accept(this);
        var rightValue = await right.Accept(this);
        return EvaluationVisitorHelper.EvaluateBinary(type, leftValue, rightValue, options, cultureInfo);
    }

    public virtual async Task<object?> Visit(UnaryExpression expression)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        // Recursively evaluates the underlying expression
        var result = await expression.Expression.Accept(this);

        return Unary(expression, result, options, cultureInfo);
    }

    public virtual async Task<object?> Visit(Function function)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();

        var functionName = function.Identifier.Name;
        var hasHandler = context.EvaluateFunctionHandler != null ||
                         context.EvaluateAsyncFunctionHandler != null ||
                         GetType() != typeof(AsyncEvaluationVisitor);

        ExpressionFunction? expressionFunction = null;
        AsyncExpressionFunction? asyncExpressionFunction = null;

        if (!hasHandler)
        {
            context.Functions.TryGetValue(functionName, out expressionFunction);
            context.AsyncFunctions.TryGetValue(functionName, out asyncExpressionFunction);
        }

        var syncVisitor = (hasHandler || expressionFunction != null || asyncExpressionFunction != null)
            ? CreateEvaluationVisitor() : null;

        var functionData = new FunctionData(
            function.Identifier.Id,
            function.Parameters,
            context,
            options,
            cultureInfo,
            syncVisitor,
            this,
            CancellationToken);

        if (hasHandler)
        {
            var functionArgs = new FunctionEventArgs(functionData);
            await OnEvaluateFunctionAsync(functionName, functionArgs);

            if (functionArgs.HasResult)
                return functionArgs.Result;

            context.TryGetFunction(functionName, out expressionFunction);
            context.TryGetAsyncFunction(functionName, out asyncExpressionFunction);
        }

        if (expressionFunction != null)
            return expressionFunction(functionData);

        if (asyncExpressionFunction != null)
            return asyncExpressionFunction(functionData);

        return await BuiltInFunctionHelper.EvaluateAsync(functionName, functionData);
    }

    public virtual async Task<object?> Visit(Identifier identifier)
    {
        var value = await GetIdentifierValueAsync(identifier);

        return value is Expression expression
            ? await expression.EvaluateAsync(CancellationToken)
            : value;
    }

    public virtual Task<object?> Visit(ValueExpression expression) => Task.FromResult(expression.Value);

    public virtual async Task<object?> Visit(LogicalExpressionList list)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        if (list.Count == 0) return Array.Empty<object?>();

        if (options.ConcurrentAsyncEvaluation)
            return await Task.WhenAll(list.Select(EvaluateAsync));

        var listCount = list.Count;
        var result = new object?[listCount];

        for (var index = 0; index < listCount; index++)
        {
            result[index] = await EvaluateAsync(list[index]);
        }

        return result;
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

    private async Task<object?> GetIdentifierValueAsync(Identifier identifier)
    {
        var identifierName = identifier.Name;

        if (context.EvaluateParameterHandler != null || context.EvaluateAsyncParameterHandler != null)
        {
            var parameterArgs = new ParameterEventArgs(identifier.Id, CancellationToken);
            context.EvaluateParameterHandler?.Invoke(identifierName, parameterArgs);

            if (!parameterArgs.HasResult)
                await (context.EvaluateAsyncParameterHandler?.Invoke(identifierName, parameterArgs) ?? Task.CompletedTask);

            if (parameterArgs.HasResult)
                return parameterArgs.Result;
        }

        if (context.TryGetParameter(identifierName, out var parameter))
        {
            if (parameter is Expression expression)
                return await EvaluateChildExpressionAsync(expression);

            return parameter;
        }

        if (context.TryGetDynamicParameter(identifierName, out var dynamicParameter))
            return dynamicParameter(new ParameterData(identifier.Id, context, CancellationToken));

        if (context.TryGetAsyncParameter(identifierName, out var asyncParameter))
            return await asyncParameter(new ParameterData(identifier.Id, context, CancellationToken));

        if (identifierName.Equals("null", StringComparison.InvariantCultureIgnoreCase) &&
            options.AllowNullParameter)
        {
            return null;
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    private async Task<object?> EvaluateChildExpressionAsync(Expression expression)
    {
        var childContext = expression.Context;
        context.ShareParametersWith(childContext);
        expression.SetEvaluationVisitorFactory(EvaluationVisitorFactory);

        using (childContext.AttachHandlersFrom(context))
        {
            return await expression.EvaluateAsync(CancellationToken);
        }
    }
}
