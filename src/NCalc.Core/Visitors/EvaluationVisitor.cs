using NCalc.Handlers;
using NCalc.Helpers;
using static NCalc.Helpers.EvaluationHelper;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to synchronously evaluating <see cref="LogicalExpression"/> objects into CLR objects.
/// </summary>
/// <param name="context">Contextual parameters of the <see cref="LogicalExpression"/>, like custom functions and parameters.</param>
public class EvaluationVisitor(ExpressionContext context) : ILogicalExpressionVisitor<object?>
{
    public virtual object? Visit(TernaryExpression expression, CancellationToken cancellationToken = default)
    {
        var left = Convert.ToBoolean(expression.LeftExpression.Accept(this, cancellationToken), context.CultureInfo);

        return left
            ? expression.MiddleExpression.Accept(this, cancellationToken)
            : expression.RightExpression.Accept(this, cancellationToken);
    }

    public virtual object? Visit(BinaryExpression expression, CancellationToken cancellationToken = default)
    {
        var binaryEventArgs = new BinaryEventArgs(expression, this, new AsyncEvaluationVisitor(context), cancellationToken);
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

    public virtual object? Visit(UnaryExpression expression, CancellationToken cancellationToken = default)
    {
        var result = expression.Expression.Accept(this, cancellationToken);

        return Unary(expression, result, context);
    }

    public virtual object? Visit(Function function, CancellationToken cancellationToken = default)
    {
        var functionName = function.Identifier.Name;
        var functionData = new FunctionData(
            function.Identifier.Id,
            function.Parameters,
            context,
            this,
            null,
            cancellationToken);
        var functionArgs = new FunctionEventArgs(functionData);

        OnEvaluateFunction(functionName, functionArgs);

        if (functionArgs.HasResult)
            return functionArgs.Result;

        if (context.Functions.TryGetValue(functionName, out var expressionFunction))
            return expressionFunction(functionData);

        return BuiltInFunctionHelper.Evaluate(functionName, functionData);
    }

    public virtual object? Visit(Identifier identifier, CancellationToken cancellationToken = default)
    {
        var value = EvaluationVisitorHelper.GetIdentifierValue(identifier, context, cancellationToken);

        return value is Expression expression ? expression.Evaluate(cancellationToken) : value;
    }

    public virtual object? Visit(ValueExpression expression, CancellationToken cancellationToken = default) => expression.Value;

    public virtual object? Visit(LogicalExpressionList list, CancellationToken cancellationToken = default)
    {
        List<object?> result = [];

        foreach (var value in list)
        {
            result.Add(value.Accept(this, cancellationToken));
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
