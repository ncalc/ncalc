using NCalc.Exceptions;

namespace NCalc.Helpers;

public static class EvaluationHelper
{
    private static (object? Left, object? Right) ConvertArithmeticNullOrEmptyStringsAsZero(
        object? leftValue,
        object? rightValue,
        ExpressionEvaluationOptions options)
    {
        if (!options.ArithmeticNullOrEmptyStringAsZero)
            return (leftValue, rightValue);

        if (leftValue is null or string { Length: 0 })
            leftValue = 0;

        if (rightValue is null or string { Length: 0 })
            rightValue = 0;

        return (leftValue, rightValue);
    }

    public static object? Plus(object? leftValue, object? rightValue, ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        if (options.StringConcat)
            return string.Concat(
                Convert.ToString(leftValue, cultureInfo),
                Convert.ToString(rightValue, cultureInfo));

        (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, options);

        if (options.NoStringTypeCoercion &&
            (leftValue is string || rightValue is string))
        {
            return string.Concat(Convert.ToString(leftValue, cultureInfo), Convert.ToString(rightValue, cultureInfo));
        }

        try
        {
            return MathHelper.Add(leftValue, rightValue, options.Math, cultureInfo);
        }
        catch (FormatException) when (leftValue is string && rightValue is string)
        {
            return string.Concat(
                Convert.ToString(leftValue, cultureInfo),
                Convert.ToString(rightValue, cultureInfo));
        }
    }

    public static object? Minus(object? leftValue, object? rightValue, ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, options);
        return MathHelper.Subtract(leftValue, rightValue, options.Math, cultureInfo);
    }

    public static object? Times(object? leftValue, object? rightValue, ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, options);
        return MathHelper.Multiply(leftValue, rightValue, options.Math, cultureInfo);
    }

    public static object? Div(object? leftValue, object? rightValue, ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, options);
        return MathHelper.Divide(leftValue, rightValue, options.Math, cultureInfo);
    }

    public static object? Modulo(object? leftValue, object? rightValue, ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, options);
        return MathHelper.Modulo(leftValue, rightValue, options.Math, cultureInfo);
    }

    public static bool In(object? rightValue, object? leftValue, ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        return rightValue switch
        {
            string rightValueString => Contains(leftValue, rightValueString, options, cultureInfo),
            object[] rightValueArray => Contains(leftValue, rightValueArray, options, cultureInfo),
            IEnumerable rightValueEnumerable => Contains(leftValue, rightValueEnumerable, options, cultureInfo),
            not null => ValuesEqual(leftValue, rightValue, options, cultureInfo),
            _ => throw new NCalcEvaluationException(
                "'in' operator right value must implement IEnumerable, be a string or an object.")
        };
    }

    private static bool Contains(object? leftValue, string rightValue, ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        if (leftValue is not string && options.NoStringTypeCoercion)
        {
            return false;
        }

        var leftValueString = Convert.ToString(leftValue, cultureInfo);

        if (string.IsNullOrEmpty(leftValueString))
            return string.IsNullOrEmpty(rightValue);

        return rightValue.Contains(leftValueString);
    }

    private static bool Contains<T>(object? leftValue, T rightValue, ExpressionEvaluationOptions options,
        CultureInfo cultureInfo) where T : IEnumerable
    {
        foreach (var item in rightValue)
        {
            if (ValuesEqual(leftValue, item, options, cultureInfo))
                return true;
        }

        return false;
    }

    private static bool ValuesEqual(object? leftValue, object? rightValue, ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        if (leftValue == null || rightValue == null)
            return leftValue == rightValue;

        var noStringTypeCoercion = options.NoStringTypeCoercion;

        if (noStringTypeCoercion)
        {
            if (leftValue is string || rightValue is string)
                return EqualityComparer<object?>.Default.Equals(leftValue, rightValue);

            return TypeHelper.CompareUsingMostPreciseType(leftValue, rightValue, options.StringComparer, cultureInfo) ==
                   ComparisonResult.Equal;
        }

        if (TypeHelper.HasNullOrTypeConflict(leftValue, rightValue, options.StrictTypeMatching))
            return false;

        var stringComparer = options.StringComparer;

        return (leftValue, rightValue) switch
        {
            (string leftString, string rightString) => stringComparer.Equals(leftString, rightString),
            (string leftString, _) => stringComparer.Equals(
                leftString,
                Convert.ToString(rightValue, cultureInfo) ?? string.Empty),
            (_, string rightString) => stringComparer.Equals(
                Convert.ToString(leftValue, cultureInfo) ?? string.Empty,
                rightString),
            _ => TypeHelper.CompareUsingMostPreciseType(leftValue, rightValue, options.StringComparer, cultureInfo) ==
                 ComparisonResult.Equal
        };
    }

    public static object? Unary(UnaryExpression expression, object? result, ExpressionEvaluationOptions options,
        CultureInfo cultureInfo)
    {
        return expression.Type switch
        {
            UnaryExpressionType.Not => !Convert.ToBoolean(result, cultureInfo),
            UnaryExpressionType.Negate => result switch
            {
                double doubleValue => -doubleValue,
                float floatValue => -floatValue,
                _ => MathHelper.Subtract(0, result, options.Math, cultureInfo)
            },
            UnaryExpressionType.BitwiseNot => ~Convert.ToUInt64(result, cultureInfo),
            UnaryExpressionType.Positive => result,
            UnaryExpressionType.Factorial => MathHelper.Factorial(result),
            _ => throw new InvalidOperationException("Unknown UnaryExpressionType")
        };
    }
}
