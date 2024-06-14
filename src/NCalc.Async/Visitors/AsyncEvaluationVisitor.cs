using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Extensions;
using NCalc.Helpers;
using BinaryExpression = NCalc.Domain.BinaryExpression;
using UnaryExpression = NCalc.Domain.UnaryExpression;

namespace NCalc.Visitors;

public class AsyncEvaluationVisitor(AsyncExpressionContext context) : IAsyncLogicalExpressionVisitor
{
    public AsyncExpressionContext Context { get; } = context;

    public object? Result { get; protected set; }
    
    protected MathHelperOptions MathHelperOptions => new(Context.CultureInfo,
        Context.Options.HasFlag(ExpressionOptions.AllowBooleanCalculation),
        Context.Options.HasFlag(ExpressionOptions.DecimalAsDefault));
    
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
                    ? MathHelper.Divide(await leftValue.Value, await rightValue.Value, MathHelperOptions)
                    : MathHelper.Divide(Convert.ToDouble(await leftValue.Value, Context.CultureInfo), await rightValue.Value,
                        MathHelperOptions);
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
                Result = MathHelper.Subtract(await leftValue.Value, await rightValue.Value, MathHelperOptions);
                break;

            case BinaryExpressionType.Modulo:
                Result = MathHelper.Modulo(await leftValue.Value, await rightValue.Value, MathHelperOptions);
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
                    Result = MathHelper.Add(await leftValue.Value, await rightValue.Value, MathHelperOptions);
                }

                break;

            case BinaryExpressionType.Times:
                Result = MathHelper.Multiply(await leftValue.Value, await rightValue.Value, MathHelperOptions);
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
            UnaryExpressionType.Negate => MathHelper.Subtract(0, Result, MathHelperOptions),
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

        if (!Context.Functions.TryGetValue(function.Identifier.Name, out var expressionFunction))
            throw new NCalcFunctionNotFoundException(function.Identifier.Name);

        Result = await expressionFunction(args, Context);
    }
    
    public async Task VisitAsync(Identifier identifier)
    {
        if (Context.StaticParameters.TryGetValue(identifier.Name, out var parameter))
        {
            if (parameter is AsyncExpression expression)
            {
                //Share the parameters with child expression.
                foreach (var p in Context.StaticParameters)
                    expression.Parameters[p.Key] = p.Value;
                
                foreach (var p in Context.DynamicParameters)
                    expression.DynamicParameters[p.Key] = p.Value;
                
                Result = await expression.EvaluateAsync();
            }
            else
            {
                Result = parameter;
            }
        }
        else if (Context.DynamicParameters.TryGetValue(identifier.Name, out var dynamicParameter))
        {
            Result = await dynamicParameter(Context);
        }
        else
        {
            throw new NCalcParameterNotDefinedException(identifier.Name);
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

    private async Task<object?> EvaluateAsync(LogicalExpression expression)
    {
        await expression.AcceptAsync(this);
        return Result;
    }
}