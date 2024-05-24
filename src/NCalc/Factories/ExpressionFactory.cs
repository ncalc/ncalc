using NCalc.Cache;
using NCalc.Domain;
using NCalc.Visitors;

namespace NCalc.Factories;

public sealed class ExpressionFactory(
    ILogicalExpressionFactory logicalExpressionFactory, 
    ILogicalExpressionCache cache,
    IEvaluationVisitor evaluationVisitor,
    IParameterExtractionVisitor parameterExtractionVisitor
    ) : IExpressionFactory
{
    public Expression Create(string expression, Action<ExpressionContext>? configure = null)
    {
        var context = new ExpressionContext();

        configure?.Invoke(context);

        return new AdvancedExpression(logicalExpressionFactory,cache,evaluationVisitor,parameterExtractionVisitor,expression, context);
    }

    public Expression Create(LogicalExpression logicalExpression, Action<ExpressionContext>? configure = null)
    {
        var context = new ExpressionContext();

        configure?.Invoke(context);
        
        return new AdvancedExpression(logicalExpressionFactory,cache,evaluationVisitor,parameterExtractionVisitor,logicalExpression, context);
    }
}