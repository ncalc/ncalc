using NCalc.Helpers;

namespace NCalc.Visitors;

internal static class EvaluationVisitorHelper
{
    internal static object? EvaluateBinary(
        BinaryExpressionType expressionType,
        object? left,
        object? right,
        ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        switch (expressionType)
        {
            case BinaryExpressionType.Div:
                return EvaluationHelper.Div(left, right, options, cultureInfo);

            case BinaryExpressionType.Equal:
                return Compare(left, right, ComparisonType.Equal, options, cultureInfo);

            case BinaryExpressionType.Greater:
                return Compare(left, right, ComparisonType.Greater, options, cultureInfo);

            case BinaryExpressionType.GreaterOrEqual:
                return Compare(left, right, ComparisonType.GreaterOrEqual, options, cultureInfo);

            case BinaryExpressionType.Lesser:
                return Compare(left, right, ComparisonType.Lesser, options, cultureInfo);

            case BinaryExpressionType.LesserOrEqual:
                return Compare(left, right, ComparisonType.LesserOrEqual, options, cultureInfo);

            case BinaryExpressionType.NotEqual:
                return Compare(left, right, ComparisonType.NotEqual, options, cultureInfo);

            case BinaryExpressionType.Minus:
                return EvaluationHelper.Minus(left, right, options, cultureInfo);

            case BinaryExpressionType.Modulo:
                return EvaluationHelper.Modulo(left, right, options, cultureInfo);

            case BinaryExpressionType.Plus:
                return EvaluationHelper.Plus(left, right, options, cultureInfo);

            case BinaryExpressionType.Times:
                return EvaluationHelper.Times(left, right, options, cultureInfo);

            case BinaryExpressionType.BitwiseAnd:
                return Convert.ToUInt64(left, cultureInfo) &
                       Convert.ToUInt64(right, cultureInfo);

            case BinaryExpressionType.BitwiseOr:
                return Convert.ToUInt64(left, cultureInfo) |
                       Convert.ToUInt64(right, cultureInfo);

            case BinaryExpressionType.BitwiseXOr:
                return Convert.ToUInt64(left, cultureInfo) ^
                       Convert.ToUInt64(right, cultureInfo);

            case BinaryExpressionType.LeftShift:
                return Convert.ToUInt64(left, cultureInfo) <<
                       Convert.ToInt32(right, cultureInfo);

            case BinaryExpressionType.RightShift:
                return Convert.ToUInt64(left, cultureInfo) >>
                       Convert.ToInt32(right, cultureInfo);

            case BinaryExpressionType.Exponentiation:
                return MathHelper.Pow(left, right, options.Math, cultureInfo);

            case BinaryExpressionType.In:
                return EvaluationHelper.In(right, left, options, cultureInfo);

            case BinaryExpressionType.NotIn:
                return !EvaluationHelper.In(right, left, options, cultureInfo);

            case BinaryExpressionType.Like:
                return LikeOperatorHelper.Like(left, right, options.StringComparer);

            case BinaryExpressionType.NotLike:
                return !LikeOperatorHelper.Like(left, right, options.StringComparer);

            default:
                return null;
        }
    }

    internal static bool Compare(
        object? a,
        object? b,
        ComparisonType comparisonType,
        ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        if (TypeHelper.HasNullOrTypeConflict(a, b, options.StrictTypeMatching))
            return comparisonType == ComparisonType.NotEqual;

        var result = TypeHelper.CompareUsingMostPreciseType(a, b, options.StringComparer, cultureInfo);

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
