using NCalc.Domain;
using NCalc.Visitors;

namespace NCalc.Services;

/// <inheritdoc cref="IEvaluationService"/>
public class EvaluationService : IEvaluationService
{
    public object? Evaluate(LogicalExpression? expression, ExpressionContext context)
    {
        var visitor = new EvaluationVisitor(context);
        return expression?.Accept(visitor);
    }
}