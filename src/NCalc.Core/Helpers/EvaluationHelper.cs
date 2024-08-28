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
        switch (rightValue)
        {
            case IEnumerable<object?> rightValueEnumerable:
            {
                var rightArray = rightValueEnumerable as object[] ?? rightValueEnumerable.ToArray();

                if (rightArray.All(v => v is string))
                    return rightArray.OfType<string>().Contains(leftValue?.ToString() ?? string.Empty,
                        TypeHelper.GetStringComparer(context));

                return rightArray.Contains(leftValue);
            }
            case string rightValueString:
                return rightValueString.Contains(leftValue?.ToString() ?? string.Empty);
            default:
                throw new NCalcEvaluationException(
                    "'in' operator right value must implement IEnumerable or be a string.");
        }
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

    public static bool Like(string value, string pattern, ExpressionContextBase context)
    {
        // Escape special regex characters in the pattern except for % and _
        var regexPattern = Regex.Escape(pattern)
            .Replace("%", ".*")     // % matches zero or more characters
            .Replace("_", ".");     // _ matches exactly one character

        // Regex options for case-insensitivity if specified
        var options = context.Options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer)
            ? RegexOptions.IgnoreCase
            : RegexOptions.None;

        // Use ^ and $ to match the start and end of the string
        return Regex.IsMatch(value, $"^{regexPattern}$", options);
    }
}