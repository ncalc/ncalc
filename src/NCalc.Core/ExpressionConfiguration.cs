using System.Diagnostics.CodeAnalysis;
using NCalc.Helpers;
using NCalc.Parser;

namespace NCalc;

/// <summary>
/// Parsing and evaluation configuration for an <see cref="Expression"/>.
/// </summary>
public sealed class ExpressionConfiguration
{
    public LogicalExpressionParserOptions Parsing { get; init; }

    public ExpressionEvaluationOptions Evaluation { get; init; }

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
                DefaultNumberType = GetDefaultNumberType(options)
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
                    DefaultNumberType = GetDefaultNumberType(options),
                    AllowBooleanCalculation = options.HasFlag(ExpressionOptions.AllowBooleanCalculation),
                    OverflowProtection = options.HasFlag(ExpressionOptions.OverflowProtection),
                    RoundAwayFromZero = options.HasFlag(ExpressionOptions.RoundAwayFromZero)
                },

                StrictTypeMatching = options.HasFlag(ExpressionOptions.StrictTypeMatching),
                StringComparer = GetStringComparer(options)
            }
        };
    }

    private static DefaultNumberType GetDefaultNumberType(ExpressionOptions options)
    {
        if (options.HasFlag(ExpressionOptions.DecimalAsDefault))
            return DefaultNumberType.Decimal;

        return options.HasFlag(ExpressionOptions.LongAsDefault)
            ? DefaultNumberType.Int64
            : DefaultNumberType.Double;
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
