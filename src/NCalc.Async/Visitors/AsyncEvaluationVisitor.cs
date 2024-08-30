using System.Numerics;
using System.Threading;
using ExtendedNumerics;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;
using BinaryExpression = NCalc.Domain.BinaryExpression;
using UnaryExpression = NCalc.Domain.UnaryExpression;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to asynchronous evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
/// <param name="context">Contextual parameters of the <see cref="LogicalExpression"/>, like custom functions and parameters.</param>
public class AsyncEvaluationVisitor(AsyncExpressionContext context) : ILogicalExpressionVisitor<ValueTask<object?>>
{
    public async ValueTask<object?> Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(await expression.LeftExpression.Accept(this), context.CultureInfo);

        if (left)
        {
            return await expression.MiddleExpression.Accept(this);
        }

        return await expression.RightExpression.Accept(this);
    }

    public async ValueTask<object?> Visit(BinaryExpression expression)
    {
        var left = new Lazy<ValueTask<object?>>(() => EvaluateAsync(expression.LeftExpression),
            LazyThreadSafetyMode.None);
        var right = new Lazy<ValueTask<object?>>(() => EvaluateAsync(expression.RightExpression),
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
                return TypeHelper.IsReal(await left.Value) || TypeHelper.IsReal(await right.Value)
                    ? MathHelper.Divide(await left.Value, await right.Value, context)
                    : MathHelper.Divide(Convert.ToDouble(await left.Value, context.CultureInfo),
                        await right.Value,
                        context);

            case BinaryExpressionType.Equal:
                return CompareUsingMostPreciseType(await left.Value, await right.Value) == 0;

            case BinaryExpressionType.Greater:
                return CompareUsingMostPreciseType(await left.Value, await right.Value) > 0;

            case BinaryExpressionType.GreaterOrEqual:
                return CompareUsingMostPreciseType(await left.Value, await right.Value) >= 0;

            case BinaryExpressionType.Lesser:
                return CompareUsingMostPreciseType(await left.Value, await right.Value) < 0;

            case BinaryExpressionType.LesserOrEqual:
                return CompareUsingMostPreciseType(await left.Value, await right.Value) <= 0;

            case BinaryExpressionType.Minus:
                return MathHelper.Subtract(await left.Value, await right.Value, context);

            case BinaryExpressionType.Modulo:
                return MathHelper.Modulo(await left.Value, await right.Value, context);

            case BinaryExpressionType.NotEqual:
                return CompareUsingMostPreciseType(await left.Value, await right.Value) != 0;

            case BinaryExpressionType.Plus:
            {
                var leftValue = await left.Value;
                var rightValue = await right.Value;

                return EvaluationHelper.Plus(leftValue, rightValue, context);
            }

            case BinaryExpressionType.Times:
                return MathHelper.Multiply(await left.Value, await right.Value, context);

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
            {
                return MathHelper.Pow(left.Value, right.Value, context);
            }

            case BinaryExpressionType.In:
            {
                var rightValue = await right.Value;
                var leftValue = await left.Value;
                return EvaluationHelper.In(rightValue, leftValue, context);
            }

            case BinaryExpressionType.NotIn:
            {
                var rightValue = await right.Value;
                var leftValue = await left.Value;
                return !EvaluationHelper.In(rightValue, leftValue, context);
            }
            case BinaryExpressionType.Like:
            {
                var rightValue = (await right.Value)?.ToString();
                var leftValue = (await left.Value)?.ToString();

                if (rightValue == null || leftValue == null)
                {
                    return false;
                }

                return EvaluationHelper.Like(leftValue, rightValue, context);
            }

            case BinaryExpressionType.NotLike:
            {
                var rightValue = (await right.Value)?.ToString();
                var leftValue = (await left.Value)?.ToString();

                if (rightValue == null || leftValue == null)
                {
                    return false;
                }

                return !EvaluationHelper.Like(leftValue, rightValue, context);
            }
        }

        return null;
    }

    public async ValueTask<object?> Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        var result = await expression.Expression.Accept(this);

        return EvaluationHelper.Unary(expression, result, context);
    }

    public async ValueTask<object?> Visit(Function function)
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
        var functionArgs = new AsyncFunctionArgs(function.Identifier.Id, args);

        await OnEvaluateFunctionAsync(functionName, functionArgs);

        if (functionArgs.HasResult)
        {
            return functionArgs.Result;
        }

        if (context.Functions.TryGetValue(functionName, out var expressionFunction))
        {
            return await expressionFunction(new(function.Identifier.Id, args, context));
        }

        return await AsyncBuiltInFunctionHelper.EvaluateAsync(functionName, args, context);
    }

    public async ValueTask<object?> Visit(Identifier identifier)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new AsyncParameterArgs(identifier.Id);

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

                return await expression.EvaluateAsync();
            }

            return parameter;
        }

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
        {
            return await dynamicParameter(new(identifier.Id, context));
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    public ValueTask<object?> Visit(ValueExpression expression) => new(expression.Value);

    public async ValueTask<object?> Visit(LogicalExpressionList list)
    {
        List<object?> result = [];

        foreach (var value in list)
        {
            result.Add(await EvaluateAsync(value));
        }

        return result;
    }

    protected int CompareUsingMostPreciseType(object? a, object? b)
    {
        return TypeHelper.CompareUsingMostPreciseType(a, b, context);
    }

    protected ValueTask OnEvaluateFunctionAsync(string name, AsyncFunctionArgs args)
    {
        return context.AsyncEvaluateFunctionHandler?.Invoke(name, args) ?? default;
    }

    protected ValueTask OnEvaluateParameterAsync(string name, AsyncParameterArgs args)
    {
        return context.AsyncEvaluateParameterHandler?.Invoke(name, args) ?? default;
    }

    private ValueTask<object?> EvaluateAsync(LogicalExpression expression)
    {
        return expression.Accept(this);
    }
}