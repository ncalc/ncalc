using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;
using static NCalc.Helpers.TypeHelper;
using static NCalc.Helpers.EvaluationHelper;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to asynchronous evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
/// <param name="context">Contextual parameters of the <see cref="LogicalExpression"/>, like custom functions and parameters.</param>
public class EvaluationVisitor(ExpressionContext context) : ILogicalExpressionVisitor<ValueTask<object?>>
{
    public virtual async ValueTask<object?> Visit(TernaryExpression expression, CancellationToken ct = default)
    {
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(await expression.LeftExpression.Accept(this, ct), context.CultureInfo);

        if (left)
        {
            return await expression.MiddleExpression.Accept(this, ct);
        }

        return await expression.RightExpression.Accept(this, ct);
    }

    public virtual async ValueTask<object?> Visit(BinaryExpression expression, CancellationToken ct = default)
    {
        if (expression.Type == BinaryExpressionType.And)
        {
            return Convert.ToBoolean(await EvaluateAsync(expression.LeftExpression, ct), context.CultureInfo) &&
                   Convert.ToBoolean(await EvaluateAsync(expression.RightExpression, ct), context.CultureInfo);
        }

        if (expression.Type == BinaryExpressionType.Or)
        {
            return Convert.ToBoolean(await EvaluateAsync(expression.LeftExpression, ct), context.CultureInfo) ||
                   Convert.ToBoolean(await EvaluateAsync(expression.RightExpression, ct), context.CultureInfo);
        }

        var left = await EvaluateAsync(expression.LeftExpression, ct);
        var right = await EvaluateAsync(expression.RightExpression, ct);

        switch (expression.Type)
        {
            case BinaryExpressionType.Div:
                return Div(left, right, context);

            case BinaryExpressionType.Equal:
                return Compare(left, right, ComparisonType.Equal);

            case BinaryExpressionType.Greater:
                return Compare(left, right, ComparisonType.Greater);

            case BinaryExpressionType.GreaterOrEqual:
                return Compare(left, right, ComparisonType.GreaterOrEqual);

            case BinaryExpressionType.Lesser:
                return Compare(left, right, ComparisonType.Lesser);

            case BinaryExpressionType.LesserOrEqual:
                return Compare(left, right, ComparisonType.LesserOrEqual);

            case BinaryExpressionType.NotEqual:
                return Compare(left, right, ComparisonType.NotEqual);

            case BinaryExpressionType.Minus:
                return Minus(left, right, context);

            case BinaryExpressionType.Modulo:
                return Modulo(left, right, context);

            case BinaryExpressionType.Plus:
                return Plus(left, right, context);

            case BinaryExpressionType.Times:
                return Times(left, right, context);

            case BinaryExpressionType.BitwiseAnd:
                return Convert.ToUInt64(left, context.CultureInfo) &
                       Convert.ToUInt64(right, context.CultureInfo);

            case BinaryExpressionType.BitwiseOr:
                return Convert.ToUInt64(left, context.CultureInfo) |
                       Convert.ToUInt64(right, context.CultureInfo);

            case BinaryExpressionType.BitwiseXOr:
                return Convert.ToUInt64(left, context.CultureInfo) ^
                       Convert.ToUInt64(right, context.CultureInfo);

            case BinaryExpressionType.LeftShift:
                return Convert.ToUInt64(left, context.CultureInfo) <<
                       Convert.ToInt32(right, context.CultureInfo);

            case BinaryExpressionType.RightShift:
                return Convert.ToUInt64(left, context.CultureInfo) >>
                       Convert.ToInt32(right, context.CultureInfo);

            case BinaryExpressionType.Exponentiation:
                return MathHelper.Pow(left, right, context);

            case BinaryExpressionType.In:
                return In(right, left, context);

            case BinaryExpressionType.NotIn:
                return !In(right, left, context);

            case BinaryExpressionType.Like:
                return Like(left, right, context);

            case BinaryExpressionType.NotLike:
                return !Like(left, right, context);
        }

        return null;
    }

    public virtual async ValueTask<object?> Visit(UnaryExpression expression, CancellationToken ct = default)
    {
        // Recursively evaluates the underlying expression
        var result = await expression.Expression.Accept(this, ct);

        return Unary(expression, result, context);
    }

    public virtual async ValueTask<object?> Visit(Function function, CancellationToken ct = default)
    {
        var argsCount = function.Parameters.Count;
        var args = new Expression[argsCount];

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (var i = 0; i < argsCount; i++)
        {
            args[i] = new Expression(function.Parameters[i], context);
        }

        var functionName = function.Identifier.Name;
        var functionArgs = new FunctionArgs(function.Identifier.Id, args, ct);

        // ReSharper disable once MethodHasAsyncOverload
        OnEvaluateFunction(functionName, functionArgs);

        if (functionArgs.HasResult)
            return functionArgs.Result;

        await OnEvaluateFunctionAsync(functionName, functionArgs);

        if (functionArgs.HasResult)
            return functionArgs.Result;

        if (context.Functions.TryGetValue(functionName, out var expressionFunction))
            return expressionFunction(new ExpressionFunctionData(function.Identifier.Id, args, context, ct));

        if (context.AsyncFunctions.TryGetValue(functionName, out var asyncExpressionFunction))
            return await asyncExpressionFunction(new ExpressionFunctionData(function.Identifier.Id, args, context, ct));

        return await BuiltInFunctionHelper.EvaluateAsync(functionName, args, context, ct);
    }

    public virtual async ValueTask<object?> Visit(Identifier identifier, CancellationToken ct = default)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new ParameterArgs(identifier.Id, ct);

        OnEvaluateParameter(identifierName, parameterArgs);

        if (parameterArgs.HasResult)
        {
            return parameterArgs.Result;
        }

        if (context.StaticParameters.TryGetValue(identifierName, out var parameter))
        {
            if (parameter is Expression expression)
            {
                //Share the parameters with child expression.
                foreach (var p in context.StaticParameters)
                    expression.Parameters[p.Key] = p.Value;

                foreach (var p in context.DynamicParameters)
                    expression.DynamicParameters[p.Key] = p.Value;

                expression.EvaluateFunction += context.EvaluateFunctionHandler;
                expression.EvaluateParameter += context.EvaluateParameterHandler;

                return await expression.EvaluateAsync(ct);
            }

            return parameter;
        }

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
        {
            return dynamicParameter(new ExpressionParameterData(identifier.Id, context, ct));
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    public virtual ValueTask<object?> Visit(ValueExpression expression, CancellationToken ct = default) => new(expression.Value);

    public virtual async ValueTask<object?> Visit(LogicalExpressionList list, CancellationToken ct = default)
    {
        List<object?> result = [];

        foreach (var value in list)
        {
            result.Add(await EvaluateAsync(value, ct));
        }

        return result;
    }

    protected bool Compare(object? a, object? b, ComparisonType comparisonType)
    {
        if (HasNullOrTypeConflict(a, b, context.Options))
            return comparisonType == ComparisonType.NotEqual;

        var result = CompareUsingMostPreciseType(a, b, context);

        return comparisonType switch
        {
            ComparisonType.Equal => result == 0,
            ComparisonType.Greater => result > 0,
            ComparisonType.GreaterOrEqual => result >= 0,
            ComparisonType.Lesser => result < 0,
            ComparisonType.LesserOrEqual => result <= 0,
            ComparisonType.NotEqual => result != 0,
            _ => throw new ArgumentOutOfRangeException(nameof(comparisonType), comparisonType, null)
        };
    }

    protected Task OnEvaluateFunctionAsync(string name, FunctionArgs args)
    {
        return context.EvaluateAsyncFunctionHandler?.Invoke(name, args) ?? Task.CompletedTask;
    }

    protected void OnEvaluateFunction(string name, FunctionArgs args)
    {
        context.EvaluateFunctionHandler?.Invoke(name, args);
    }

    protected void OnEvaluateParameter(string name, ParameterArgs args)
    {
        context.EvaluateParameterHandler?.Invoke(name, args);
    }

    protected ValueTask<object?> EvaluateAsync(LogicalExpression expression, CancellationToken ct = default)
    {
        return expression.Accept(this, ct);
    }
}
