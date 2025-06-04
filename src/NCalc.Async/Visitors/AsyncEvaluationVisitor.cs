using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;

using static NCalc.Helpers.TypeHelper;

using BinaryExpression = NCalc.Domain.BinaryExpression;
using UnaryExpression = NCalc.Domain.UnaryExpression;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to asynchronous evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
/// <param name="context">Contextual parameters of the <see cref="LogicalExpression"/>, like custom functions and parameters.</param>
public class AsyncEvaluationVisitor(AsyncExpressionContext context) : ILogicalExpressionVisitor<ValueTask<object?>>
{
    public virtual async ValueTask<object?> Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(await expression.LeftExpression.Accept(this), context.CultureInfo);

        if (left)
        {
            return await expression.MiddleExpression.Accept(this);
        }

        return await expression.RightExpression.Accept(this);
    }

    public virtual async ValueTask<object?> Visit(BinaryExpression expression)
    {
        var left = new Lazy<ValueTask<object?>>(() => EvaluateAsync(expression.LeftExpression),
            LazyThreadSafetyMode.None);
        var right = new Lazy<ValueTask<object?>>(() => EvaluateAsync(expression.RightExpression),
            LazyThreadSafetyMode.None);

        if (expression.LeftExpression is PercentExpression && expression.RightExpression is PercentExpression)
        {
            switch (expression.Type)
            {
                case BinaryExpressionType.Minus:
                    return new PercentExpression(new ValueExpression(MathHelper.Subtract(await left.Value, await right.Value, context) ?? 0));
                case BinaryExpressionType.Plus:
                    return new PercentExpression(new ValueExpression(MathHelper.Add(await left.Value, await right.Value, context) ?? 0));
            }
        }
        else
        if (expression.LeftExpression is PercentExpression)
        {
            switch (expression.Type)
            {
                case BinaryExpressionType.Times:
                    return new PercentExpression(new ValueExpression(MathHelper.Multiply(await left.Value, await right.Value, context) ?? 0));
                case BinaryExpressionType.Div:
                    return new PercentExpression(new ValueExpression(MathHelper.Divide(await left.Value, await right.Value, context) ?? 0));
            }
        }
        else
        if (expression.RightExpression is PercentExpression)
        {
            switch (expression.Type)
            {
                case BinaryExpressionType.Minus:
                    return MathHelper.SubtractPercent(await left.Value, await right.Value, context);
                case BinaryExpressionType.Plus:
                    return MathHelper.AddPercent(await left.Value, await right.Value, context);
                case BinaryExpressionType.Times:
                    return MathHelper.MultiplyPercent(await left.Value, await right.Value, context);
                case BinaryExpressionType.Div:
                    return MathHelper.DividePercent(await left.Value, await right.Value, context);
            }
        }
        else
        {
        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                return Convert.ToBoolean(await left.Value, context.CultureInfo) &&
                       Convert.ToBoolean(await right.Value, context.CultureInfo);

            case BinaryExpressionType.Or:
                return Convert.ToBoolean(await left.Value, context.CultureInfo) ||
                       Convert.ToBoolean(await right.Value, context.CultureInfo);

            case BinaryExpressionType.Div:
                return IsReal(await left.Value) || IsReal(await right.Value)
                    ? MathHelper.Divide(await left.Value, await right.Value, context)
                    : MathHelper.Divide(Convert.ToDouble(await left.Value, context.CultureInfo),
                        await right.Value,
                        context);

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
                return MathHelper.Subtract(await left.Value, await right.Value, context);

            case BinaryExpressionType.Modulo:
                return MathHelper.Modulo(await left.Value, await right.Value, context);

            case BinaryExpressionType.Plus:
                return EvaluationHelper.Plus(await left.Value, await right.Value, context);

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
                return MathHelper.Pow(await left.Value, await right.Value, context);

            case BinaryExpressionType.In:
                return EvaluationHelper.In(await right.Value, await left.Value, context);

            case BinaryExpressionType.NotIn:
                return !EvaluationHelper.In(await right.Value, await left.Value, context);

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
        }
        return null;
    }

    public virtual async ValueTask<object?> Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        var result = await expression.Expression.Accept(this);

        return EvaluationHelper.Unary(expression, result, context);
    }

    public virtual async ValueTask<object?> Visit(PercentExpression expression)
    {
        return await expression.Expression.Accept(this);
    }

    public virtual async ValueTask<object?> Visit(Function function)
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
            return await expressionFunction(new AsyncExpressionFunctionData(function.Identifier.Id, args, context));
        }

        return await AsyncBuiltInFunctionHelper.EvaluateAsync(functionName, args, context);
    }

    public virtual async ValueTask<object?> Visit(Identifier identifier)
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
            return await dynamicParameter(new AsyncExpressionParameterData(identifier.Id, context));
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    public virtual ValueTask<object?> Visit(ValueExpression expression) => new(expression.Value);

    public virtual async ValueTask<object?> Visit(LogicalExpressionList list)
    {
        List<object?> result = [];

        foreach (var value in list)
        {
            result.Add(await EvaluateAsync(value));
        }

        return result;
    }

    protected bool Compare(object? a, object? b, ComparisonType comparisonType)
    {
        if (context.Options.HasFlag(ExpressionOptions.StrictTypeMatching) && a?.GetType() != b?.GetType())
            return false;

        if ((a == null || b == null) && !(a == null && b == null))
            return false;

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

    protected ValueTask<object?> EvaluateAsync(LogicalExpression expression)
    {
        return expression.Accept(this);
    }
}