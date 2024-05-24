using NCalc.Cache;
using NCalc.Domain;
using NCalc.Factories;
using NCalc.Visitors;

namespace NCalc;

/// <summary>
/// An Expression object with dependency injection friendly constructors.
/// </summary>
internal sealed class AdvancedExpression : Expression
{
    public AdvancedExpression(
        ILogicalExpressionFactory logicalExpressionFactory, 
        ILogicalExpressionCache logicalExpressionCache,
        IEvaluationVisitor evaluationVisitor, 
        IParameterExtractionVisitor parameterExtractionVisitor,
        string expression, 
        ExpressionContext? context = null) : base(logicalExpressionFactory,logicalExpressionCache, evaluationVisitor, parameterExtractionVisitor)
    {
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
        EvaluationVisitor = evaluationVisitor;
        ParameterExtractionVisitor = parameterExtractionVisitor;
        ExpressionString = expression;
        Context = context ?? new();
    }

    public AdvancedExpression(
        ILogicalExpressionFactory logicalExpressionFactory, 
        ILogicalExpressionCache logicalExpressionCache, 
        IEvaluationVisitor evaluationVisitor,
        IParameterExtractionVisitor parameterExtractionVisitor, 
        LogicalExpression logicalExpression,
        ExpressionContext? context = null): base(logicalExpressionFactory,logicalExpressionCache, evaluationVisitor, parameterExtractionVisitor)
    {
        
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
        EvaluationVisitor = evaluationVisitor;
        ParameterExtractionVisitor = parameterExtractionVisitor;
        LogicalExpression = logicalExpression;
        Context = context ?? new();
    }
}