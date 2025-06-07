using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;
using static NCalc.Helpers.TypeHelper;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
public class EvaluationVisitor(ExpressionContext context) : ILogicalExpressionVisitor<object?>
{
    public virtual object? Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(expression.LeftExpression.Accept(this), context.CultureInfo);

        if (left)
        {
            return expression.MiddleExpression.Accept(this);
        }

        return expression.RightExpression.Accept(this);
    }

    public virtual object? Visit(BinaryExpression expression)
    {
        var left = new Lazy<object?>(() => Evaluate(expression.LeftExpression), LazyThreadSafetyMode.None);
        var right = new Lazy<object?>(() => Evaluate(expression.RightExpression), LazyThreadSafetyMode.None);

        if (expression.LeftExpression is PercentExpression && expression.RightExpression is PercentExpression)
        {
            switch (expression.Type)
            {
                case BinaryExpressionType.Minus:
                    return new PercentExpression(new ValueExpression(MathHelper.Subtract(left.Value, right.Value, context) ?? 0));
                case BinaryExpressionType.Plus:
                    return new PercentExpression(new ValueExpression(MathHelper.Add(left.Value, right.Value, context) ?? 0));
            }
        }
        else
        if (expression.LeftExpression is PercentExpression)
        {
            switch (expression.Type)
            {
                case BinaryExpressionType.Times:
                    return new PercentExpression(new ValueExpression(MathHelper.Multiply(left.Value, right.Value, context) ?? 0));
                case BinaryExpressionType.Div:
                    return new PercentExpression(new ValueExpression(MathHelper.Divide(left.Value, right.Value, context) ?? 0));
            }
        }
        else
        if (expression.RightExpression is PercentExpression)
        {
            switch (expression.Type)
            {
                case BinaryExpressionType.Minus:
                    return MathHelper.SubtractPercent(left.Value, right.Value, context);
                case BinaryExpressionType.Plus:
                    return MathHelper.AddPercent(left.Value, right.Value, context);
                case BinaryExpressionType.Times:
                    return MathHelper.MultiplyPercent(left.Value, right.Value, context);
                case BinaryExpressionType.Div:
                    return MathHelper.DividePercent(left.Value, right.Value, context);
            }
        }
        else
        {
            switch (expression.Type)
            {
                case BinaryExpressionType.And:
                    return Convert.ToBoolean(left.Value, context.CultureInfo) &&
                           Convert.ToBoolean(right.Value, context.CultureInfo);

            case BinaryExpressionType.Or:
                return Convert.ToBoolean(left.Value, context.CultureInfo) ||
                       Convert.ToBoolean(right.Value, context.CultureInfo);

            case BinaryExpressionType.Div:
                return IsReal(left.Value) || IsReal(right.Value)
                    ? MathHelper.Divide(left.Value, right.Value, context)
                    : MathHelper.Divide(Convert.ToDouble(left.Value, context.CultureInfo), right.Value,
                        context);

            case BinaryExpressionType.Equal:
                return Compare(left.Value, right.Value, ComparisonType.Equal);

            case BinaryExpressionType.Greater:
                return Compare(left.Value, right.Value, ComparisonType.Greater);

            case BinaryExpressionType.GreaterOrEqual:
                return Compare(left.Value, right.Value, ComparisonType.GreaterOrEqual);

            case BinaryExpressionType.Lesser:
                return Compare(left.Value, right.Value, ComparisonType.Lesser);

            case BinaryExpressionType.LesserOrEqual:
                return Compare(left.Value, right.Value, ComparisonType.LesserOrEqual);

            case BinaryExpressionType.NotEqual:
                return Compare(left.Value, right.Value, ComparisonType.NotEqual);

            case BinaryExpressionType.Minus:
                return MathHelper.Subtract(left.Value, right.Value, context);

            case BinaryExpressionType.Modulo:
                return MathHelper.Modulo(left.Value, right.Value, context);

            case BinaryExpressionType.Plus:
                return EvaluationHelper.Plus(left.Value, right.Value, context);

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
                return MathHelper.Pow(left.Value, right.Value, context);

            case BinaryExpressionType.In:
                return EvaluationHelper.In(right.Value, left.Value, context);

            case BinaryExpressionType.NotIn:
                return !EvaluationHelper.In(right.Value, left.Value, context);

            case BinaryExpressionType.Like:
            {
                var rightValue = right.Value?.ToString();
                var leftValue = left.Value?.ToString();

                if (rightValue == null || leftValue == null)
                {
                    return false;
                }

                return EvaluationHelper.Like(leftValue, rightValue, context);
            }

            case BinaryExpressionType.NotLike:
            {
                var rightValue = right.Value?.ToString();
                var leftValue = left.Value?.ToString();

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

    public virtual object? Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        var result = expression.Expression.Accept(this);

        return EvaluationHelper.Unary(expression, result, context);
    }

    public virtual object? Visit(PercentExpression expression)
    {
        // Recursively evaluates the underlying expression
        return expression.Expression.Accept(this);
    }

    public virtual object? Visit(ValueExpression expression) => expression.Value;

    public virtual object? Visit(Function function)
    {
        var argsCount = function.Parameters.Count;
        var args = new Expression[argsCount];

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (var i = 0; i < argsCount; i++)
        {
            args[i] = new Expression(function.Parameters[i], context);
        }

        var functionName = function.Identifier.Name;
        var functionArgs = new FunctionArgs(function.Identifier.Id, args);

        OnEvaluateFunction(functionName, functionArgs);

        if (functionArgs.HasResult)
            return functionArgs.Result;

        if (context.Functions.TryGetValue(functionName, out var expressionFunction))
        {
            return expressionFunction(new ExpressionFunctionData(function.Identifier.Id, args, context));
        }

        return BuiltInFunctionHelper.Evaluate(functionName, args, context);
    }

    public virtual object? Visit(Identifier identifier)
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
            return dynamicParameter(new ExpressionParameterData(identifier.Id, context));
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    public virtual object Visit(LogicalExpressionList list)
    {
        List<object?> result = [];

        result.AddRange(list.Select(Evaluate));

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

    protected void OnEvaluateFunction(string name, FunctionArgs args)
    {
        context.EvaluateFunctionHandler?.Invoke(name, args);
    }

    protected void OnEvaluateParameter(string name, ParameterArgs args)
    {
        context.EvaluateParameterHandler?.Invoke(name, args);
    }

    protected object? Evaluate(LogicalExpression expression)
    {
        return expression.Accept(this);
    }
}