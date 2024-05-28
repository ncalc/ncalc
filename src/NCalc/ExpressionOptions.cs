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
    None = 1 << 0,

    /// <summary>
    /// Specifies case-insensitive matching.
    /// </summary>
    IgnoreCase = 1 << 1,

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
    [Obsolete("Please use CaseInsensitiveStringComparer")]
    CaseInsensitiveComparer = 1 << 5,

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
    /// When using Abs(), return a double instead of a decimal.
    /// </summary>
    UseDoubleForAbsFunction = 1 << 8,
    
    /// <summary>
    /// Use ordinal culture on string compare
    /// </summary>
    OrdinalStringComparer = 1 << 9,
}
    
public static class ExpressionOptionsExtensions
{
    /// <summary>
    /// Checks if the ExpressionOptions enum have an option selected.
    /// </summary>
    public static bool HasOption(this ExpressionOptions options, ExpressionOptions option)
    {
        return (options & option) == option;
    }
}