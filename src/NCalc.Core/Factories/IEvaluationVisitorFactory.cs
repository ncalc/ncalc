using NCalc.Visitors;

namespace NCalc.Factories;

public interface IEvaluationVisitorFactory
{
    EvaluationVisitor CreateEvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default);
    AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default);
}
