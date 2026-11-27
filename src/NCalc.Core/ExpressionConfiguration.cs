using NCalc.Helpers;

namespace NCalc;

/// <summary>
/// Parsing and evaluation configuration for an <see cref="Expression"/>.
/// </summary>
public sealed class ExpressionConfiguration
{
    /// <summary>
    /// The configuration an <see cref="Expression"/> uses when none is given. The configuration and the option
    /// objects it holds are immutable, so one instance is shared by every such expression.
    /// </summary>
    public static ExpressionConfiguration Default { get; } = new();

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
        if (options == ExpressionOptions.None)
            return Default;

        // The option objects are passed to the constructor; an object initializer on the default constructor would
        // build a second set of them only to replace it.
        return new ExpressionConfiguration(
            new LogicalExpressionParserOptions
            {
                AllowCharValues = options.HasFlag(ExpressionOptions.AllowCharValues),
                DisallowSingleEquals = options.HasFlag(ExpressionOptions.DisallowSingleEquals),
                FloatingPointNumberType = GetFloatingPointNumberType(options),
                IntegerNumberType = GetIntegerNumberType(options)
            },
            new ExpressionEvaluationOptions
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
                    MidpointRounding = options.HasFlag(ExpressionOptions.RoundAwayFromZero) ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven
                },

                StrictTypeMatching = options.HasFlag(ExpressionOptions.StrictTypeMatching),
                StringComparer = GetStringComparer(options),

                ConcurrentAsyncEvaluation = options.HasFlag(ExpressionOptions.ConcurrentAsyncEvaluation)
            },
            cacheEnabled: !options.HasFlag(ExpressionOptions.NoCache));
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
