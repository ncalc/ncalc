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
    None = 0,

    /// <summary>
    /// Specifies case-insensitive matching for built-in functions.
    /// </summary>
    IgnoreCaseAtBuiltInFunctions = 1 << 1,

    /// <summary>
    /// No-cache mode. Ignores any pre-compiled expression in the cache.
    /// </summary>
    NoCache = 1 << 2,

    /// <summary>
    /// Treats parameters as arrays and returns a set of results.
    /// </summary>
    IterateParameters = 1 << 3,

    /// <summary>
    /// When using Round(), if a number is halfway between two others, it is rounded toward the nearest number that is away from zero.
    /// </summary>
    RoundAwayFromZero = 1 << 4,

    /// <summary>
    /// Specifies the use of CaseInsensitiveComparer for comparisons.
    /// </summary>
    CaseInsensitiveStringComparer = 1 << 5,

    /// <summary>
    /// Uses decimals instead of doubles as default floating point data type.
    /// </summary>
    DecimalAsDefault = 1 << 6,

    /// <summary>
    /// Defines a "null" parameter and allows comparison of values to null.
    /// </summary>
    AllowNullParameter = 1 << 7,

    /// <summary>
    /// Use ordinal culture on string compare.
    /// </summary>
    OrdinalStringComparer = 1 << 8,

    /// <summary>
    /// Allow calculation with <see cref="bool"/> values.
    /// </summary>
    AllowBooleanCalculation = 1 << 9,

    /// <summary>
    /// Check for arithmetic binary operation overflow.
    /// </summary>
    OverflowProtection = 1 << 10,

    /// <summary>
    /// Concat values as strings instead of arithmetic addition.
    /// </summary>
    StringConcat = 1 << 11,

    /// <summary>
    /// Parse single quoted strings as <see cref="char"/>.
    /// </summary>
    AllowCharValues = 1 << 12,

    /// <summary>
    /// Disables coercion of string values to other types.
    /// </summary>
    NoStringTypeCoercion = 1 << 13,

    /// <summary>
    /// Return the value instead of throwing an exception when the expression is null or empty.
    /// </summary>
    AllowNullOrEmptyExpressions = 1 << 14,

    /// <summary>
    /// Enables strict type matching, where comparisons between objects of different types will return false.
    /// </summary>
    StrictTypeMatching = 1 << 15,

    /// <summary>
    /// Uses long instead of integer as default integral data type.
    /// </summary>
    LongAsDefault = 1 << 16,

    /// <summary>
    /// Converts null or empty string values to 0 during arithmetic operations.
    /// </summary>
    ArithmeticNullOrEmptyStringAsZero = 1 << 17
}
