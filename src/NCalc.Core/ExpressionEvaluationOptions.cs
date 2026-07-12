using NCalc.Helpers;

namespace NCalc;

/// <summary>
/// Evaluation configuration for an <see cref="Expression"/>.
/// </summary>
public sealed class ExpressionEvaluationOptions
{
    /// <summary>
    /// Enables case-insensitive matching for built-in function names.
    /// </summary>
    public bool IgnoreCaseAtBuiltInFunctions { get; init; }

    /// <summary>
    /// Treats enumerable parameters as parallel value sequences and returns one result per item.
    /// </summary>
    public bool IterateParameters { get; init; }

    /// <summary>
    /// Allows the identifier <c>null</c> to resolve to a null value during evaluation.
    /// </summary>
    public bool AllowNullParameter { get; init; }

    /// <summary>
    /// Concatenates values as strings instead of attempting arithmetic addition.
    /// </summary>
    public bool StringConcat { get; init; }

    /// <summary>
    /// Disables coercion of string values to other types during evaluation.
    /// </summary>
    public bool NoStringTypeCoercion { get; init; }

    /// <summary>
    /// Allows null or empty expression text to evaluate without throwing.
    /// </summary>
    public bool AllowNullOrEmptyExpressions { get; init; }

    /// <summary>
    /// Converts null or empty string values to zero during arithmetic operations.
    /// </summary>
    public bool ArithmeticNullOrEmptyStringAsZero { get; init; }

    /// <summary>
    /// Makes comparisons between values of different runtime types return false.
    /// </summary>
    public bool StrictTypeMatching { get; init; }

    /// <summary>
    /// Gets math evaluation options.
    /// </summary>
    public MathOptions Math { get; init; } = new();

    /// <summary>
    /// Gets the string comparer used for string comparisons.
    /// </summary>
    public StringComparer StringComparer { get; init; } = StringComparer.CurrentCulture;
}
