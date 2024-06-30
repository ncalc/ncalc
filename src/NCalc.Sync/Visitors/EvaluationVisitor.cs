using ExtendedNumerics;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;
using System.Numerics;

namespace NCalc.Visitors;

public interface IEvaluationVisitor : ILogicalExpressionVisitor<object?>
{
    public event EvaluateFunctionHandler? EvaluateFunction;
    public event EvaluateParameterHandler? EvaluateParameter;
}

public class EvaluationVisitor(ExpressionContext context) : IEvaluationVisitor
{
    public event EvaluateFunctionHandler? EvaluateFunction;
    public event EvaluateParameterHandler? EvaluateParameter;
    
    public ExpressionContext Context { get; } = context;

    public object? Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(expression.LeftExpression.Accept(this), Context.CultureInfo);

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
                return Convert.ToBoolean(leftValue.Value, Context.CultureInfo) &&
                         Convert.ToBoolean(rightValue.Value, Context.CultureInfo);
                

            case BinaryExpressionType.Or:
                return Convert.ToBoolean(leftValue.Value, Context.CultureInfo) ||
                         Convert.ToBoolean(rightValue.Value, Context.CultureInfo);
                

            case BinaryExpressionType.Div:
                return TypeHelper.IsReal(leftValue.Value) || TypeHelper.IsReal(rightValue.Value)
                    ? MathHelper.Divide(leftValue.Value, rightValue.Value, Context)
                    : MathHelper.Divide(Convert.ToDouble(leftValue.Value, Context.CultureInfo), rightValue.Value,
                        Context);
                

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
                return MathHelper.Subtract(leftValue.Value, rightValue.Value, Context);
                

            case BinaryExpressionType.Modulo:
                return MathHelper.Modulo(leftValue.Value, rightValue.Value, Context);
                

            case BinaryExpressionType.NotEqual:
                return CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) != 0;
                

            case BinaryExpressionType.Plus:
                return leftValue.Value is string ? string.Concat(leftValue.Value, rightValue.Value) : MathHelper.Add(leftValue.Value, rightValue.Value, Context);
            case BinaryExpressionType.Times:
                return MathHelper.Multiply(leftValue.Value, rightValue.Value, Context);
                

            case BinaryExpressionType.BitwiseAnd:
                return Convert.ToUInt64(leftValue.Value, Context.CultureInfo) &
						 Convert.ToUInt64(rightValue.Value, Context.CultureInfo);
                

            case BinaryExpressionType.BitwiseOr:
                return Convert.ToUInt64(leftValue.Value, Context.CultureInfo) |
						 Convert.ToUInt64(rightValue.Value, Context.CultureInfo);
                

            case BinaryExpressionType.BitwiseXOr:
                return Convert.ToUInt64(leftValue.Value, Context.CultureInfo) ^
						 Convert.ToUInt64(rightValue.Value, Context.CultureInfo);
                

            case BinaryExpressionType.LeftShift:
                return Convert.ToUInt64(leftValue.Value, Context.CultureInfo) <<
						 Convert.ToInt32(rightValue.Value, Context.CultureInfo);
                

            case BinaryExpressionType.RightShift:
                return Convert.ToUInt64(leftValue.Value, Context.CultureInfo) >>
						 Convert.ToInt32(rightValue.Value, Context.CultureInfo);
            
            case BinaryExpressionType.Exponentiation:
            {
                if (!Context.Options.HasFlag(ExpressionOptions.DecimalAsDefault))
                    return Math.Pow(Convert.ToDouble(leftValue.Value, Context.CultureInfo),
                        Convert.ToDouble(rightValue.Value, Context.CultureInfo));
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
            UnaryExpressionType.Not => !Convert.ToBoolean(result, Context.CultureInfo),
            UnaryExpressionType.Negate => MathHelper.Subtract(0, result, Context),
            UnaryExpressionType.BitwiseNot => ~Convert.ToUInt64(result, Context.CultureInfo),
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
            args[i] = new Expression(function.Expressions[i], Context);
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

        if (Context.Functions.TryGetValue(functionName, out var expressionFunction))
        {
            return expressionFunction(new(function.Identifier.Id, args, Context));
        }

        return BuiltInFunctionHelper.Evaluate(functionName, args, Context);
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

        if (Context.StaticParameters.TryGetValue(identifierName, out var parameter))
        {
            if (parameter is Expression expression)
            {
                //Share the parameters with child expression.
                foreach (var p in Context.StaticParameters)
                    expression.Parameters[p.Key] = p.Value;
                
                foreach (var p in Context.DynamicParameters)
                    expression.DynamicParameters[p.Key] = p.Value;

                expression.EvaluateFunction += EvaluateFunction;
                expression.EvaluateParameter += EvaluateParameter;
                
                return expression.Evaluate();
            }

            return parameter;
        }

        if (Context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
        {
            return dynamicParameter(new(identifier.Id, Context));
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }
    
    protected int CompareUsingMostPreciseType(object? a, object? b)
    {
        return TypeHelper.CompareUsingMostPreciseType(a, b, Context);
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