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
        var binaryEventArgs = new BinaryEventArgs(expression, CreateEvaluationVisitor(), this, CancellationToken);
        await OnEvaluateBinaryAsync(binaryEventArgs);

        if (binaryEventArgs.HasResult)
            return binaryEventArgs.Result;

        if (expression.Type == BinaryExpressionType.And)
        {
            return Convert.ToBoolean(await binaryEventArgs.LeftValueAsync(), cultureInfo) &&
                   Convert.ToBoolean(await binaryEventArgs.RightValueAsync(), cultureInfo);
        }

        if (expression.Type == BinaryExpressionType.Or)
        {
            return Convert.ToBoolean(await binaryEventArgs.LeftValueAsync(), cultureInfo) || Convert.ToBoolean(await binaryEventArgs.RightValueAsync(), cultureInfo);
        }

        var left = await binaryEventArgs.LeftValueAsync();
        var right = await binaryEventArgs.RightValueAsync();

        return EvaluationVisitorHelper.EvaluateBinary(expression.Type, left, right, options, cultureInfo);
    }

    public virtual async Task<object?> Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        var result = await expression.Expression.Accept(this);

        return Unary(expression, result, options, cultureInfo);
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
            options,
            cultureInfo,
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
        var value = await GetIdentifierValueAsync(identifier);

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
        return EvaluationVisitorHelper.Compare(a, b, comparisonType, options, cultureInfo);
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

        var parameterArgs = new ParameterEventArgs(identifier.Id, CancellationToken);

        context.EvaluateParameterHandler?.Invoke(identifierName, parameterArgs);

        if (!parameterArgs.HasResult)
            await (context.EvaluateAsyncParameterHandler?.Invoke(identifierName, parameterArgs) ?? Task.CompletedTask);

        if (parameterArgs.HasResult)
            return parameterArgs.Result;

        if (context.Parameters.TryGetValue(identifierName, out var parameter))
        {
            if (parameter is Expression expression)
            {
                ShareParametersWithChildExpression(expression);
                return expression;
            }

            return parameter;
        }

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
            return dynamicParameter(new ParameterData(identifier.Id, context, CancellationToken));

        if (context.AsyncParameters.TryGetValue(identifierName, out var asyncParameter))
            return await asyncParameter(new ParameterData(identifier.Id, context, CancellationToken));

        if (identifierName.Equals("null", StringComparison.InvariantCultureIgnoreCase) &&
            options.AllowNullParameter)
        {
            return null;
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    private void ShareParametersWithChildExpression(Expression expression)
    {
        foreach (var parameter in context.Parameters)
            expression.Parameters[parameter.Key] = parameter.Value;

        foreach (var parameter in context.DynamicParameters)
            expression.DynamicParameters[parameter.Key] = parameter.Value;

        foreach (var parameter in context.AsyncParameters)
            expression.AsyncParameters[parameter.Key] = parameter.Value;

        expression.SetEvaluationVisitorFactory(EvaluationVisitorFactory);

        expression.EvaluateFunction += context.EvaluateFunctionHandler;
        expression.EvaluateAsyncFunction += context.EvaluateAsyncFunctionHandler;
        expression.EvaluateParameter += context.EvaluateParameterHandler;
        expression.EvaluateAsyncParameter += context.EvaluateAsyncParameterHandler;
        expression.EvaluateBinary += context.EvaluateBinaryHandler;
        expression.EvaluateBinaryAsync += context.EvaluateBinaryAsyncHandler;
    }
}
