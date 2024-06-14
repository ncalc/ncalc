using NCalc.Cache;
using NCalc.Domain;
using NCalc.Factories;
using NCalc.Visitors;

namespace NCalc;

/// <summary>
/// An <see cref="Expression"/> object with dependency injection friendly constructors.
/// </summary>
public class AdvancedExpression : Expression
{
    public AdvancedExpression(
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache,
        IEvaluationVisitor evaluationVisitor,
        IParameterExtractionVisitor parameterExtractionVisitor,
        string expression,
        ExpressionContext? context = null) : base(logicalExpressionFactory, logicalExpressionCache, evaluationVisitor, parameterExtractionVisitor)
    {
        Context = context ?? new();
        EvaluationVisitor.Context = Context;
        ExpressionString = expression;
    }

    public AdvancedExpression(
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache,
        IEvaluationVisitor evaluationVisitor,
        IParameterExtractionVisitor parameterExtractionVisitor,
        LogicalExpression logicalExpression,
        ExpressionContext? context = null) : base(logicalExpressionFactory, logicalExpressionCache, evaluationVisitor, parameterExtractionVisitor)
    {
        Context = context ?? new();
        EvaluationVisitor.Context = Context;
        LogicalExpression = logicalExpression;
    }
}