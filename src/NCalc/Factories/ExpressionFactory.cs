using NCalc.Cache;
using NCalc.Domain;
using NCalc.Visitors;

namespace NCalc.Factories;

/// <summary>
/// Default <see cref="IExpressionFactory"/> implementation.
/// </summary>
public sealed class ExpressionFactory(
    ILogicalExpressionFactory logicalExpressionFactory, 
    ILogicalExpressionCache cache,
    IEvaluationVisitor evaluationVisitor,
    IParameterExtractionVisitor parameterExtractionVisitor
    ) : IExpressionFactory
{
    public Expression Create(string expression, ExpressionContext? expressionContext = null)
    {
        return new AdvancedExpression(logicalExpressionFactory,cache,evaluationVisitor,parameterExtractionVisitor,expression, expressionContext);
    }

    public Expression Create(LogicalExpression logicalExpression, ExpressionContext? expressionContext = null)
    {
        return new AdvancedExpression(logicalExpressionFactory,cache,evaluationVisitor,parameterExtractionVisitor,logicalExpression, expressionContext);
    }
}