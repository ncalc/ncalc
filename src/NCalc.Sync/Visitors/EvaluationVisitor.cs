using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;
using static NCalc.Helpers.TypeHelper;
using static NCalc.Helpers.EvaluationHelper<NCalc.ExpressionContext>;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
public class EvaluationVisitor(ExpressionContext context) : ILogicalExpressionVisitor<object?>
{
    public virtual object? Visit(TernaryExpression expression, CancellationToken ct = default)
    {
        // Evaluates the left expression and saves the value
        var left = Convert.ToBoolean(expression.LeftExpression.Accept(this, ct), context.CultureInfo);

        if (left)
        {
            return expression.MiddleExpression.Accept(this, ct);
        }

        return expression.RightExpression.Accept(this, ct);
    }

    public virtual object? Visit(BinaryExpression expression, CancellationToken ct = default)
    {
        var left = new Lazy<object?>(() => Evaluate(expression.LeftExpression, ct), LazyThreadSafetyMode.None);
        var right = new Lazy<object?>(() => Evaluate(expression.RightExpression, ct), LazyThreadSafetyMode.None);

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                return Convert.ToBoolean(left.Value, context.CultureInfo) &&
                       Convert.ToBoolean(right.Value, context.CultureInfo);

            case BinaryExpressionType.Or:
                return Convert.ToBoolean(left.Value, context.CultureInfo) ||
                       Convert.ToBoolean(right.Value, context.CultureInfo);

            case BinaryExpressionType.Div:
                return Div(left.Value, right.Value, context);

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
                return Minus(left.Value, right.Value, context);

            case BinaryExpressionType.Modulo:
                return Modulo(left.Value, right.Value, context);

            case BinaryExpressionType.Plus:
                return Plus(left.Value, right.Value, context);

            case BinaryExpressionType.Times:
                return Times(left.Value, right.Value, context);

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
                return In(right.Value, left.Value, context);

            case BinaryExpressionType.NotIn:
                return !In(right.Value, left.Value, context);

            case BinaryExpressionType.Like:
                return Like(left.Value, right.Value, context);

            case BinaryExpressionType.NotLike:
                return !Like(left.Value, right.Value, context);
        }

        return null;
    }

    public virtual object? Visit(UnaryExpression expression, CancellationToken ct = default)
    {
        // Recursively evaluates the underlying expression
        var result = expression.Expression.Accept(this, ct);

        return Unary(expression, result, context);
    }

    public virtual object? Visit(ValueExpression expression, CancellationToken ct = default) => expression.Value;

    public virtual object? Visit(Function function, CancellationToken ct = default)
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
        var functionArgs = new FunctionArgs(function.Identifier.Id, args, ct);

        OnEvaluateFunction(functionName, functionArgs);

        if (functionArgs.HasResult)
            return functionArgs.Result;

        if (context.Functions.TryGetValue(functionName, out var expressionFunction))
        {
            return expressionFunction(new ExpressionFunctionData(function.Identifier.Id, args, context, ct));
        }

        return BuiltInFunctionHelper.Evaluate(functionName, args, context, ct);
    }

    public virtual object? Visit(Identifier identifier, CancellationToken ct = default)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new ParameterArgs(identifier.Id, ct);

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

                return expression.Evaluate(ct);
            }

            return parameter;
        }

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
        {
            return dynamicParameter(new ExpressionParameterData(identifier.Id, context, ct));
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    public virtual object Visit(LogicalExpressionList list, CancellationToken ct = default)
    {
        List<object?> result = [];

        result.AddRange(list.Select(e => Evaluate(e, ct)));

        return result;
    }

    protected bool Compare(object? a, object? b, ComparisonType comparisonType)
    {
        if (HasNullOrTypeConflict(a, b, context.Options))
            return comparisonType == ComparisonType.NotEqual;

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

    protected object? Evaluate(LogicalExpression expression, CancellationToken ct = default)
    {
        return expression.Accept(this, ct);
    }
}
