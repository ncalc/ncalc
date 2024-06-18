using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;

namespace NCalc.Visitors;

public class EvaluationVisitor : ILogicalExpressionVisitor
{
    public event EvaluateFunctionHandler? EvaluateFunction;
    public event EvaluateParameterHandler? EvaluateParameter;
    
    public ExpressionContext Context { get; }
    
    public EvaluationVisitor(ExpressionContext context)
    {
        Context = context;
        EvaluateFunction += context.EvaluateFunctionHandler;
        EvaluateParameter += context.EvaluateParameterHandler;
    }

    public object? Result { get; protected set; }

    public void Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        expression.LeftExpression.Accept(this);
        var left = Convert.ToBoolean(Result, Context.CultureInfo);

        if (left)
        {
            expression.MiddleExpression.Accept(this);
        }
        else
        {
            expression.RightExpression.Accept(this);
        }
    }

    public void Visit(BinaryExpression expression)
    {
        var leftValue = new Lazy<object?>(() => Evaluate(expression.LeftExpression));
        var rightValue = new Lazy<object?>(() => Evaluate(expression.RightExpression));
        
        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                Result = Convert.ToBoolean(leftValue.Value, Context.CultureInfo) &&
                         Convert.ToBoolean(rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.Or:
                Result = Convert.ToBoolean(leftValue.Value, Context.CultureInfo) ||
                         Convert.ToBoolean(rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.Div:
                Result = TypeHelper.IsReal(leftValue.Value) || TypeHelper.IsReal(rightValue.Value)
                    ? MathHelper.Divide(leftValue.Value, rightValue.Value, Context)
                    : MathHelper.Divide(Convert.ToDouble(leftValue.Value, Context.CultureInfo), rightValue.Value,
                        Context);
                break;

            case BinaryExpressionType.Equal:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) == 0;
                break;

            case BinaryExpressionType.Greater:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) > 0;
                break;

            case BinaryExpressionType.GreaterOrEqual:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) >= 0;
                break;

            case BinaryExpressionType.Lesser:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) < 0;
                break;

            case BinaryExpressionType.LesserOrEqual:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) <= 0;
                break;

            case BinaryExpressionType.Minus:
                Result = MathHelper.Subtract(leftValue.Value, rightValue.Value, Context);
                break;

            case BinaryExpressionType.Modulo:
                Result = MathHelper.Modulo(leftValue.Value, rightValue.Value, Context);
                break;

            case BinaryExpressionType.NotEqual:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) != 0;
                break;

            case BinaryExpressionType.Plus:
                if (leftValue.Value is string)
                {
                    Result = string.Concat(leftValue.Value, rightValue.Value);
                }
                else
                {
                    Result = MathHelper.Add(leftValue.Value, rightValue.Value, Context);
                }

                break;

            case BinaryExpressionType.Times:
                Result = MathHelper.Multiply(leftValue.Value, rightValue.Value, Context);
                break;

            case BinaryExpressionType.BitwiseAnd:
                Result = Convert.ToUInt64(leftValue.Value, Context.CultureInfo) &
						 Convert.ToUInt64(rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.BitwiseOr:
                Result = Convert.ToUInt64(leftValue.Value, Context.CultureInfo) |
						 Convert.ToUInt64(rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.BitwiseXOr:
                Result = Convert.ToUInt64(leftValue.Value, Context.CultureInfo) ^
						 Convert.ToUInt64(rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.LeftShift:
                Result = Convert.ToUInt64(leftValue.Value, Context.CultureInfo) <<
						 Convert.ToInt32(rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.RightShift:
                Result = Convert.ToUInt64(leftValue.Value, Context.CultureInfo) >>
						 Convert.ToInt32(rightValue.Value, Context.CultureInfo);
                break;

            case BinaryExpressionType.Exponentiation:
                Result = Math.Pow(Convert.ToDouble(leftValue.Value, Context.CultureInfo),
                    Convert.ToDouble(rightValue.Value, Context.CultureInfo));
                break;
        }
    }
    
    public void Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        expression.Expression.Accept(this);

        Result = expression.Type switch
        {
            UnaryExpressionType.Not => !Convert.ToBoolean(Result, Context.CultureInfo),
            UnaryExpressionType.Negate => MathHelper.Subtract(0, Result, Context),
            UnaryExpressionType.BitwiseNot => ~Convert.ToUInt64(Result, Context.CultureInfo),
            UnaryExpressionType.Positive => Result,
            _ => throw new InvalidOperationException("Unknown UnaryExpressionType")
        };
    }

    public void Visit(ValueExpression expression)
    {
        Result = expression.Value;
    }

    public void Visit(Function function)
    {
        var argsCount = function.Expressions.Length;
        var args = new Expression[argsCount];

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (var i = 0; i < argsCount; i++)
        {
            args[i] = new Expression(function.Expressions[i], Context);
        }

        var functionName = function.Identifier.Name;
        var functionArgs = new FunctionArgs(args);
        
        OnEvaluateFunction(functionName, functionArgs);

        if (functionArgs.HasResult)
        {
            Result = functionArgs.Result;
        }
        else if (Context.Functions.TryGetValue(functionName, out var expressionFunction))
        {
            Result = expressionFunction(args, Context);
        }
        else
        {
            Result = BuiltInFunctionHelper.Evaluate(functionName, args, Context);
        }
    }
    
    public void Visit(Identifier identifier)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new ParameterArgs();
        
        OnEvaluateParameter(identifierName, parameterArgs);
        
        if (parameterArgs.HasResult)
        {
            Result = parameterArgs.Result;
        }
        else if (Context.StaticParameters.TryGetValue(identifierName, out var parameter))
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
                
                Result = expression.Evaluate();
            }
            else
            {
                Result = parameter;
            }
        }
        else if (Context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
        {
            Result = dynamicParameter(Context);
        }
        else
        {
            throw new NCalcParameterNotDefinedException(identifierName);
        }
    }
    
    protected int CompareUsingMostPreciseType(object? a, object? b)
    {
        return TypeHelper.CompareUsingMostPreciseType(a,
            b, new(Context.CultureInfo,
                Context.Options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer),
                Context.Options.HasFlag(ExpressionOptions.OrdinalStringComparer)));
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
        expression.Accept(this);
        return Result;
    }
}