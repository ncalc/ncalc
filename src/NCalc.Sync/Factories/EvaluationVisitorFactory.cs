using NCalc.Visitors;

namespace NCalc.Factories;

public sealed class EvaluationVisitorFactory : IEvaluationVisitorFactory
{
    public EvaluationVisitor Create(ExpressionContext context)
    {
        return new EvaluationVisitor(context);
    }
}