using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Exceptions;
using static NCalc.Helpers.EvaluationHelper;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to synchronously evaluating <see cref="LogicalExpression"/>.
/// </summary>
public class EvaluationVisitor(
    ExpressionContext context,
    ExpressionEvaluationOptions options,
    CultureInfo cultureInfo,
    IEvaluationVisitorFactory? evaluationVisitorFactory = null,
    CancellationToken cancellationToken = default) : ILogicalExpressionVisitor<object?>
{
    protected CancellationToken CancellationToken { get; } = cancellationToken;
    protected IEvaluationVisitorFactory? EvaluationVisitorFactory { get; } = evaluationVisitorFactory;

    protected AsyncEvaluationVisitor CreateAsyncEvaluationVisitor()
    {
        return EvaluationVisitorFactory?.CreateAsyncEvaluationVisitor(context, options, cultureInfo, CancellationToken)
               ?? new AsyncEvaluationVisitor(context, options, cultureInfo, cancellationToken: CancellationToken);
    }

    public virtual object? Visit(TernaryExpression expression)
    {
        var left = Convert.ToBoolean(expression.LeftExpression.Accept(this), cultureInfo);

        return left
            ? expression.MiddleExpression.Accept(this)
            : expression.RightExpression.Accept(this);
    }

    public virtual object? Visit(BinaryExpression expression)
    {
        var binaryEventArgs = new BinaryEventArgs(expression, this, CreateAsyncEvaluationVisitor(), CancellationToken);
        OnEvaluateBinary(binaryEventArgs);

        if (binaryEventArgs.HasResult)
            return binaryEventArgs.Result;

        if (expression.Type == BinaryExpressionType.And)
        {
            return Convert.ToBoolean(binaryEventArgs.LeftValue(), cultureInfo) &&
                   Convert.ToBoolean(binaryEventArgs.RightValue(), cultureInfo);
        }

        if (expression.Type == BinaryExpressionType.Or)
        {
            return Convert.ToBoolean(binaryEventArgs.LeftValue(), cultureInfo) ||
                   Convert.ToBoolean(binaryEventArgs.RightValue(), cultureInfo);
        }

        var left = binaryEventArgs.LeftValue();
        var right = binaryEventArgs.RightValue();

        return EvaluationVisitorHelper.EvaluateBinary(expression.Type, left, right, options, cultureInfo);
    }

    public virtual object? Visit(UnaryExpression expression)
    {
        var result = expression.Expression.Accept(this);

        return Unary(expression, result, options, cultureInfo);
    }

    public virtual object? Visit(Function function)
    {
        var functionName = function.Identifier.Name;
        var functionData = new FunctionData(
            function.Identifier.Id,
            function.Parameters,
            context,
            options,
            cultureInfo,
            this,
            CreateAsyncEvaluationVisitor(),
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
        var value = GetIdentifierValue(identifier);

        return value is Expression expression ? expression.Evaluate(CancellationToken) : value;
    }

    public virtual object? Visit(ValueExpression expression) => expression.Value;

    public virtual object? Visit(LogicalExpressionList list)
    {
        if (list.Count == 0)
            return Array.Empty<object?>();

        var expressions = list.AsSpan();
        var listCount = expressions.Length;

        var result = new object?[listCount];

        for (var index = 0; index < listCount; index++)
        {
            result[index] = expressions[index].Accept(this);
        }

        return result;
    }

    protected void OnEvaluateFunction(string name, FunctionEventArgs args)
    {
        context.EvaluateFunctionHandler?.Invoke(name, args);
    }

    protected void OnEvaluateBinary(BinaryEventArgs args)
    {
        context.EvaluateBinaryHandler?.Invoke(args);
    }

    private object? GetIdentifierValue(Identifier identifier)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new ParameterEventArgs(identifier.Id, CancellationToken);

        context.EvaluateParameterHandler?.Invoke(identifierName, parameterArgs);

        if (parameterArgs.HasResult)
            return parameterArgs.Result;

        if (context.Parameters.TryGetValue(identifierName, out var parameter))
        {
            if (parameter is Expression expression)
            {
                ShareParametersWithChildExpression(expression);
                return expression;
            }

            return parameter;
        }

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
            return dynamicParameter(new ParameterData(identifier.Id, context, CancellationToken));

        if (identifierName.Equals("null", StringComparison.InvariantCultureIgnoreCase) &&
            options.AllowNullParameter)
        {
            return null;
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    private void ShareParametersWithChildExpression(Expression expression)
    {
        foreach (var parameter in context.Parameters)
            expression.Parameters[parameter.Key] = parameter.Value;

        foreach (var parameter in context.DynamicParameters)
            expression.DynamicParameters[parameter.Key] = parameter.Value;

        foreach (var parameter in context.AsyncParameters)
            expression.AsyncParameters[parameter.Key] = parameter.Value;

        expression.SetEvaluationVisitorFactory(EvaluationVisitorFactory);

        expression.EvaluateFunction += context.EvaluateFunctionHandler;
        expression.EvaluateAsyncFunction += context.EvaluateAsyncFunctionHandler;
        expression.EvaluateParameter += context.EvaluateParameterHandler;
        expression.EvaluateAsyncParameter += context.EvaluateAsyncParameterHandler;
        expression.EvaluateBinary += context.EvaluateBinaryHandler;
        expression.EvaluateBinaryAsync += context.EvaluateBinaryAsyncHandler;
    }
}
