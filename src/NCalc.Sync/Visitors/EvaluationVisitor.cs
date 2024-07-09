using ExtendedNumerics;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;
using System.Numerics;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
/// <param name="context">Contextual parameters of the <see cref="LogicalExpression"/>, like custom functions and parameters.</param>
public class EvaluationVisitor(ExpressionContext context) : ILogicalExpressionVisitor<object?>
{
    public event EvaluateFunctionHandler? EvaluateFunction;
    public event EvaluateParameterHandler? EvaluateParameter;

    public object? Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(expression.LeftExpression.Accept(this), context.CultureInfo);

        if (left)
        {
            return expression.MiddleExpression.Accept(this);
        }

        return expression.RightExpression.Accept(this);
    }

    public object? Visit(BinaryExpression expression)
    {
        var leftValue = new Lazy<object?>(() => Evaluate(expression.LeftExpression));
        var rightValue = new Lazy<object?>(() => Evaluate(expression.RightExpression));

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                return Convert.ToBoolean(leftValue.Value, context.CultureInfo) &&
                       Convert.ToBoolean(rightValue.Value, context.CultureInfo);

            case BinaryExpressionType.Or:
                return Convert.ToBoolean(leftValue.Value, context.CultureInfo) ||
                       Convert.ToBoolean(rightValue.Value, context.CultureInfo);

            case BinaryExpressionType.Div:
                return TypeHelper.IsReal(leftValue.Value) || TypeHelper.IsReal(rightValue.Value)
                    ? MathHelper.Divide(leftValue.Value, rightValue.Value, context)
                    : MathHelper.Divide(Convert.ToDouble(leftValue.Value, context.CultureInfo), rightValue.Value,
                        context);

            case BinaryExpressionType.Equal:
                return CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) == 0;

            case BinaryExpressionType.Greater:
                return CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) > 0;

            case BinaryExpressionType.GreaterOrEqual:
                return CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) >= 0;

            case BinaryExpressionType.Lesser:
                return CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) < 0;

            case BinaryExpressionType.LesserOrEqual:
                return CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) <= 0;

            case BinaryExpressionType.Minus:
                return MathHelper.Subtract(leftValue.Value, rightValue.Value, context);

            case BinaryExpressionType.Modulo:
                return MathHelper.Modulo(leftValue.Value, rightValue.Value, context);

            case BinaryExpressionType.NotEqual:
                return CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) != 0;

            case BinaryExpressionType.Plus:
            {
                var left = leftValue.Value;
                var right = rightValue.Value;

                if (context.Options.HasFlag(ExpressionOptions.StringConcat) || left is string && right is string)
                    return string.Concat(left, right);

                return MathHelper.Add(left, right, context);
            }

            case BinaryExpressionType.Times:
                return MathHelper.Multiply(leftValue.Value, rightValue.Value, context);


            case BinaryExpressionType.BitwiseAnd:
                return Convert.ToUInt64(leftValue.Value, context.CultureInfo) &
                       Convert.ToUInt64(rightValue.Value, context.CultureInfo);


            case BinaryExpressionType.BitwiseOr:
                return Convert.ToUInt64(leftValue.Value, context.CultureInfo) |
                       Convert.ToUInt64(rightValue.Value, context.CultureInfo);


            case BinaryExpressionType.BitwiseXOr:
                return Convert.ToUInt64(leftValue.Value, context.CultureInfo) ^
                       Convert.ToUInt64(rightValue.Value, context.CultureInfo);


            case BinaryExpressionType.LeftShift:
                return Convert.ToUInt64(leftValue.Value, context.CultureInfo) <<
                       Convert.ToInt32(rightValue.Value, context.CultureInfo);


            case BinaryExpressionType.RightShift:
                return Convert.ToUInt64(leftValue.Value, context.CultureInfo) >>
                       Convert.ToInt32(rightValue.Value, context.CultureInfo);

            case BinaryExpressionType.Exponentiation:
            {
                if (!context.Options.HasFlag(ExpressionOptions.DecimalAsDefault))
                    return Math.Pow(Convert.ToDouble(leftValue.Value, context.CultureInfo),
                        Convert.ToDouble(rightValue.Value, context.CultureInfo));
                BigDecimal @base = new BigDecimal(Convert.ToDecimal(leftValue.Value));
                BigInteger exponent = new BigInteger(Convert.ToDecimal(rightValue.Value));

                return (decimal)BigDecimal.Pow(@base, exponent);
            }
        }

        return null;
    }

    public object? Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        var result = expression.Expression.Accept(this);

        return expression.Type switch
        {
            UnaryExpressionType.Not => !Convert.ToBoolean(result, context.CultureInfo),
            UnaryExpressionType.Negate => MathHelper.Subtract(0, result, context),
            UnaryExpressionType.BitwiseNot => ~Convert.ToUInt64(result, context.CultureInfo),
            UnaryExpressionType.Positive => result,
            _ => throw new InvalidOperationException("Unknown UnaryExpressionType")
        };
    }

    public object? Visit(ValueExpression expression) => expression.Value;

    public object? Visit(Function function)
    {
        var argsCount = function.Expressions.Length;
        var args = new Expression[argsCount];

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (var i = 0; i < argsCount; i++)
        {
            args[i] = new Expression(function.Expressions[i], context);
            args[i].EvaluateParameter += EvaluateParameter;
            args[i].EvaluateFunction += EvaluateFunction;
        }

        var functionName = function.Identifier.Name;
        var functionArgs = new FunctionArgs(function.Identifier.Id, args);

        OnEvaluateFunction(functionName, functionArgs);

        if (functionArgs.HasResult)
        {
            return functionArgs.Result;
        }

        if (context.Functions.TryGetValue(functionName, out var expressionFunction))
        {
            return expressionFunction(new(function.Identifier.Id, args, context));
        }

        return BuiltInFunctionHelper.Evaluate(functionName, args, context);
    }

    public object? Visit(Identifier identifier)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new ParameterArgs(identifier.Id);

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

                expression.EvaluateFunction += EvaluateFunction;
                expression.EvaluateParameter += EvaluateParameter;

                return expression.Evaluate();
            }

            return parameter;
        }

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
        {
            return dynamicParameter(new(identifier.Id, context));
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    protected int CompareUsingMostPreciseType(object? a, object? b)
    {
        return TypeHelper.CompareUsingMostPreciseType(a, b, context);
    }

    protected void OnEvaluateFunction(string name, FunctionArgs args)
    {
        EvaluateFunction?.Invoke(name, args);
    }

    protected void OnEvaluateParameter(string name, ParameterArgs args)
    {
        EvaluateParameter?.Invoke(name, args);
    }

    private object? Evaluate(LogicalExpression expression)
    {
        return expression.Accept(this);
    }
}