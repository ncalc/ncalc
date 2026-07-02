using NCalc.Exceptions;

namespace NCalc.Helpers;

/// <summary>
/// Provides helper methods for evaluating expressions.
/// </summary>
public static class EvaluationHelper
{
    private static (object? Left, object? Right) ConvertArithmeticNullOrEmptyStringsAsZero(
        object? leftValue,
        object? rightValue,
        ExpressionContext context)
    {
        if (!context.Options.HasFlag(ExpressionOptions.ArithmeticNullOrEmptyStringAsZero))
            return (leftValue, rightValue);

        if (leftValue is null or string { Length: 0 })
            leftValue = 0;

        if (rightValue is null or string { Length: 0 })
            rightValue = 0;

        return (leftValue, rightValue);
    }

    /// <summary>
    /// Adds two values, with special handling for string concatenation based on the context options.
    /// </summary>
    /// <param name="leftValue">The left operand.</param>
    /// <param name="rightValue">The right operand.</param>
    /// <param name="context">The evaluation context.</param>
    /// <returns>The result of the addition or string concatenation.</returns>
    public static object? Plus(object? leftValue, object? rightValue, ExpressionContext context)
    {
        if (context.Options.HasFlag(ExpressionOptions.StringConcat))
            return string.Concat(
                Convert.ToString(leftValue, context.CultureInfo),
                Convert.ToString(rightValue, context.CultureInfo));

        (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, context);

        if (context.Options.HasFlag(ExpressionOptions.NoStringTypeCoercion) &&
            (leftValue is string || rightValue is string))
        {
            return string.Concat(Convert.ToString(leftValue, context.CultureInfo), Convert.ToString(rightValue, context.CultureInfo));
        }

        try
        {
            return MathHelper.Add(leftValue, rightValue, context.MathHelperOptions);
        }
        catch (FormatException) when (leftValue is string && rightValue is string)
        {
            return string.Concat(
                Convert.ToString(leftValue, context.CultureInfo),
                Convert.ToString(rightValue, context.CultureInfo));
        }
    }

        public static object? Minus(object? leftValue, object? rightValue, ExpressionContext context)
        {
            (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, context);
            return MathHelper.Subtract(leftValue, rightValue, context.MathHelperOptions);
        }

        public static object? Times(object? leftValue, object? rightValue, ExpressionContext context)
        {
            (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, context);
            return MathHelper.Multiply(leftValue, rightValue, context.MathHelperOptions);
        }

        public static object? Div(object? leftValue, object? rightValue, ExpressionContext context)
        {
            (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, context);
            return MathHelper.Divide(leftValue, rightValue, context.MathHelperOptions);
        }

        public static object? Modulo(object? leftValue, object? rightValue, ExpressionContext context)
        {
            (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, context);
            return MathHelper.Modulo(leftValue, rightValue, context.MathHelperOptions);
        }

        /// <summary>
        /// Determines if the left value is contained within the right value, which must be either an enumerable or a string.
        /// </summary>
        /// <param name="rightValue">The right operand.</param>
        /// <param name="leftValue">The left operand.</param>
        /// <param name="context">The evaluation context.</param>
        /// <returns>True if the left value is contained within the right value, otherwise false.</returns>
        /// <exception cref="NCalcEvaluationException">Thrown when the right value is not an enumerable or a string.</exception>
        public static bool In(object? rightValue, object? leftValue, ExpressionContext context)
        {
            return rightValue switch
            {
                string rightValueString => Contains(leftValue, rightValueString, context),
                object[] rightValueArray => Contains(leftValue, rightValueArray, context),
                IEnumerable rightValueEnumerable => Contains(leftValue, rightValueEnumerable, context),
                { } rightValueObject => ValuesEqual(leftValue, rightValueObject, context),
                _ => throw new NCalcEvaluationException(
                    "'in' operator right value must implement IEnumerable, be a string or an object.")
            };
        }

        private static bool Contains(object? leftValue, string rightValue, ExpressionContext context)
        {
            if (leftValue is not string && context.Options.HasFlag(ExpressionOptions.NoStringTypeCoercion))
            {
                return false;
            }

            var leftValueString = Convert.ToString(leftValue, context.CultureInfo);

            if (string.IsNullOrEmpty(leftValueString))
                return string.IsNullOrEmpty(rightValue);

            return rightValue.Contains(leftValueString);
        }

        private static bool Contains<T>(object? leftValue, T rightValue, ExpressionContext context) where T : IEnumerable
        {
            foreach (var item in rightValue)
            {
                if (ValuesEqual(leftValue, item, context))
                    return true;
            }

            return false;
        }

        private static bool ValuesEqual(object? leftValue, object? rightValue, ExpressionContext context)
        {
            if (leftValue == null || rightValue == null)
                return leftValue == rightValue;

            var noStringTypeCoercion = context.Options.HasFlag(ExpressionOptions.NoStringTypeCoercion);

            if (noStringTypeCoercion)
            {
                if (leftValue is string || rightValue is string)
                    return EqualityComparer<object?>.Default.Equals(leftValue, rightValue);

                return TypeHelper.CompareUsingMostPreciseType(leftValue, rightValue, context.ComparisonOptions) == ComparisonResult.Equal;
            }

            if (TypeHelper.HasNullOrTypeConflict(leftValue, rightValue, context.Options))
                return false;

            var stringComparer = TypeHelper.GetStringComparer(context.ComparisonOptions);

            return (leftValue, rightValue) switch
            {
                (string leftString, string rightString) => stringComparer.Equals(leftString, rightString),
                (string leftString, _) => stringComparer.Equals(
                    leftString,
                    Convert.ToString(rightValue, context.CultureInfo) ?? string.Empty),
                (_, string rightString) => stringComparer.Equals(
                    Convert.ToString(leftValue, context.CultureInfo) ?? string.Empty,
                    rightString),
                _ => TypeHelper.CompareUsingMostPreciseType(leftValue, rightValue, context.ComparisonOptions) == ComparisonResult.Equal
            };
        }

        /// <summary>
        /// Evaluates a unary expression.
        /// </summary>
        /// <param name="expression">The unary expression to evaluate.</param>
        /// <param name="result">The result of evaluating the operand of the unary expression.</param>
        /// <param name="context">The evaluation context.</param>
        /// <returns>The result of the unary operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the unary expression type is unknown.</exception>
        public static object? Unary(UnaryExpression expression, object? result, ExpressionContext context)
        {
            return expression.Type switch
            {
                UnaryExpressionType.Not => !Convert.ToBoolean(result, context.CultureInfo),
                UnaryExpressionType.Negate => result switch
                {
                    double doubleValue => -doubleValue,
                    float floatValue => -floatValue,
                    _ => MathHelper.Subtract(0, result, context.MathHelperOptions)
                },
                UnaryExpressionType.BitwiseNot => ~Convert.ToUInt64(result, context.CultureInfo),
                UnaryExpressionType.Positive => result,
                UnaryExpressionType.Factorial => MathHelper.Factorial(result),
                _ => throw new InvalidOperationException("Unknown UnaryExpressionType")
            };
        }

        [Obsolete("Please use LikeOperatorHelper.Like.")]
        public static bool Like(object? leftValue, object? rightValue, ExpressionContext context)
        {
            return LikeOperatorHelper.Like(leftValue, rightValue, context);
        }
}
