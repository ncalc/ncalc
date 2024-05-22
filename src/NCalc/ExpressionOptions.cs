namespace NCalc;

[Flags]
public enum ExpressionOptions
{
    // Summary:
    //     Specifies that no options are set.
    None = 1 << 0,

    //
    // Summary:
    //     Specifies case-insensitive matching.
    IgnoreCase = 1 << 1,

    //
    // Summary:
    //     No-cache mode. Ingores any pre-compiled expression in the cache.
    NoCache = 1 << 2,

    //
    // Summary:
    //     Treats parameters as arrays and result a set of results.
    IterateParameters = 1 << 3,

    //
    // Summary:
    //     When using Round(), if a number is halfway between two others, it is rounded toward the nearest number that is away from zero. 
    RoundAwayFromZero = 1 << 4,

    //
    // Summary:
    //     Specifies the use of CaseInsensitiveComparer for comparasions.
    CaseInsensitiveComparer = 1 << 5,

    //
    // Summary:
    //     Uses decimals instead of doubles as default floating point data type
    DecimalAsDefault = 1 << 6,

    /// <summary>
    /// Defines a "null" parameter and allows comparison of values to null.
    /// </summary>
    AllowNullParameter = 1 << 7
}

// Summary:
//     Provides enumerated values to use to set evaluation options.
//
public static class ExpressionOptionsExtensions
{
    public static bool HasOption(this ExpressionOptions options, ExpressionOptions option)
    {
        return (options & option) == option;
    }
}