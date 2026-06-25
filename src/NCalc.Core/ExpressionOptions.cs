#if NET
using System.ComponentModel.DataAnnotations;
#endif

namespace NCalc;

/// <summary>
/// Options used for both parsing and evaluation of an expression.
/// </summary>
[Flags]
public enum ExpressionOptions
{
    /// <summary>
    /// Specifies that no options are set.
    /// </summary>
    [Display(Name = "None")]
    None = 0,

    /// <summary>
    /// Specifies case-insensitive matching for built-in functions.
    /// </summary>
    [Display(Name = "Ignore Case At Built-In Functions")]
    IgnoreCaseAtBuiltInFunctions = 1 << 1,

    /// <summary>
    /// No-cache mode. Ignores any pre-compiled expression in the cache.
    /// </summary>
    [Display(Name = "No Cache")]
    NoCache = 1 << 2,

    /// <summary>
    /// Treats parameters as arrays and returns a set of results.
    /// </summary>
    [Display(Name = "Iterate Parameters")]
    IterateParameters = 1 << 3,

    /// <summary>
    /// When using Round(), if a number is halfway between two others, it is rounded toward the nearest number that is away from zero.
    /// </summary>
    [Display(Name = "Round Away From Zero")]
    RoundAwayFromZero = 1 << 4,

    /// <summary>
    /// Specifies the use of CaseInsensitiveComparer for comparisons.
    /// </summary>
    [Display(Name = "Case-Insensitive String Comparer")]
    CaseInsensitiveStringComparer = 1 << 5,

    /// <summary>
    /// Uses decimals instead of doubles as default floating point data type.
    /// </summary>
    [Display(Name = "Decimal As Default")]
    DecimalAsDefault = 1 << 6,

    /// <summary>
    /// Defines a "null" parameter and allows comparison of values to null.
    /// </summary>
    [Display(Name = "Allow Null Parameter")]
    AllowNullParameter = 1 << 7,

    /// <summary>
    /// Use ordinal culture on string compare.
    /// </summary>
    [Display(Name = "Ordinal String Comparer")]
    OrdinalStringComparer = 1 << 8,

    /// <summary>
    /// Allow calculation with <see cref="bool"/> values.
    /// </summary>
    [Display(Name = "Allow Boolean Calculation")]
    AllowBooleanCalculation = 1 << 9,

    /// <summary>
    /// Check for arithmetic binary operation overflow.
    /// </summary>
    [Display(Name = "Overflow Protection")]
    OverflowProtection = 1 << 10,

    /// <summary>
    /// Concat values as strings instead of arithmetic addition.
    /// </summary>
    [Display(Name = "String Concatenation")]
    StringConcat = 1 << 11,

    /// <summary>
    /// Parse single quoted strings as <see cref="char"/>.
    /// </summary>
    [Display(Name = "Allow Char Values")]
    AllowCharValues = 1 << 12,

    /// <summary>
    /// Disables coercion of string values to other types.
    /// </summary>
    [Display(Name = "No String Type Coercion")]
    NoStringTypeCoercion = 1 << 13,

    /// <summary>
    /// Return the value instead of throwing an exception when the expression is null or empty.
    /// </summary>
    [Display(Name = "Allow Null Or Empty Expressions")]
    AllowNullOrEmptyExpressions = 1 << 14,

    /// <summary>
    /// Enables strict type matching, where comparisons between objects of different types will return false.
    /// </summary>
    [Display(Name = "Strict Type Matching")]
    StrictTypeMatching = 1 << 15,

    /// <summary>
    /// Uses long instead of integer as default integral data type.
    /// </summary>
    [Display(Name = "Long As Default")]
    LongAsDefault = 1 << 16,

    /// <summary>
    /// Converts null or empty string values to 0 during arithmetic operations.
    /// </summary>
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