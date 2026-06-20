using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;
using static NCalc.Helpers.EvaluationHelper;
using static NCalc.Helpers.TypeHelper;

namespace NCalc.Visitors;

internal static class EvaluationVisitorHelper
{
    internal static object? EvaluateBinary(
        BinaryExpressionType expressionType,
        object? left,
        object? right,
        ExpressionContext context)
    {
        switch (expressionType)
        {
            case BinaryExpressionType.Div:
                return Div(left, right, context);

            case BinaryExpressionType.Equal:
                return Compare(left, right, ComparisonType.Equal, context);

            case BinaryExpressionType.Greater:
                return Compare(left, right, ComparisonType.Greater, context);

            case BinaryExpressionType.GreaterOrEqual:
                return Compare(left, right, ComparisonType.GreaterOrEqual, context);

            case BinaryExpressionType.Lesser:
                return Compare(left, right, ComparisonType.Lesser, context);

            case BinaryExpressionType.LesserOrEqual:
                return Compare(left, right, ComparisonType.LesserOrEqual, context);

            case BinaryExpressionType.NotEqual:
                return Compare(left, right, ComparisonType.NotEqual, context);

            case BinaryExpressionType.Minus:
                return Minus(left, right, context);

            case BinaryExpressionType.Modulo:
                return Modulo(left, right, context);

            case BinaryExpressionType.Plus:
                return Plus(left, right, context);

            case BinaryExpressionType.Times:
                return Times(left, right, context);

            case BinaryExpressionType.BitwiseAnd:
                return Convert.ToUInt64(left, context.CultureInfo) &
                       Convert.ToUInt64(right, context.CultureInfo);

            case BinaryExpressionType.BitwiseOr:
                return Convert.ToUInt64(left, context.CultureInfo) |
                       Convert.ToUInt64(right, context.CultureInfo);

            case BinaryExpressionType.BitwiseXOr:
                return Convert.ToUInt64(left, context.CultureInfo) ^
                       Convert.ToUInt64(right, context.CultureInfo);

            case BinaryExpressionType.LeftShift:
                return Convert.ToUInt64(left, context.CultureInfo) <<
                       Convert.ToInt32(right, context.CultureInfo);

            case BinaryExpressionType.RightShift:
                return Convert.ToUInt64(left, context.CultureInfo) >>
                       Convert.ToInt32(right, context.CultureInfo);

            case BinaryExpressionType.Exponentiation:
                return MathHelper.Pow(left, right, context);

            case BinaryExpressionType.In:
                return In(right, left, context);

            case BinaryExpressionType.NotIn:
                return !In(right, left, context);

            case BinaryExpressionType.Like:
                return Like(left, right, context);

            case BinaryExpressionType.NotLike:
                return !Like(left, right, context);

            default:
                return null;
        }
    }

    internal static bool Compare(
        object? a,
        object? b,
        ComparisonType comparisonType,
        ExpressionContext context)
    {
        if (HasNullOrTypeConflict(a, b, context.Options))
            return comparisonType == ComparisonType.NotEqual;

        var result = CompareUsingMostPreciseType(a, b, context);

        return comparisonType switch
        {
            ComparisonType.Equal => result is ComparisonResult.Equal,
            ComparisonType.Greater => result is ComparisonResult.Greater,
            ComparisonType.GreaterOrEqual => result is ComparisonResult.Greater or ComparisonResult.Equal,
            ComparisonType.Lesser => result is ComparisonResult.Less,
            ComparisonType.LesserOrEqual => result is ComparisonResult.Less or ComparisonResult.Equal,
            ComparisonType.NotEqual => result is not ComparisonResult.Equal,
            _ => throw new ArgumentOutOfRangeException(nameof(comparisonType), comparisonType, null)
        };
    }

    internal static object? GetIdentifierValue(
        Identifier identifier,
        ExpressionContext context,
        CancellationToken cancellationToken)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new ParameterEventArgs(identifier.Id, cancellationToken);

        context.EvaluateParameterHandler?.Invoke(identifierName, parameterArgs);

        if (parameterArgs.HasResult)
            return parameterArgs.Result;

        if (context.StaticParameters.TryGetValue(identifierName, out var parameter))
        {
            if (parameter is Expression expression)
            {
                ShareParametersWithChildExpression(expression, context);

                return expression;
            }

            return parameter;
        }

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
            return dynamicParameter(new ParameterData(identifier.Id, context, cancellationToken));

        if (identifierName.Equals("null", StringComparison.InvariantCultureIgnoreCase) &&
            context.Options.HasFlag(ExpressionOptions.AllowNullParameter))
        {
            return null;
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    private static void ShareParametersWithChildExpression(Expression expression, ExpressionContext context)
    {
        foreach (var p in context.StaticParameters)
            expression.Parameters[p.Key] = p.Value;

        foreach (var p in context.DynamicParameters)
            expression.DynamicParameters[p.Key] = p.Value;

        expression.EvaluateFunction += context.EvaluateFunctionHandler;
        expression.EvaluateAsyncFunction += context.EvaluateAsyncFunctionHandler;

        expression.EvaluateParameter += context.EvaluateParameterHandler;

        expression.EvaluateBinary += context.EvaluateBinaryHandler;
        expression.EvaluateBinaryAsync += context.EvaluateBinaryAsyncHandler;
    }
}
