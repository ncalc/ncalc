using NCalc.Helpers;

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
                return EvaluationHelper.Div(left, right, context);

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
                return EvaluationHelper.Minus(left, right, context);

            case BinaryExpressionType.Modulo:
                return EvaluationHelper.Modulo(left, right, context);

            case BinaryExpressionType.Plus:
                return EvaluationHelper.Plus(left, right, context);

            case BinaryExpressionType.Times:
                return EvaluationHelper.Times(left, right, context);

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
                return MathHelper.Pow(left, right, context.MathHelperOptions);

            case BinaryExpressionType.In:
                return EvaluationHelper.In(right, left, context);

            case BinaryExpressionType.NotIn:
                return !EvaluationHelper.In(right, left, context);

            case BinaryExpressionType.Like:
                return LikeOperatorHelper.Like(left, right, context);

            case BinaryExpressionType.NotLike:
                return !LikeOperatorHelper.Like(left, right, context);

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
        if (TypeHelper.HasNullOrTypeConflict(a, b, context.Options))
            return comparisonType == ComparisonType.NotEqual;

        var result = TypeHelper.CompareUsingMostPreciseType(a, b, context.ComparisonOptions);

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
}
