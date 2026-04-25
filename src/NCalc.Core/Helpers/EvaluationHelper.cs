using System.Text.RegularExpressions;
using NCalc.Domain;
using NCalc.Exceptions;

namespace NCalc.Helpers;

/// <summary>
/// Provides helper methods for evaluating expressions.
/// </summary>
public static class EvaluationHelper<TExpressionContext> where TExpressionContext : ExpressionContextBase
{
    private static (object? Left, object? Right) ConvertArithmeticNullOrEmptyStringsAsZero(
        object? leftValue,
        object? rightValue,
        TExpressionContext context)
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
    public static object? Plus(object? leftValue, object? rightValue, TExpressionContext context)
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
            return MathHelper.Add(leftValue, rightValue, context);
        }
        catch (FormatException) when (leftValue is string && rightValue is string)
        {
            return string.Concat(
                Convert.ToString(leftValue, context.CultureInfo),
                Convert.ToString(rightValue, context.CultureInfo));
        }
    }

        public static object? Minus(object? leftValue, object? rightValue, TExpressionContext context)
        {
            (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, context);
            return MathHelper.Subtract(leftValue, rightValue, context);
        }

        public static object? Times(object? leftValue, object? rightValue, TExpressionContext context)
        {
            (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, context);
            return MathHelper.Multiply(leftValue, rightValue, context);
        }

        public static object? Div(object? leftValue, object? rightValue, TExpressionContext context)
        {
            (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, context);
            return MathHelper.Divide(leftValue, rightValue, context);
        }

        public static object? Modulo(object? leftValue, object? rightValue, TExpressionContext context)
        {
            (leftValue, rightValue) = ConvertArithmeticNullOrEmptyStringsAsZero(leftValue, rightValue, context);
            return MathHelper.Modulo(leftValue, rightValue, context);
        }

        /// <summary>
        /// Determines if the left value is contained within the right value, which must be either an enumerable or a string.
        /// </summary>
        /// <param name="rightValue">The right operand.</param>
        /// <param name="leftValue">The left operand.</param>
        /// <param name="context">The evaluation context.</param>
        /// <returns>True if the left value is contained within the right value, otherwise false.</returns>
        /// <exception cref="NCalcEvaluationException">Thrown when the right value is not an enumerable or a string.</exception>
        public static bool In(object? rightValue, object? leftValue, TExpressionContext context)
        {
            return rightValue switch
            {
                string rightValueString => Contains(leftValue, rightValueString, context),
                IEnumerable<object?> rightValueEnumerableOfObj => Contains(leftValue, rightValueEnumerableOfObj, context),
                IEnumerable rightValueEnumerable => Contains(leftValue, rightValueEnumerable, context),
                { } rightValueObject => Contains(leftValue, [rightValueObject], context),
                _ => throw new NCalcEvaluationException(
                    "'in' operator right value must implement IEnumerable, be a string or an object.")
            };
        }

        private static bool Contains(object? leftValue, string rightValue, TExpressionContext context)
        {
            if (leftValue is not string && context.Options.HasFlag(ExpressionOptions.NoStringTypeCoercion))
            {
                return false;
            }

            var leftValueString = Convert.ToString(leftValue, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(leftValueString))
                return string.IsNullOrEmpty(rightValue);

            return rightValue.Contains(leftValueString);
        }

        private static bool Contains(object? leftValue, IEnumerable<object?> rightValue, TExpressionContext context)
        {
            var rightArray = rightValue as object[] ?? rightValue.ToArray();

            var noStringTypeCoercion = context.Options.HasFlag(ExpressionOptions.NoStringTypeCoercion);

            if (rightArray.All(v => v is string))
            {
                if (noStringTypeCoercion && leftValue is not string)
                {
                    return false;
                }

                return rightArray.OfType<string>().Contains(Convert.ToString(leftValue, context.CultureInfo) ?? string.Empty,
                    TypeHelper.GetStringComparer(context));
            }

            return rightArray.Contains(leftValue,
                noStringTypeCoercion ? EqualityComparer<object?>.Default : StringCoercionComparer.Default);
        }

        private static bool Contains(object? leftValue, IEnumerable rightValue, TExpressionContext context)
        {
            if (rightValue == null)
                return false;

            if (leftValue == null)
            {
                foreach (var item in rightValue)
                {
                    if (item == null)
                        return true;
                }

                return false;
            }

            var leftType = leftValue.GetType();

            var noStringTypeCoercion = context.Options.HasFlag(ExpressionOptions.NoStringTypeCoercion);
            var comparer = noStringTypeCoercion ? EqualityComparer<object?>.Default : StringCoercionComparer.Default;

            foreach (var item in rightValue)
            {
                if (item != null)
                {
                    var rightType = item.GetType();

                    if (rightType == leftType)
                    {
                        if (leftValue.Equals(item))
                            return true;
                    }
                    else if (comparer.Equals(leftValue, item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Evaluates a unary expression.
        /// </summary>
        /// <param name="expression">The unary expression to evaluate.</param>
        /// <param name="result">The result of evaluating the operand of the unary expression.</param>
        /// <param name="context">The evaluation context.</param>
        /// <returns>The result of the unary operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the unary expression type is unknown.</exception>
        public static object? Unary(UnaryExpression expression, object? result, TExpressionContext context)
        {
            return expression.Type switch
            {
                UnaryExpressionType.Not => !Convert.ToBoolean(result, context.CultureInfo),
                UnaryExpressionType.Negate => MathHelper.Subtract(0, result, context),
                UnaryExpressionType.BitwiseNot => ~Convert.ToUInt64(result, context.CultureInfo),
                UnaryExpressionType.Positive => result,
                UnaryExpressionType.Factorial => MathHelper.Factorial(result),
                _ => throw new InvalidOperationException("Unknown UnaryExpressionType")
            };
        }

        /// <summary>
        /// Determines whether a specified string matches a pattern using SQL-like wildcards.
        /// </summary>
        /// <param name="leftValue">The left operand.</param>///
        /// <param name="rightValue">The right operand.</param>
        /// <param name="context">The context containing options for the comparison.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="value"/> matches the <paramref name="pattern"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The comparison is case-insensitive if the <see cref="ExpressionOptions.CaseInsensitiveStringComparer"/> flag is set in the <paramref name="context"/>.
        /// </remarks>
        public static bool Like(object? leftValue, object? rightValue, TExpressionContext context)
        {
            if (leftValue == null || rightValue == null)
                return false;

            string value = leftValue.ToString()!;
            string pattern = rightValue.ToString()!;

            var regexPattern = Regex.Escape(pattern)
                .Replace("%", ".*") // % matches zero or more characters
                .Replace("_", "."); // _ matches exactly one character

            var options = context.Options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer)
                ? RegexOptions.IgnoreCase
                : RegexOptions.None;

            // Use ^ and $ to match the start and end of the string
            return Regex.IsMatch(value, $"^{regexPattern}$", options);
        }
}
