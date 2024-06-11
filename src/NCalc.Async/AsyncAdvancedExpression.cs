using NCalc.Cache;
using NCalc.Domain;
using NCalc.Factories;
using NCalc.Visitors;

namespace NCalc;

/// <summary>
/// An <see cref="AsyncExpression"/> object with dependency injection friendly constructors.
/// </summary>
public class AsyncAdvancedExpression : AsyncExpression
{
    public AsyncAdvancedExpression(
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache,
        IAsyncEvaluationVisitor asyncEvaluationVisitor,
        IParameterExtractionVisitor parameterExtractionVisitor,
        string expression,
        ExpressionContext? context = null) : base(logicalExpressionFactory, logicalExpressionCache, asyncEvaluationVisitor, parameterExtractionVisitor)
    {
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
        AsyncEvaluationVisitor = asyncEvaluationVisitor;
        ParameterExtractionVisitor = parameterExtractionVisitor;
        ExpressionString = expression;
        Context = context ?? new();
    }

    public AsyncAdvancedExpression(
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache,
        IAsyncEvaluationVisitor asyncEvaluationVisitor,
        IParameterExtractionVisitor parameterExtractionVisitor,
        LogicalExpression logicalExpression,
        ExpressionContext? context = null) : base(logicalExpressionFactory, logicalExpressionCache, asyncEvaluationVisitor, parameterExtractionVisitor)
    {

        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
        AsyncEvaluationVisitor = asyncEvaluationVisitor;
        ParameterExtractionVisitor = parameterExtractionVisitor;
        LogicalExpression = logicalExpression;
        Context = context ?? new();
    }
}