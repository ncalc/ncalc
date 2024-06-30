using ExtendedNumerics;
using System.Numerics;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;
using BinaryExpression = NCalc.Domain.BinaryExpression;
using UnaryExpression = NCalc.Domain.UnaryExpression;

namespace NCalc.Visitors;

public class AsyncEvaluationVisitor(AsyncExpressionContext context) : ILogicalExpressionVisitor<Task<object?>>
{
    public event AsyncEvaluateFunctionHandler? EvaluateFunctionAsync;
    public event AsyncEvaluateParameterHandler? EvaluateParameterAsync;
    public AsyncExpressionContext Context { get; } = context;

    public async Task<object?> Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(await expression.LeftExpression.Accept(this), Context.CultureInfo);

        if (left)
        {
            return expression.MiddleExpression.Accept(this);
        }

        return expression.RightExpression.Accept(this);
    }

    public async Task<object?> Visit(BinaryExpression expression)
    {
        var leftValue = new Lazy<ValueTask<object?>>(() => EvaluateAsync(expression.LeftExpression));
        var rightValue = new Lazy<ValueTask<object?>>(() => EvaluateAsync(expression.RightExpression));

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                return Convert.ToBoolean(await leftValue.Value, Context.CultureInfo) &&
                       Convert.ToBoolean(await rightValue.Value, Context.CultureInfo);

            case BinaryExpressionType.Or:
                return Convert.ToBoolean(await leftValue.Value, Context.CultureInfo) ||
                       Convert.ToBoolean(await rightValue.Value, Context.CultureInfo);
            case BinaryExpressionType.Div:
                return TypeHelper.IsReal(await leftValue.Value) || TypeHelper.IsReal(await rightValue.Value)
                    ? MathHelper.Divide(await leftValue.Value, await rightValue.Value, Context)
                    : MathHelper.Divide(Convert.ToDouble(await leftValue.Value, Context.CultureInfo),
                        await rightValue.Value,
                        Context);

            case BinaryExpressionType.Equal:
                return CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) == 0;


            case BinaryExpressionType.Greater:
                return CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) > 0;


            case BinaryExpressionType.GreaterOrEqual:
                return CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) >= 0;


            case BinaryExpressionType.Lesser:
                return CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) < 0;


            case BinaryExpressionType.LesserOrEqual:
                return CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) <= 0;


            case BinaryExpressionType.Minus:
                return MathHelper.Subtract(await leftValue.Value, await rightValue.Value, Context);


            case BinaryExpressionType.Modulo:
                return MathHelper.Modulo(await leftValue.Value, await rightValue.Value, Context);


            case BinaryExpressionType.NotEqual:
                return CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) != 0;


            case BinaryExpressionType.Plus:
                if (await leftValue.Value is string)
                {
                    return string.Concat(await leftValue.Value, await rightValue.Value);
                }

                return MathHelper.Add(await leftValue.Value, await rightValue.Value, Context);


            case BinaryExpressionType.Times:
                return MathHelper.Multiply(await leftValue.Value, await rightValue.Value, Context);


            case BinaryExpressionType.BitwiseAnd:
                return Convert.ToUInt64(await leftValue.Value, Context.CultureInfo) &
                       Convert.ToUInt64(await rightValue.Value, Context.CultureInfo);


            case BinaryExpressionType.BitwiseOr:
                return Convert.ToUInt64(await leftValue.Value, Context.CultureInfo) |
                       Convert.ToUInt64(await rightValue.Value, Context.CultureInfo);


            case BinaryExpressionType.BitwiseXOr:
                return Convert.ToUInt64(await leftValue.Value, Context.CultureInfo) ^
                       Convert.ToUInt64(await rightValue.Value, Context.CultureInfo);


            case BinaryExpressionType.LeftShift:
                return Convert.ToUInt64(await leftValue.Value, Context.CultureInfo) <<
                       Convert.ToInt32(await rightValue.Value, Context.CultureInfo);


            case BinaryExpressionType.RightShift:
                return Convert.ToUInt64(await leftValue.Value, Context.CultureInfo) >>
                       Convert.ToInt32(await rightValue.Value, Context.CultureInfo);


            case BinaryExpressionType.Exponentiation:
            {
                if (Context.Options.HasFlag(ExpressionOptions.DecimalAsDefault))
                {
                    BigDecimal @base = new BigDecimal(Convert.ToDecimal(leftValue.Value));
                    BigInteger exponent = new BigInteger(Convert.ToDecimal(rightValue.Value));

                    return (decimal)BigDecimal.Pow(@base, exponent);
                }

                return Math.Pow(Convert.ToDouble(await leftValue.Value, Context.CultureInfo),
                    Convert.ToDouble(await rightValue.Value, Context.CultureInfo));
            }
        }

        return null;
    }

    public async Task<object?> Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        var result = await expression.Expression.Accept(this);

        return expression.Type switch
        {
            UnaryExpressionType.Not => !Convert.ToBoolean(result, Context.CultureInfo),
            UnaryExpressionType.Negate => MathHelper.Subtract(0, result, Context),
            UnaryExpressionType.BitwiseNot => ~Convert.ToUInt64(result, Context.CultureInfo),
            UnaryExpressionType.Positive => result,
            _ => throw new InvalidOperationException("Unknown UnaryExpressionType")
        };
    }

    public async Task<object?> Visit(Function function)
    {
        var argsCount = function.Expressions.Length;
        var args = new AsyncExpression[argsCount];

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (var i = 0; i < argsCount; i++)
        {
            args[i] = new AsyncExpression(function.Expressions[i], Context);
            args[i].EvaluateParameterAsync += EvaluateParameterAsync;
            args[i].EvaluateFunctionAsync += EvaluateFunctionAsync;
        }

        var functionName = function.Identifier.Name;
        var functionArgs = new AsyncFunctionArgs(function.Identifier.Id, args);

        await OnEvaluateFunctionAsync(functionName, functionArgs);

        if (functionArgs.HasResult)
        {
            return functionArgs.Result;
        }

        if (Context.Functions.TryGetValue(functionName, out var expressionFunction))
        {
            return await expressionFunction(new(function.Identifier.Id, args, Context));
        }

        return await AsyncBuiltInFunctionHelper.EvaluateAsync(functionName, args, Context);
    }

    public async Task<object?> Visit(Identifier identifier)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new AsyncParameterArgs(identifier.Id);

        await OnEvaluateParameterAsync(identifierName, parameterArgs);

        if (parameterArgs.HasResult)
        {
            return parameterArgs.Result;
        }

        if (Context.StaticParameters.TryGetValue(identifierName, out var parameter))
        {
            if (parameter is AsyncExpression expression)
            {
                //Share the parameters with child expression.
                foreach (var p in Context.StaticParameters)
                    expression.Parameters[p.Key] = p.Value;

                foreach (var p in Context.DynamicParameters)
                    expression.DynamicParameters[p.Key] = p.Value;

                expression.EvaluateFunctionAsync += EvaluateFunctionAsync;
                expression.EvaluateParameterAsync += EvaluateParameterAsync;

                return await expression.EvaluateAsync();
            }

            return parameter;
        }

        if (Context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
        {
            return await dynamicParameter(new(identifier.Id, Context));
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    public Task<object?> Visit(ValueExpression expression) => Task.FromResult(expression.Value);

    protected int CompareUsingMostPreciseType(object? a, object? b)
    {
        return TypeHelper.CompareUsingMostPreciseType(a, b, Context);
    }

    protected ValueTask OnEvaluateFunctionAsync(string name, AsyncFunctionArgs args)
    {
        return EvaluateFunctionAsync?.Invoke(name, args) ?? default;
    }

    protected ValueTask OnEvaluateParameterAsync(string name, AsyncParameterArgs args)
    {
        return EvaluateParameterAsync?.Invoke(name, args) ?? default;
    }

    private async ValueTask<object?> EvaluateAsync(LogicalExpression expression)
    {
        return await expression.Accept(this);
    }
}