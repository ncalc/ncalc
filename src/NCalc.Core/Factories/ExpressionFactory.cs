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
    public Expression Create(
        string expression,
        ExpressionConfiguration? configuration = null,
        ExpressionContext? context = null,
        CultureInfo? cultureInfo = null)
    {
        return new Expression(expression, configuration ?? new(), context ?? new(),
            cultureInfo ?? CultureInfo.CurrentCulture, logicalExpressionFactory, cache, evaluationVisitorFactory);
    }

    public Expression Create(
        LogicalExpression logicalExpression,
        ExpressionConfiguration? configuration = null,
        ExpressionContext? context = null,
        CultureInfo? cultureInfo = null)
    {
        return new Expression(logicalExpression, configuration ?? new(), context ?? new(),
            cultureInfo ?? CultureInfo.CurrentCulture, logicalExpressionFactory, cache, evaluationVisitorFactory);
    }
}