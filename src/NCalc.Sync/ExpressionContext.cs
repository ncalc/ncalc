using NCalc.Handlers;

namespace NCalc;

public record ExpressionContext : ExpressionContextBase
{
    public IDictionary<string, ExpressionParameter> DynamicParameters { get; set; } = new Dictionary<string, ExpressionParameter>();

    public IDictionary<string, ExpressionFunction> Functions { get; set; } = new Dictionary<string, ExpressionFunction>();

    public EvaluateParameterHandler? EvaluateParameterHandler { get; set; }
    public EvaluateFunctionHandler? EvaluateFunctionHandler { get; set; }

    public ExpressionContext()
    {
    }

    public ExpressionContext(ExpressionOptions options, CultureInfo? cultureInfo)
    {
        Options = options;
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }

    public static implicit operator ExpressionContext(ExpressionOptions options) => new()
    {
        Options = options
    };

    public static implicit operator ExpressionContext(CultureInfo cultureInfo) => new()
    {
        CultureInfo = cultureInfo
    };
}