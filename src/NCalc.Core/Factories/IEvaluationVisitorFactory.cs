using NCalc.Visitors;

namespace NCalc.Factories;

public interface IEvaluationVisitorFactory
{
    public EvaluationVisitor Create(ExpressionContext context);

    public AsyncEvaluationVisitor CreateAsync(ExpressionContext context);
}
