using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;
using static NCalc.Helpers.TypeHelper;
using static NCalc.Helpers.EvaluationHelper<NCalc.AsyncExpressionContext>;
using BinaryExpression = NCalc.Domain.BinaryExpression;
using UnaryExpression = NCalc.Domain.UnaryExpression;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to asynchronous evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
/// <param name="context">Contextual parameters of the <see cref="LogicalExpression"/>, like custom functions and parameters.</param>
public class AsyncEvaluationVisitor(AsyncExpressionContext context) : ILogicalExpressionVisitor<ValueTask<object?>>
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
        var left = new Lazy<ValueTask<object?>>(() => EvaluateAsync(expression.LeftExpression, ct),
            LazyThreadSafetyMode.None);
        var right = new Lazy<ValueTask<object?>>(() => EvaluateAsync(expression.RightExpression, ct),
            LazyThreadSafetyMode.None);

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                return Convert.ToBoolean(await left.Value, context.CultureInfo) &&
                       Convert.ToBoolean(await right.Value, context.CultureInfo);

            case BinaryExpressionType.Or:
                return Convert.ToBoolean(await left.Value, context.CultureInfo) ||
                       Convert.ToBoolean(await right.Value, context.CultureInfo);

            case BinaryExpressionType.Div:
                return Div(await left.Value, await right.Value, context);

            case BinaryExpressionType.Equal:
                return Compare(await left.Value, await right.Value, ComparisonType.Equal);

            case BinaryExpressionType.Greater:
                return Compare(await left.Value, await right.Value, ComparisonType.Greater);

            case BinaryExpressionType.GreaterOrEqual:
                return Compare(await left.Value, await right.Value, ComparisonType.GreaterOrEqual);

            case BinaryExpressionType.Lesser:
                return Compare(await left.Value, await right.Value, ComparisonType.Lesser);

            case BinaryExpressionType.LesserOrEqual:
                return Compare(await left.Value, await right.Value, ComparisonType.LesserOrEqual);

            case BinaryExpressionType.NotEqual:
                return Compare(await left.Value, await right.Value, ComparisonType.NotEqual);

            case BinaryExpressionType.Minus:
                return Minus(await left.Value, await right.Value, context);

            case BinaryExpressionType.Modulo:
                return Modulo(await left.Value, await right.Value, context);

            case BinaryExpressionType.Plus:
                return Plus(await left.Value, await right.Value, context);

            case BinaryExpressionType.Times:
                return Times(await left.Value, await right.Value, context);

            case BinaryExpressionType.BitwiseAnd:
                return Convert.ToUInt64(await left.Value, context.CultureInfo) &
                       Convert.ToUInt64(await right.Value, context.CultureInfo);

            case BinaryExpressionType.BitwiseOr:
                return Convert.ToUInt64(await left.Value, context.CultureInfo) |
                       Convert.ToUInt64(await right.Value, context.CultureInfo);

            case BinaryExpressionType.BitwiseXOr:
                return Convert.ToUInt64(await left.Value, context.CultureInfo) ^
                       Convert.ToUInt64(await right.Value, context.CultureInfo);

            case BinaryExpressionType.LeftShift:
                return Convert.ToUInt64(await left.Value, context.CultureInfo) <<
                       Convert.ToInt32(await right.Value, context.CultureInfo);

            case BinaryExpressionType.RightShift:
                return Convert.ToUInt64(await left.Value, context.CultureInfo) >>
                       Convert.ToInt32(await right.Value, context.CultureInfo);

            case BinaryExpressionType.Exponentiation:
                return MathHelper.Pow(await left.Value, await right.Value, context);

            case BinaryExpressionType.In:
                return In(await right.Value, await left.Value, context);

            case BinaryExpressionType.NotIn:
                return !In(await right.Value, await left.Value, context);

            case BinaryExpressionType.Like:
                return Like(await left.Value, await right.Value, context);

            case BinaryExpressionType.NotLike:
                return !Like(await left.Value, await right.Value, context);
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
        var args = new AsyncExpression[argsCount];

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (var i = 0; i < argsCount; i++)
        {
            args[i] = new AsyncExpression(function.Parameters[i], context);
        }

        var functionName = function.Identifier.Name;
        var functionArgs = new AsyncFunctionArgs(function.Identifier.Id, args, ct);

        await OnEvaluateFunctionAsync(functionName, functionArgs);

        if (functionArgs.HasResult)
        {
            return functionArgs.Result;
        }

        if (context.Functions.TryGetValue(functionName, out var expressionFunction))
        {
            return await expressionFunction(new AsyncExpressionFunctionData(function.Identifier.Id, args, context, ct));
        }

        return await AsyncBuiltInFunctionHelper.EvaluateAsync(functionName, args, context, ct);
    }

    public virtual async ValueTask<object?> Visit(Identifier identifier, CancellationToken ct = default)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new AsyncParameterArgs(identifier.Id, ct);

        await OnEvaluateParameterAsync(identifierName, parameterArgs);

        if (parameterArgs.HasResult)
        {
            return parameterArgs.Result;
        }

        if (context.StaticParameters.TryGetValue(identifierName, out var parameter))
        {
            if (parameter is AsyncExpression expression)
            {
                //Share the parameters with child expression.
                foreach (var p in context.StaticParameters)
                    expression.Parameters[p.Key] = p.Value;

                foreach (var p in context.DynamicParameters)
                    expression.DynamicParameters[p.Key] = p.Value;

                expression.EvaluateFunctionAsync += context.AsyncEvaluateFunctionHandler;
                expression.EvaluateParameterAsync += context.AsyncEvaluateParameterHandler;

                return await expression.EvaluateAsync(ct);
            }

            return parameter;
        }

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
        {
            return await dynamicParameter(new AsyncExpressionParameterData(identifier.Id, context, ct));
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

    protected ValueTask OnEvaluateFunctionAsync(string name, AsyncFunctionArgs args)
    {
        return context.AsyncEvaluateFunctionHandler?.Invoke(name, args) ?? default;
    }

    protected ValueTask OnEvaluateParameterAsync(string name, AsyncParameterArgs args)
    {
        return context.AsyncEvaluateParameterHandler?.Invoke(name, args) ?? default;
    }

    protected ValueTask<object?> EvaluateAsync(LogicalExpression expression, CancellationToken ct = default)
    {
        return expression.Accept(this, ct);
    }
}
