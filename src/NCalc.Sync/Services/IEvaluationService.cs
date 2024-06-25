using NCalc.Domain;
using NCalc.Handlers;

namespace NCalc.Services;

/// <summary>
/// Service used to evaluate the result of a <see cref="LogicalExpression"/>
/// </summary>
public interface IEvaluationService
{
    public event EvaluateFunctionHandler? EvaluateFunction;
    public event EvaluateParameterHandler? EvaluateParameter;
    object? Evaluate(LogicalExpression expression, ExpressionContext context);
}