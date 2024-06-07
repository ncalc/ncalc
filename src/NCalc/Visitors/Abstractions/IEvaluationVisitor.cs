using NCalc.Handlers;

namespace NCalc.Visitors;

public interface IEvaluationVisitor : ILogicalExpressionVisitor
{
    public event EvaluateFunctionHandler? EvaluateFunction;
    public event EvaluateParameterHandler? EvaluateParameter;

    ExpressionContext Context { get; set; }

    public object? Result { get; }
}