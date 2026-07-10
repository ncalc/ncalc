#if NET
using System.ComponentModel.DataAnnotations;
#endif

namespace NCalc;

/// <summary>
/// Legacy flag-based options that can be converted to <see cref="ExpressionConfiguration"/>.
/// </summary>
/// <remarks>
/// Prefer configuring new code with <see cref="ExpressionConfiguration"/>,
/// <see cref="Parser.LogicalExpressionParserOptions"/>, <see cref="ExpressionEvaluationOptions"/>,
/// and <see cref="Helpers.MathOptions"/> directly. This enum is kept as a compatibility helper for
/// older code paths and is converted by <see cref="ExpressionConfiguration.FromOptions(ExpressionOptions)"/>.
/// </remarks>
[Flags]
public enum ExpressionOptions
{
    /// <summary>
    /// No legacy options are set.
    /// </summary>
    [Display(Name = "None")]
    None = 0,

    /// <summary>
    /// Enables case-insensitive matching for built-in function names.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.IgnoreCaseAtBuiltInFunctions"/>.
    /// </remarks>
    [Display(Name = "Ignore Case At Built-In Functions")]
    IgnoreCaseAtBuiltInFunctions = 1 << 1,

    /// <summary>
    /// Disables parsed expression cache usage.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionConfiguration.CacheEnabled"/> set to <see langword="false"/>.
    /// </remarks>
    [Display(Name = "No Cache")]
    NoCache = 1 << 2,

    /// <summary>
    /// Treats enumerable parameters as parallel value sequences and returns one result per item.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.IterateParameters"/>.
    /// </remarks>
    [Display(Name = "Iterate Parameters")]
    IterateParameters = 1 << 3,

    /// <summary>
    /// Uses <see cref="MidpointRounding.AwayFromZero"/> for the built-in Round function.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="Helpers.MathOptions.RoundAwayFromZero"/>.
    /// </remarks>
    [Display(Name = "Round Away From Zero")]
    RoundAwayFromZero = 1 << 4,

    /// <summary>
    /// Uses a case-insensitive string comparer for string comparisons.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.StringComparer"/> as either
    /// <see cref="StringComparer.CurrentCultureIgnoreCase"/> or <see cref="StringComparer.OrdinalIgnoreCase"/>,
    /// depending on <see cref="OrdinalStringComparer"/>.
    /// </remarks>
    [Display(Name = "Case-Insensitive String Comparer")]
    CaseInsensitiveStringComparer = 1 << 5,

    /// <summary>
    /// Uses <see cref="decimal"/> as the default parsed and coerced number type.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="Parser.DefaultNumberType.Decimal"/> on both parser and math options.
    /// </remarks>
    [Display(Name = "Decimal As Default")]
    DecimalAsDefault = 1 << 6,

    /// <summary>
    /// Allows the identifier <c>null</c> to resolve to a null value during evaluation.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.AllowNullParameter"/>.
    /// </remarks>
    [Display(Name = "Allow Null Parameter")]
    AllowNullParameter = 1 << 7,

    /// <summary>
    /// Uses ordinal string comparison instead of current-culture comparison.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.StringComparer"/> as either
    /// <see cref="StringComparer.Ordinal"/> or <see cref="StringComparer.OrdinalIgnoreCase"/>,
    /// depending on <see cref="CaseInsensitiveStringComparer"/>.
    /// </remarks>
    [Display(Name = "Ordinal String Comparer")]
    OrdinalStringComparer = 1 << 8,

    /// <summary>
    /// Allows arithmetic operations with <see cref="bool"/> values.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="Helpers.MathOptions.AllowBooleanCalculation"/>.
    /// </remarks>
    [Display(Name = "Allow Boolean Calculation")]
    AllowBooleanCalculation = 1 << 9,

    /// <summary>
    /// Checks arithmetic binary operations for overflow.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="Helpers.MathOptions.OverflowProtection"/>.
    /// </remarks>
    [Display(Name = "Overflow Protection")]
    OverflowProtection = 1 << 10,

    /// <summary>
    /// Concatenates values as strings instead of attempting arithmetic addition.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.StringConcat"/>.
    /// </remarks>
    [Display(Name = "String Concatenation")]
    StringConcat = 1 << 11,

    /// <summary>
    /// Parses single-quoted one-character values as <see cref="char"/>.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="Parser.LogicalExpressionParserOptions.AllowCharValues"/>.
    /// </remarks>
    [Display(Name = "Allow Char Values")]
    AllowCharValues = 1 << 12,

    /// <summary>
    /// Disables coercion of string values to other types during evaluation.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.NoStringTypeCoercion"/>.
    /// </remarks>
    [Display(Name = "No String Type Coercion")]
    NoStringTypeCoercion = 1 << 13,

    /// <summary>
    /// Allows null or empty expression text to evaluate without throwing.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.AllowNullOrEmptyExpressions"/>.
    /// </remarks>
    [Display(Name = "Allow Null Or Empty Expressions")]
    AllowNullOrEmptyExpressions = 1 << 14,

    /// <summary>
    /// Makes comparisons between values of different runtime types return false.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.StrictTypeMatching"/>.
    /// </remarks>
    [Display(Name = "Strict Type Matching")]
    StrictTypeMatching = 1 << 15,

    /// <summary>
    /// Uses <see cref="long"/> as the default parsed and coerced integral number type.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="Parser.DefaultNumberType.Int64"/> on both parser and math options,
    /// unless <see cref="DecimalAsDefault"/> is also set.
    /// </remarks>
    [Display(Name = "Long As Default")]
    LongAsDefault = 1 << 16,

    /// <summary>
    /// Converts null or empty string values to zero during arithmetic operations.
    /// </summary>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.ArithmeticNullOrEmptyStringAsZero"/>.
    /// </remarks>
    [Display(Name = "Arithmetic Null Or Empty String As Zero")]
    ArithmeticNullOrEmptyStringAsZero = 1 << 17
}

#if NETFRAMEWORK || NETSTANDARD2_0
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal sealed class DisplayAttribute : Attribute
{
    public string? Name { get; set; }
}
#endif
