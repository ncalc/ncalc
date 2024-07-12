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
        var left = new Lazy<object?>(() => Evaluate(expression.LeftExpression), LazyThreadSafetyMode.None);
        var right = new Lazy<object?>(() => Evaluate(expression.RightExpression), LazyThreadSafetyMode.None);

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                return Convert.ToBoolean(left.Value, context.CultureInfo) &&
                         Convert.ToBoolean(right.Value, context.CultureInfo);

            case BinaryExpressionType.Or:
                return Convert.ToBoolean(left.Value, context.CultureInfo) ||
                         Convert.ToBoolean(right.Value, context.CultureInfo);

            case BinaryExpressionType.Div:
                return TypeHelper.IsReal(left.Value) || TypeHelper.IsReal(right.Value)
                    ? MathHelper.Divide(left.Value, right.Value, context)
                    : MathHelper.Divide(Convert.ToDouble(left.Value, context.CultureInfo), right.Value,
                        context);

            case BinaryExpressionType.Equal:
                return CompareUsingMostPreciseType(left.Value, right.Value) == 0;

            case BinaryExpressionType.Greater:
                return CompareUsingMostPreciseType(left.Value, right.Value) > 0;

            case BinaryExpressionType.GreaterOrEqual:
                return CompareUsingMostPreciseType(left.Value, right.Value) >= 0;

            case BinaryExpressionType.Lesser:
                return CompareUsingMostPreciseType(left.Value, right.Value) < 0;

            case BinaryExpressionType.LesserOrEqual:
                return CompareUsingMostPreciseType(left.Value, right.Value) <= 0;

            case BinaryExpressionType.Minus:
                return MathHelper.Subtract(left.Value, right.Value, context);

            case BinaryExpressionType.Modulo:
                return MathHelper.Modulo(left.Value, right.Value, context);

            case BinaryExpressionType.NotEqual:
                return CompareUsingMostPreciseType(left.Value, right.Value) != 0;

            case BinaryExpressionType.Plus:
                {
                    var leftValue = left.Value;
                    var rightValue = right.Value;

                    if (context.Options.HasFlag(ExpressionOptions.StringConcat))
                        return string.Concat(leftValue, rightValue);

                    try
                    {
                        return MathHelper.Add(leftValue, rightValue, context);
                    }
                    catch (FormatException) when (leftValue is string && rightValue is string)
                    {
                        return string.Concat(leftValue, rightValue);
                    }
                }

            case BinaryExpressionType.Times:
                return MathHelper.Multiply(left.Value, right.Value, context);


            case BinaryExpressionType.BitwiseAnd:
                return Convert.ToUInt64(left.Value, context.CultureInfo) &
                         Convert.ToUInt64(right.Value, context.CultureInfo);


            case BinaryExpressionType.BitwiseOr:
                return Convert.ToUInt64(left.Value, context.CultureInfo) |
                         Convert.ToUInt64(right.Value, context.CultureInfo);


            case BinaryExpressionType.BitwiseXOr:
                return Convert.ToUInt64(left.Value, context.CultureInfo) ^
                         Convert.ToUInt64(right.Value, context.CultureInfo);


            case BinaryExpressionType.LeftShift:
                return Convert.ToUInt64(left.Value, context.CultureInfo) <<
                         Convert.ToInt32(right.Value, context.CultureInfo);


            case BinaryExpressionType.RightShift:
                return Convert.ToUInt64(left.Value, context.CultureInfo) >>
                         Convert.ToInt32(right.Value, context.CultureInfo);

            case BinaryExpressionType.Exponentiation:
                {
                    if (!context.Options.HasFlag(ExpressionOptions.DecimalAsDefault))
                        return Math.Pow(Convert.ToDouble(left.Value, context.CultureInfo),
                            Convert.ToDouble(right.Value, context.CultureInfo));
                    BigDecimal @base = new BigDecimal(Convert.ToDecimal(left.Value));
                    BigInteger exponent = new BigInteger(Convert.ToDecimal(right.Value));

                    return (decimal)BigDecimal.Pow(@base, exponent);
                }

            case BinaryExpressionType.In:
            {
                return right.Value switch
                {
                    IEnumerable<object> rightValueEnumerable => rightValueEnumerable.Contains(left.Value),
                    string rightValueString => rightValueString.Contains(left.Value?.ToString() ?? string.Empty),
                    _ => throw new NCalcEvaluationException(
                        "'in' operator right value must implement IEnumerable or be a string.")
                };
            }
            case BinaryExpressionType.NotIn:
            {
                return right.Value switch
                {
                    IEnumerable<object> rightValueEnumerable => !rightValueEnumerable.Contains(left.Value),
                    string rightValueString => !rightValueString.Contains(left.Value?.ToString() ?? string.Empty),
                    _ => throw new NCalcEvaluationException(
                        "'not in' operator right value must implement IEnumerable or be a string.")
                };
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

                expression.EvaluateFunction += context.EvaluateFunctionHandler;
                expression.EvaluateParameter += context.EvaluateParameterHandler;
                
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

    public object Visit(LogicalExpressionList list)
    {
        List<object?> result = [];

        foreach (var value in list)
        {
            result.Add(Evaluate(value));
        }

        return result.ToArray();
    }

    protected int CompareUsingMostPreciseType(object? a, object? b)
    {
        return TypeHelper.CompareUsingMostPreciseType(a, b, context);
    }
    protected void OnEvaluateFunction(string name, FunctionArgs args)
    {
        context.EvaluateFunctionHandler?.Invoke(name, args);
    }
    protected void OnEvaluateParameter(string name, ParameterArgs args)
    {
        context.EvaluateParameterHandler?.Invoke(name, args);
    }

    private object? Evaluate(LogicalExpression expression)
    {
        return expression.Accept(this);
    }
}