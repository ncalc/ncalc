using NCalc.Cache;

namespace NCalc.Factories;

/// <summary>
/// Default <see cref="IExpressionFactory"/> implementation.
/// </summary>
public sealed class ExpressionFactory(
    ILogicalExpressionFactory logicalExpressionFactory,
    ILogicalExpressionCache cache,
    IEvaluationVisitorFactory? evaluationVisitorFactory = null
) : IExpressionFactory
{
    public Expression Create(string expression, ExpressionConfiguration? configuration = null)
    {
        return new Expression(expression, configuration, logicalExpressionFactory, cache, evaluationVisitorFactory);
    }

    public Expression Create(LogicalExpression logicalExpression, ExpressionConfiguration? configuration = null)
    {
        return new Expression(logicalExpression, configuration, logicalExpressionFactory, cache, evaluationVisitorFactory);
    }
}
