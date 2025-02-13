using NCalc.Visitors;

namespace NCalc.Factories;

internal sealed class EvaluationVisitorFactory : IEvaluationVisitorFactory
{
    public EvaluationVisitor Create(ExpressionContext context)
    {
        return new EvaluationVisitor(context);
    }
}