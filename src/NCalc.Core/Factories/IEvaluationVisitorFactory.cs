using NCalc.Visitors;

namespace NCalc.Factories;

public interface IEvaluationVisitorFactory
{
    public EvaluationVisitor Create(ExpressionContext context);
}