using NCalc.Handlers;
using NCalc.Helpers;
using static NCalc.Helpers.EvaluationHelper;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to synchronously evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
/// <param name="context">Contextual parameters of the <see cref="LogicalExpression"/>, like custom functions and parameters.</param>
public class EvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default) : ILogicalExpressionVisitor<object?>
{
    protected CancellationToken CancellationToken { get; } = cancellationToken;

    public virtual object? Visit(TernaryExpression expression)
    {
        var left = Convert.ToBoolean(expression.LeftExpression.Accept(this), context.CultureInfo);

        return left
            ? expression.MiddleExpression.Accept(this)
            : expression.RightExpression.Accept(this);
    }

    public virtual object? Visit(BinaryExpression expression)
    {
        var binaryEventArgs = new BinaryEventArgs(expression, this, new AsyncEvaluationVisitor(context, CancellationToken), CancellationToken);
        OnEvaluateBinary(binaryEventArgs);

        if (binaryEventArgs.HasResult)
            return binaryEventArgs.Result;

        if (expression.Type == BinaryExpressionType.And)
        {
            return Convert.ToBoolean(binaryEventArgs.LeftValue(), context.CultureInfo) &&
                   Convert.ToBoolean(binaryEventArgs.RightValue(), context.CultureInfo);
        }

        if (expression.Type == BinaryExpressionType.Or)
        {
            return Convert.ToBoolean(binaryEventArgs.LeftValue(), context.CultureInfo) ||
                   Convert.ToBoolean(binaryEventArgs.RightValue(), context.CultureInfo);
        }

        var left = binaryEventArgs.LeftValue();
        var right = binaryEventArgs.RightValue();

        return EvaluationVisitorHelper.EvaluateBinary(expression.Type, left, right, context);
    }

    public virtual object? Visit(UnaryExpression expression)
    {
        var result = expression.Expression.Accept(this);

        return Unary(expression, result, context);
    }

    public virtual object? Visit(Function function)
    {
        var functionName = function.Identifier.Name;
        var functionData = new FunctionData(
            function.Identifier.Id,
            function.Parameters,
            context,
            this,
            null,
            CancellationToken);
        var functionArgs = new FunctionEventArgs(functionData);

        OnEvaluateFunction(functionName, functionArgs);

        if (functionArgs.HasResult)
            return functionArgs.Result;

        if (context.Functions.TryGetValue(functionName, out var expressionFunction))
            return expressionFunction(functionData);

        return BuiltInFunctionHelper.Evaluate(functionName, functionData);
    }

    public virtual object? Visit(Identifier identifier)
    {
        var value = EvaluationVisitorHelper.GetIdentifierValue(identifier, context, CancellationToken);

        return value is Expression expression ? expression.Evaluate(CancellationToken) : value;
    }

    public virtual object? Visit(ValueExpression expression) => expression.Value;

    public virtual object? Visit(LogicalExpressionList list)
    {
        if (list.Count == 0) return Array.Empty<object?>();

        var expressions = list.AsSpan();
        var listCount = expressions.Length;

        var result = new object?[listCount];

        for (var index = 0; index < listCount; index++)
        {
            result[index] = expressions[index].Accept(this);
        }

        return result;
    }

    protected bool Compare(object? a, object? b, ComparisonType comparisonType)
    {
        return EvaluationVisitorHelper.Compare(a, b, comparisonType, context);
    }

    protected void OnEvaluateFunction(string name, FunctionEventArgs args)
    {
        context.EvaluateFunctionHandler?.Invoke(name, args);
    }

    protected void OnEvaluateBinary(BinaryEventArgs args)
    {
        context.EvaluateBinaryHandler?.Invoke(args);
    }
}
