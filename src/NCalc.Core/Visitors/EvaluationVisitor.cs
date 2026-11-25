using System.Runtime.CompilerServices;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Exceptions;
using static NCalc.Helpers.EvaluationHelper;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to synchronously evaluating <see cref="LogicalExpression"/>.
/// </summary>
public class EvaluationVisitor(
    ExpressionContext context,
    ExpressionEvaluationOptions options,
    CultureInfo cultureInfo,
    IEvaluationVisitorFactory? evaluationVisitorFactory = null,
    CancellationToken cancellationToken = default) : ILogicalExpressionVisitor<object?>
{
    protected CancellationToken CancellationToken { get; } = cancellationToken;
    protected IEvaluationVisitorFactory? EvaluationVisitorFactory { get; } = evaluationVisitorFactory;

    protected AsyncEvaluationVisitor CreateAsyncEvaluationVisitor()
    {
        return EvaluationVisitorFactory?.CreateAsyncEvaluationVisitor(context, options, cultureInfo, CancellationToken)
               ?? new AsyncEvaluationVisitor(context, options, cultureInfo, cancellationToken: CancellationToken);
    }

    public virtual object? Visit(TernaryExpression expression)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        var left = Convert.ToBoolean(expression.LeftExpression.Accept(this), cultureInfo);

        return left
            ? expression.MiddleExpression.Accept(this)
            : expression.RightExpression.Accept(this);
    }

    public virtual object? Visit(BinaryExpression expression)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        var binaryEventArgs = new BinaryEventArgs(expression, this, CreateAsyncEvaluationVisitor(), CancellationToken);
        OnEvaluateBinary(binaryEventArgs);

        if (binaryEventArgs.HasResult)
            return binaryEventArgs.Result;

        if (expression.Type == BinaryExpressionType.And)
        {
            return BoxedValues.Box(Convert.ToBoolean(binaryEventArgs.LeftValue(), cultureInfo) &&
                   Convert.ToBoolean(binaryEventArgs.RightValue(), cultureInfo));
        }

        if (expression.Type == BinaryExpressionType.Or)
        {
            return BoxedValues.Box(Convert.ToBoolean(binaryEventArgs.LeftValue(), cultureInfo) ||
                   Convert.ToBoolean(binaryEventArgs.RightValue(), cultureInfo));
        }

        if (expression.Type == BinaryExpressionType.Coalesce)
        {
            var leftValue = binaryEventArgs.LeftValue();
            return leftValue ?? binaryEventArgs.RightValue();
        }

        var left = binaryEventArgs.LeftValue();
        var right = binaryEventArgs.RightValue();

        return EvaluationVisitorHelper.EvaluateBinary(expression.Type, left, right, options, cultureInfo);
    }

    public virtual object? Visit(UnaryExpression expression)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        var result = expression.Expression.Accept(this);

        return Unary(expression, result, options, cultureInfo);
    }

    public virtual object? Visit(Function function)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        var functionName = function.Identifier.Name;
        var functionData = new FunctionData(
            function.Identifier.Id,
            function.Parameters,
            context,
            options,
            cultureInfo,
            this,
            CreateAsyncEvaluationVisitor(),
            CancellationToken);
        var functionArgs = new FunctionEventArgs(functionData);

        OnEvaluateFunction(functionName, functionArgs);

        if (functionArgs.HasResult)
            return functionArgs.Result;

        if (context.TryGetFunction(functionName, out var expressionFunction))
            return expressionFunction(functionData);

        return BuiltInFunctionHelper.Evaluate(functionName, functionData);
    }

    public virtual object? Visit(Identifier identifier)
    {
        var value = GetIdentifierValue(identifier);

        return value is Expression expression ? expression.Evaluate(CancellationToken) : value;
    }

    public virtual object? Visit(ValueExpression expression) => expression.Value;

    public virtual object? Visit(LogicalExpressionList list)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        if (list.Count == 0)
            return Array.Empty<object?>();

        var expressions = list.AsSpan();
        var listCount = expressions.Length;

        var result = new object?[listCount];

        for (var index = 0; index < listCount; index++)
        {
            result[index] = expressions[index].Accept(this);
        }

        return result;
    }

    protected void OnEvaluateFunction(string name, FunctionEventArgs args)
    {
        context.EvaluateFunctionHandler?.Invoke(name, args);
    }

    protected void OnEvaluateBinary(BinaryEventArgs args)
    {
        context.EvaluateBinaryHandler?.Invoke(args);
    }

    private object? GetIdentifierValue(Identifier identifier)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new ParameterEventArgs(identifier.Id, CancellationToken);

        context.EvaluateParameterHandler?.Invoke(identifierName, parameterArgs);

        if (parameterArgs.HasResult)
            return parameterArgs.Result;

        if (context.TryGetParameter(identifierName, out var parameter))
        {
            if (parameter is Expression expression)
            {
                ShareParametersWithChildExpression(expression);
                return expression;
            }

            return parameter;
        }

        if (context.TryGetDynamicParameter(identifierName, out var dynamicParameter))
            return dynamicParameter(new ParameterData(identifier.Id, context, CancellationToken));

        if (identifierName.Equals("null", StringComparison.InvariantCultureIgnoreCase) &&
            options.AllowNullParameter)
        {
            return null;
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    private void ShareParametersWithChildExpression(Expression expression)
    {
        CopyEntries(context.Parameters, expression.Parameters);
        CopyEntries(context.DynamicParameters, expression.DynamicParameters);
        CopyEntries(context.AsyncParameters, expression.AsyncParameters);

        expression.SetEvaluationVisitorFactory(EvaluationVisitorFactory);

        // The child is shared again on every evaluation of the parent, so a handler is removed before it is added.
        // Adding it unconditionally grew the child's subscription list by one entry per evaluation, and the handler
        // ran once more each time.
        var childContext = expression.Context;
        childContext.EvaluateFunctionHandler = Share(childContext.EvaluateFunctionHandler, context.EvaluateFunctionHandler);
        childContext.EvaluateAsyncFunctionHandler = Share(childContext.EvaluateAsyncFunctionHandler, context.EvaluateAsyncFunctionHandler);
        childContext.EvaluateParameterHandler = Share(childContext.EvaluateParameterHandler, context.EvaluateParameterHandler);
        childContext.EvaluateAsyncParameterHandler = Share(childContext.EvaluateAsyncParameterHandler, context.EvaluateAsyncParameterHandler);
        childContext.EvaluateBinaryHandler = Share(childContext.EvaluateBinaryHandler, context.EvaluateBinaryHandler);
        childContext.EvaluateBinaryAsyncHandler = Share(childContext.EvaluateBinaryAsyncHandler, context.EvaluateBinaryAsyncHandler);
    }

    private static void CopyEntries<TValue>(IDictionary<string, TValue> source, IDictionary<string, TValue> target)
    {
        if (source.Count == 0)
            return;

        foreach (var entry in source)
            target[entry.Key] = entry.Value;
    }

    private static THandler? Share<THandler>(THandler? child, THandler? parent) where THandler : Delegate
    {
        if (parent is null)
            return child;

        return (THandler?)Delegate.Combine(Delegate.Remove(child, parent), parent);
    }
}
