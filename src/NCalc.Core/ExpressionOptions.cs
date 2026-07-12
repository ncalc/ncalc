#if NET
using System.ComponentModel.DataAnnotations;
#endif

namespace NCalc;

/// <summary>
/// Flag-based options that can be converted to <see cref="ExpressionConfiguration"/>.
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
    /// No options are set.
    /// </summary>
    [Display(Name = "None")]
    None = 0,

    /// <inheritdoc cref="ExpressionEvaluationOptions.IgnoreCaseAtBuiltInFunctions"/>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.IgnoreCaseAtBuiltInFunctions"/>.
    /// </remarks>
    [Display(Name = "Ignore Case At Built-In Functions")]
    IgnoreCaseAtBuiltInFunctions = 1 << 1,

    /// <inheritdoc cref="ExpressionConfiguration.CacheEnabled"/>
    /// <remarks>
    /// Converts to <see cref="ExpressionConfiguration.CacheEnabled"/> set to <see langword="false"/>,
    /// disabling parsed expression cache usage.
    /// </remarks>
    [Display(Name = "No Cache")]
    NoCache = 1 << 2,

    /// <inheritdoc cref="ExpressionEvaluationOptions.IterateParameters"/>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.IterateParameters"/>.
    /// </remarks>
    [Display(Name = "Iterate Parameters")]
    IterateParameters = 1 << 3,

    /// <inheritdoc cref="Helpers.MathOptions.RoundAwayFromZero"/>
    /// <remarks>
    /// Converts to <see cref="Helpers.MathOptions.RoundAwayFromZero"/>.
    /// </remarks>
    [Display(Name = "Round Away From Zero")]
    RoundAwayFromZero = 1 << 4,

    /// <inheritdoc cref="ExpressionEvaluationOptions.StringComparer"/>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.StringComparer"/> as either
    /// <see cref="StringComparer.CurrentCultureIgnoreCase"/> or <see cref="StringComparer.OrdinalIgnoreCase"/>,
    /// using a case-insensitive string comparer for string comparisons,
    /// depending on <see cref="OrdinalStringComparer"/>.
    /// </remarks>
    [Display(Name = "Case-Insensitive String Comparer")]
    CaseInsensitiveStringComparer = 1 << 5,

    /// <inheritdoc cref="Parser.DefaultNumberType.Decimal"/>
    /// <remarks>
    /// Converts to <see cref="Parser.DefaultNumberType.Decimal"/> on both parser and math options.
    /// </remarks>
    [Display(Name = "Decimal As Default")]
    DecimalAsDefault = 1 << 6,

    /// <inheritdoc cref="ExpressionEvaluationOptions.AllowNullParameter"/>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.AllowNullParameter"/>.
    /// </remarks>
    [Display(Name = "Allow Null Parameter")]
    AllowNullParameter = 1 << 7,

    /// <inheritdoc cref="ExpressionEvaluationOptions.StringComparer"/>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.StringComparer"/> as either
    /// <see cref="StringComparer.Ordinal"/> or <see cref="StringComparer.OrdinalIgnoreCase"/>,
    /// using ordinal string comparison instead of current-culture comparison,
    /// depending on <see cref="CaseInsensitiveStringComparer"/>.
    /// </remarks>
    [Display(Name = "Ordinal String Comparer")]
    OrdinalStringComparer = 1 << 8,

    /// <inheritdoc cref="Helpers.MathOptions.AllowBooleanCalculation"/>
    /// <remarks>
    /// Converts to <see cref="Helpers.MathOptions.AllowBooleanCalculation"/>.
    /// </remarks>
    [Display(Name = "Allow Boolean Calculation")]
    AllowBooleanCalculation = 1 << 9,

    /// <inheritdoc cref="Helpers.MathOptions.OverflowProtection"/>
    /// <remarks>
    /// Converts to <see cref="Helpers.MathOptions.OverflowProtection"/>.
    /// </remarks>
    [Display(Name = "Overflow Protection")]
    OverflowProtection = 1 << 10,

    /// <inheritdoc cref="ExpressionEvaluationOptions.StringConcat"/>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.StringConcat"/>.
    /// </remarks>
    [Display(Name = "String Concatenation")]
    StringConcat = 1 << 11,

    /// <inheritdoc cref="Parser.LogicalExpressionParserOptions.AllowCharValues"/>
    /// <remarks>
    /// Converts to <see cref="Parser.LogicalExpressionParserOptions.AllowCharValues"/>.
    /// </remarks>
    [Display(Name = "Allow Char Values")]
    AllowCharValues = 1 << 12,

    /// <inheritdoc cref="ExpressionEvaluationOptions.NoStringTypeCoercion"/>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.NoStringTypeCoercion"/>.
    /// </remarks>
    [Display(Name = "No String Type Coercion")]
    NoStringTypeCoercion = 1 << 13,

    /// <inheritdoc cref="ExpressionEvaluationOptions.AllowNullOrEmptyExpressions"/>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.AllowNullOrEmptyExpressions"/>.
    /// </remarks>
    [Display(Name = "Allow Null Or Empty Expressions")]
    AllowNullOrEmptyExpressions = 1 << 14,

    /// <inheritdoc cref="ExpressionEvaluationOptions.StrictTypeMatching"/>
    /// <remarks>
    /// Converts to <see cref="ExpressionEvaluationOptions.StrictTypeMatching"/>.
    /// </remarks>
    [Display(Name = "Strict Type Matching")]
    StrictTypeMatching = 1 << 15,

    /// <inheritdoc cref="Parser.DefaultNumberType.Int64"/>
    /// <remarks>
    /// Converts to <see cref="Parser.DefaultNumberType.Int64"/> on both parser and math options,
    /// unless <see cref="DecimalAsDefault"/> is also set.
    /// </remarks>
    [Display(Name = "Long As Default")]
    LongAsDefault = 1 << 16,

    /// <inheritdoc cref="ExpressionEvaluationOptions.ArithmeticNullOrEmptyStringAsZero"/>
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
