using System.Linq.Expressions;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Extensions;
using NCalc.Handlers;
using NCalc.Helpers;
using BinaryExpression = NCalc.Domain.BinaryExpression;
using UnaryExpression = NCalc.Domain.UnaryExpression;

namespace NCalc.Visitors;

public class AsyncEvaluationVisitor : IAsyncLogicalExpressionVisitor
{
    public event AsyncEvaluateFunctionHandler? EvaluateFunctionAsync;
    public event AsyncEvaluateParameterHandler? EvaluateParameterAsync;
    public AsyncExpressionContext Context { get; }
    
    public object? Result { get; protected set; }
    
    public AsyncEvaluationVisitor(AsyncExpressionContext context)
    {
        Context = context;
        EvaluateFunctionAsync += context.AsyncEvaluateFunctionHandler;
        EvaluateParameterAsync += context.AsyncEvaluateParameterHandler;
    }

    
    public async Task VisitAsync(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        await expression.LeftExpression.AcceptAsync(this);
        var left = Convert.ToBoolean(Result, Context.CultureInfo);

        if (left)
        {
            await expression.MiddleExpression.AcceptAsync(this);
        }
        else
        {
            await expression.RightExpression.AcceptAsync(this);
        }
    }

    public async Task VisitAsync(BinaryExpression expression)
    {
        var leftValue = new Lazy<Task<object?>>(() => EvaluateAsync(expression.LeftExpression));
        var rightValue = new Lazy<Task<object?>>(() => EvaluateAsync(expression.RightExpression));

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                Result = Convert.ToBoolean(await leftValue.Value, Context.CultureInfo) &&
                         Convert.ToBoolean(await rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.Or:
                Result = Convert.ToBoolean(await leftValue.Value, Context.CultureInfo) ||
                         Convert.ToBoolean(await rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.Div:
                Result = TypeHelper.IsReal(await leftValue.Value) || TypeHelper.IsReal(await rightValue.Value)
                    ? MathHelper.Divide(await leftValue.Value, await rightValue.Value, Context)
                    : MathHelper.Divide(Convert.ToDouble(await leftValue.Value, Context.CultureInfo), await rightValue.Value,
                        Context);
                break;

            case BinaryExpressionType.Equal:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) == 0;
                break;

            case BinaryExpressionType.Greater:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) > 0;
                break;

            case BinaryExpressionType.GreaterOrEqual:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) >= 0;
                break;

            case BinaryExpressionType.Lesser:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) < 0;
                break;

            case BinaryExpressionType.LesserOrEqual:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) <= 0;
                break;

            case BinaryExpressionType.Minus:
                Result = MathHelper.Subtract(await leftValue.Value, await rightValue.Value, Context);
                break;

            case BinaryExpressionType.Modulo:
                Result = MathHelper.Modulo(await leftValue.Value, await rightValue.Value, Context);
                break;

            case BinaryExpressionType.NotEqual:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) != 0;
                break;

            case BinaryExpressionType.Plus:
                if (await leftValue.Value is string)
                {
                    Result = string.Concat(await leftValue.Value, await rightValue.Value);
                }
                else
                {
                    Result = MathHelper.Add(await leftValue.Value, await rightValue.Value, Context);
                }

                break;

            case BinaryExpressionType.Times:
                Result = MathHelper.Multiply(await leftValue.Value, await rightValue.Value, Context);
                break;

            case BinaryExpressionType.BitwiseAnd:
                Result = Convert.ToUInt64(await leftValue.Value, Context.CultureInfo) &
                         Convert.ToUInt64(await rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.BitwiseOr:
                Result = Convert.ToUInt64(await leftValue.Value, Context.CultureInfo) |
                         Convert.ToUInt64(await rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.BitwiseXOr:
                Result = Convert.ToUInt64(await leftValue.Value, Context.CultureInfo) ^
                         Convert.ToUInt64(await rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.LeftShift:
                Result = Convert.ToUInt64(await leftValue.Value, Context.CultureInfo) <<
                         Convert.ToInt32(await rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.RightShift:
                Result = Convert.ToUInt64(await leftValue.Value, Context.CultureInfo) >>
                         Convert.ToInt32(await rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.Exponentiation:
                Result = Math.Pow(Convert.ToDouble(await leftValue.Value, Context.CultureInfo),
                    Convert.ToDouble(await rightValue.Value, Context.CultureInfo));
                break;
        }
    }

    public async Task VisitAsync(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        await expression.Expression.AcceptAsync(this);

        Result = expression.Type switch
        {
            UnaryExpressionType.Not => !Convert.ToBoolean(Result, Context.CultureInfo),
            UnaryExpressionType.Negate => MathHelper.Subtract(0, Result, Context),
            UnaryExpressionType.BitwiseNot => ~Convert.ToUInt64(Result, Context.CultureInfo),
            UnaryExpressionType.Positive => Result,
            _ => throw new InvalidOperationException("Unknown UnaryExpressionType")
        };
    }

    public async Task VisitAsync(Function function)
    {
        var argsCount = function.Expressions.Length;
        var args = new AsyncExpression[argsCount];

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (var i = 0; i < argsCount; i++)
        {
            args[i] = new AsyncExpression(function.Expressions[i], Context);
        }

        var functionName = function.Identifier.Name;
        var functionArgs = new AsyncFunctionArgs(args);
        
        await OnEvaluateFunctionAsync(functionName, functionArgs);

        if (functionArgs.HasResult)
        {
            Result = functionArgs.Result;
        }
        else if (Context.Functions.TryGetValue(functionName, out var expressionFunction))
        {
            Result = await expressionFunction(args, Context);
        }
        else
        {
            Result = await AsyncBuiltInFunctionHelper.EvaluateAsync(functionName, args, Context);
        }
    }
    
    public async Task VisitAsync(Identifier identifier)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new AsyncParameterArgs();
        
        await OnEvaluateParameterAsync(identifierName, parameterArgs);
        
        if (parameterArgs.HasResult)
        {
            Result = parameterArgs.Result;
        }
        else if (Context.StaticParameters.TryGetValue(identifierName, out var parameter))
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
                
                Result = await expression.EvaluateAsync();
            }
            else
            {
                Result = parameter;
            }
        }
        else if (Context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
        {
            Result = await dynamicParameter(Context);
        }
        else
        {
            throw new NCalcParameterNotDefinedException(identifierName);
        }
    }

    public Task VisitAsync(ValueExpression expression)
    {
        Result = expression.Value;
        return Task.CompletedTask;
    }
    
    protected int CompareUsingMostPreciseType(object? a, object? b)
    {
        return TypeHelper.CompareUsingMostPreciseType(a,
            b, new(Context.CultureInfo,
                Context.Options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer),
                Context.Options.HasFlag(ExpressionOptions.OrdinalStringComparer)));
    }

    protected Task OnEvaluateFunctionAsync(string name, AsyncFunctionArgs args)
    {
        if (EvaluateFunctionAsync is not null)
        {
            return EvaluateFunctionAsync(name, args);
        }

        return Task.CompletedTask;
    }

    protected Task OnEvaluateParameterAsync(string name, AsyncParameterArgs args)
    {
        if (EvaluateParameterAsync is not null)
        {
            return EvaluateParameterAsync(name, args);
        }

        return Task.CompletedTask;
    }
    
    private async Task<object?> EvaluateAsync(LogicalExpression expression)
    {
        await expression.AcceptAsync(this);
        return Result;
    }
}