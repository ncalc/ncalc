using NCalc.Helpers;

namespace NCalc;

/// <summary>
/// Evaluation configuration for an <see cref="Expression"/>.
/// </summary>
public sealed class ExpressionEvaluationOptions
{
    public bool IgnoreCaseAtBuiltInFunctions { get; init; }
    public bool IterateParameters { get; init; }
    public bool AllowNullParameter { get; init; }
    public bool StringConcat { get; init; }
    public bool NoStringTypeCoercion { get; init; }
    public bool AllowNullOrEmptyExpressions { get; init; }
    public bool ArithmeticNullOrEmptyStringAsZero { get; init; }
    public bool StrictTypeMatching { get; init; }
    public MathOptions Math { get; init; } = new();
    public StringComparer StringComparer { get; init; } = StringComparer.CurrentCulture;
}
