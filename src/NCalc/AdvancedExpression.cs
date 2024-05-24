using NCalc.Cache;
using NCalc.Domain;
using NCalc.Factories;
using NCalc.Visitors;

namespace NCalc;

/// <summary>
/// An Expression object with dependency injection friendly constructors.
/// </summary>
public sealed class AdvancedExpression : Expression
{
    protected override ILogicalExpressionCache LogicalExpressionCache { get; }
    protected override ILogicalExpressionFactory LogicalExpressionFactory { get; }
    protected override IEvaluationVisitor EvaluationVisitor { get; }
    protected override IParameterExtractionVisitor ParameterExtractionVisitor { get; }
    
    public AdvancedExpression(
        ILogicalExpressionFactory logicalExpressionFactory, 
        ILogicalExpressionCache logicalExpressionCache,
        IEvaluationVisitor evaluationVisitor, 
        IParameterExtractionVisitor parameterExtractionVisitor,
        string expression, 
        ExpressionContext? context = null)
    {
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
        EvaluationVisitor = evaluationVisitor;
        ParameterExtractionVisitor = parameterExtractionVisitor;
        Options = context?.Options ?? ExpressionOptions.None;
        CultureInfo = context?.CultureInfo ?? CultureInfo.CurrentCulture;
        ExpressionString = expression;
    }

    public AdvancedExpression(
        ILogicalExpressionFactory logicalExpressionFactory, 
        ILogicalExpressionCache logicalExpressionCache, 
        IEvaluationVisitor evaluationVisitor,
        IParameterExtractionVisitor parameterExtractionVisitor, 
        LogicalExpression logicalExpression,
        ExpressionContext? context = null)
    {
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
        EvaluationVisitor = evaluationVisitor;
        ParameterExtractionVisitor = parameterExtractionVisitor;
        LogicalExpression = logicalExpression;
        Options = context?.Options ?? ExpressionOptions.None;
        CultureInfo = context?.CultureInfo ?? CultureInfo.CurrentCulture;
    }
}