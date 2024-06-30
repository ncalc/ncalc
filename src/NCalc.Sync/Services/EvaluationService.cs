using NCalc.Domain;
using NCalc.Handlers;
using NCalc.Visitors;

namespace NCalc.Services;

/// <inheritdoc cref="IEvaluationService"/>
public class EvaluationService : IEvaluationService
{
    public event EvaluateFunctionHandler? EvaluateFunction;
    public event EvaluateParameterHandler? EvaluateParameter;
    public object? Evaluate(LogicalExpression expression, ExpressionContext context)
    {
        var visitor = new EvaluationVisitor(context);
        
        visitor.EvaluateFunction += EvaluateFunction;
        visitor.EvaluateParameter += EvaluateParameter;
        
        return expression.Accept(visitor);
    }
}