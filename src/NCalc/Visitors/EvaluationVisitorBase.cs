using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Helpers;

namespace NCalc.Visitors;

public abstract class EvaluationVisitorBase
{
    public ExpressionOptions Options
    {
        get => Context.Options;
        set => Context.Options = value;
    }

    public CultureInfo CultureInfo
    {
        get => Context.CultureInfo;
        set => Context.CultureInfo = value;
    }

    public Dictionary<string, object?> Parameters { get; set; } = new();

    public ExpressionContext Context { get; set; } = new();

    public object? Result { get; protected set; }

    public int CompareUsingMostPreciseType(object? a, object? b)
    {
        return TypeHelper.CompareUsingMostPreciseType(a,
            b, new(CultureInfo,
                Options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer),
                Options.HasFlag(ExpressionOptions.OrdinalStringComparer)));
    }
    
    protected MathHelperOptions MathHelperOptions => new(Context.CultureInfo,
        Context.Options.HasFlag(ExpressionOptions.AllowBooleanCalculation),
        Context.Options.HasFlag(ExpressionOptions.DecimalAsDefault));

    protected void ExecuteUnaryExpression(UnaryExpression expression)
    {
        switch (expression.Type)
        {
            case UnaryExpressionType.Not:
                Result = !Convert.ToBoolean(Result, CultureInfo);
                break;

            case UnaryExpressionType.Negate:
                Result = MathHelper.Subtract(0, Result, MathHelperOptions);
                break;

            case UnaryExpressionType.BitwiseNot:
                Result = ~Convert.ToUInt16(Result, CultureInfo);
                break;

            case UnaryExpressionType.Positive:
                // No-op
                break;
        }
    }
    
    protected void CheckCase(string function, string called)
    {
        bool ignoreCase = Options.HasFlag(ExpressionOptions.IgnoreCase);

        if (!ignoreCase && function != called)
            throw new NCalcFunctionNotFoundException($"Function {called} not found. Try {function} instead.", called);
    }
}