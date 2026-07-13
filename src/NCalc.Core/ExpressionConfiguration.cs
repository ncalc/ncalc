using NCalc.Helpers;

namespace NCalc;

/// <summary>
/// Parsing and evaluation configuration for an <see cref="Expression"/>.
/// </summary>
public sealed class ExpressionConfiguration
{
    public LogicalExpressionParserOptions Parsing { get; init; }

    public ExpressionEvaluationOptions Evaluation { get; init; }

    /// <summary>
    /// Enables parsed expression cache usage.
    /// </summary>
    public bool CacheEnabled { get; init; } = true;

    public ExpressionConfiguration()
    {
        Parsing = new LogicalExpressionParserOptions();
        Evaluation = new ExpressionEvaluationOptions();
    }

    public ExpressionConfiguration(LogicalExpressionParserOptions parserOptions, ExpressionEvaluationOptions evaluationOptions, bool cacheEnabled = true)
    {
        Parsing = parserOptions;
        Evaluation = evaluationOptions;
        CacheEnabled = cacheEnabled;
    }

    public static ExpressionConfiguration FromOptions(ExpressionOptions options)
    {
        return new ExpressionConfiguration
        {
            CacheEnabled = !options.HasFlag(ExpressionOptions.NoCache),
            Parsing = new LogicalExpressionParserOptions
            {
                AllowCharValues = options.HasFlag(ExpressionOptions.AllowCharValues),
                FloatingPointNumberType = GetFloatingPointNumberType(options),
                IntegerNumberType = GetIntegerNumberType(options)
            },
            Evaluation = new ExpressionEvaluationOptions
            {
                IgnoreCaseAtBuiltInFunctions = options.HasFlag(ExpressionOptions.IgnoreCaseAtBuiltInFunctions),
                IterateParameters = options.HasFlag(ExpressionOptions.IterateParameters),
                AllowNullParameter = options.HasFlag(ExpressionOptions.AllowNullParameter),
                StringConcat = options.HasFlag(ExpressionOptions.StringConcat),
                NoStringTypeCoercion = options.HasFlag(ExpressionOptions.NoStringTypeCoercion),
                AllowNullOrEmptyExpressions = options.HasFlag(ExpressionOptions.AllowNullOrEmptyExpressions),
                ArithmeticNullOrEmptyStringAsZero =
                    options.HasFlag(ExpressionOptions.ArithmeticNullOrEmptyStringAsZero),

                Math = new MathOptions
                {
                    FloatingPointNumberType = GetFloatingPointNumberType(options),
                    IntegerNumberType = GetIntegerNumberType(options),
                    AllowBooleanCalculation = options.HasFlag(ExpressionOptions.AllowBooleanCalculation),
                    OverflowProtection = options.HasFlag(ExpressionOptions.OverflowProtection),
                    RoundAwayFromZero = options.HasFlag(ExpressionOptions.RoundAwayFromZero)
                },

                StrictTypeMatching = options.HasFlag(ExpressionOptions.StrictTypeMatching),
                StringComparer = GetStringComparer(options)
            }
        };
    }

    private static FloatingPointNumberType GetFloatingPointNumberType(ExpressionOptions options)
    {
        if (options.HasFlag(ExpressionOptions.DecimalAsDefault))
            return FloatingPointNumberType.Decimal;

        return FloatingPointNumberType.Double;
    }

    private static IntegerNumberType GetIntegerNumberType(ExpressionOptions options)
    {
        return options.HasFlag(ExpressionOptions.LongAsDefault)
            ? IntegerNumberType.Int64
            : IntegerNumberType.Int32;
    }

    private static StringComparer GetStringComparer(ExpressionOptions options)
    {
        return options.HasFlag(ExpressionOptions.OrdinalStringComparer) switch
        {
            true when options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer) => StringComparer.OrdinalIgnoreCase,
            true => StringComparer.Ordinal,
            false when options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer) => StringComparer.CurrentCultureIgnoreCase,
            _ => StringComparer.CurrentCulture
        };
    }

    public static implicit operator ExpressionConfiguration(ExpressionOptions options) => FromOptions(options);
}
