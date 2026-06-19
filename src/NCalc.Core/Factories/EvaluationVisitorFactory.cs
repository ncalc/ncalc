using NCalc.Visitors;

namespace NCalc.Factories;

public sealed class EvaluationVisitorFactory : IEvaluationVisitorFactory
{
    public EvaluationVisitor Create(ExpressionContext context)
    {
        return new EvaluationVisitor(context);
    }

    public AsyncEvaluationVisitor CreateAsync(ExpressionContext context)
    {
        return new AsyncEvaluationVisitor(context);
    }
}
