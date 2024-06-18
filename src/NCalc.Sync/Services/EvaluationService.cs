using NCalc.Domain;
using NCalc.Handlers;
using NCalc.Visitors;

namespace NCalc.Services;

/// <inheritdoc cref="IEvaluationService"/>
public class EvaluationService : IEvaluationService
{
    public object? Evaluate(LogicalExpression expression, ExpressionContext context)
    {
        var visitor = new EvaluationVisitor(context);
        expression.Accept(visitor);
        return visitor.Result;
    }
}