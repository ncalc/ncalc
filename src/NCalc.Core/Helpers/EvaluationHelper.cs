using System.Text.RegularExpressions;
using NCalc.Domain;
using NCalc.Exceptions;

namespace NCalc.Helpers;

/// <summary>
/// Provides helper methods for evaluating expressions.
/// </summary>
public static class EvaluationHelper
{
    /// <summary>
    /// Adds two values, with special handling for string concatenation based on the context options.
    /// </summary>
    /// <param name="leftValue">The left operand.</param>
    /// <param name="rightValue">The right operand.</param>
    /// <param name="context">The evaluation context.</param>
    /// <returns>The result of the addition or string concatenation.</returns>
    public static object? Plus(object? leftValue, object? rightValue, ExpressionContextBase context)
    {
        if (context.Options.HasFlag(ExpressionOptions.StringConcat))
            return string.Concat(leftValue, rightValue);

        if (context.Options.HasFlag(ExpressionOptions.NoStringTypeCoercion) &&
            (leftValue is string || rightValue is string))
        {
            return string.Concat(leftValue, rightValue);
        }

        try
        {
            return MathHelper.Add(leftValue, rightValue, context);
        }
        catch (FormatException) when (leftValue is string && rightValue is string)
        {
            return string.Concat(leftValue, rightValue);
        }
    }

    /// <summary>
    /// Determines if the left value is contained within the right value, which must be either an enumerable or a string.
    /// </summary>
    /// <param name="rightValue">The right operand.</param>
    /// <param name="leftValue">The left operand.</param>
    /// <param name="context">The evaluation context.</param>
    /// <returns>True if the left value is contained within the right value, otherwise false.</returns>
    /// <exception cref="NCalcEvaluationException">Thrown when the right value is not an enumerable or a string.</exception>
    public static bool In(object? rightValue, object? leftValue, ExpressionContextBase context)
    {
        return rightValue switch
        {
            string rightValueString => Contains(leftValue, rightValueString, context),
            IEnumerable<object?> rightValueEnumerable => Contains(leftValue, rightValueEnumerable, context),
            { } rightValueObject => Contains(leftValue, [rightValueObject], context),
            _ => throw new NCalcEvaluationException(
                "'in' operator right value must implement IEnumerable, be a string or an object.")
        };
    }

    private static bool Contains(object? leftValue, string rightValue, ExpressionContextBase context)
    {
        if (leftValue is not string && context.Options.HasFlag(ExpressionOptions.NoStringTypeCoercion))
        {
            return false;
        }

        var leftValueString = leftValue?.ToString();

        if (string.IsNullOrEmpty(leftValueString) && string.IsNullOrEmpty(rightValue))
            return true;

        if (string.IsNullOrEmpty(leftValueString))
            return false;

        return rightValue.Contains(leftValueString);
    }

    private static bool Contains(object? leftValue, IEnumerable<object?> rightValue, ExpressionContextBase context)
    {
        var rightArray = rightValue as object[] ?? rightValue.ToArray();

        var noStringTypeCoercion = context.Options.HasFlag(ExpressionOptions.NoStringTypeCoercion);

        if (rightArray.All(v => v is string))
        {
            if (noStringTypeCoercion && leftValue is not string)
            {
                return false;
            }

            return rightArray.OfType<string>().Contains(leftValue?.ToString() ?? string.Empty,
                TypeHelper.GetStringComparer(context));
        }

        return rightArray.Contains(leftValue,
            noStringTypeCoercion ? EqualityComparer<object?>.Default : StringCoercionComparer.Default);
    }

    /// <summary>
    /// Evaluates a unary expression.
    /// </summary>
    /// <param name="expression">The unary expression to evaluate.</param>
    /// <param name="result">The result of evaluating the operand of the unary expression.</param>
    /// <param name="context">The evaluation context.</param>
    /// <returns>The result of the unary operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the unary expression type is unknown.</exception>
    public static object? Unary(UnaryExpression expression, object? result, ExpressionContextBase context)
    {
        return expression.Type switch
        {
            UnaryExpressionType.Not => !Convert.ToBoolean(result, context.CultureInfo),
            UnaryExpressionType.Negate => MathHelper.Subtract(0, result, context),
            UnaryExpressionType.BitwiseNot => ~Convert.ToUInt64(result, context.CultureInfo),
            UnaryExpressionType.Positive => result,
            _ => throw new InvalidOperationException("Unknown UnaryExpressionType")
        };
    }

    /// <summary>
    /// Determines whether a specified string matches a pattern using SQL-like wildcards.
    /// </summary>
    /// <param name="value">The string to be compared against the pattern.</param>
    /// <param name="pattern">The pattern to match. '%' matches zero or more characters, and '_' matches exactly one character.</param>
    /// <param name="context">The context containing options for the comparison.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="value"/> matches the <paramref name="pattern"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The comparison is case-insensitive if the <see cref="ExpressionOptions.CaseInsensitiveStringComparer"/> flag is set in the <paramref name="context"/>.
    /// </remarks>
    public static bool Like(string value, string pattern, ExpressionContextBase context)
    {
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