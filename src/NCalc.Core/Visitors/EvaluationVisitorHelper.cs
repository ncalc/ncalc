using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Helpers;

namespace NCalc.Visitors;

internal static class EvaluationVisitorHelper
{
    internal static object? EvaluateBinary(
        BinaryExpressionType expressionType,
        object? left,
        object? right,
        ExpressionContext context)
    {
        switch (expressionType)
        {
            case BinaryExpressionType.Div:
                return EvaluationHelper.Div(left, right, context);

            case BinaryExpressionType.Equal:
                return Compare(left, right, ComparisonType.Equal, context);

            case BinaryExpressionType.Greater:
                return Compare(left, right, ComparisonType.Greater, context);

            case BinaryExpressionType.GreaterOrEqual:
                return Compare(left, right, ComparisonType.GreaterOrEqual, context);

            case BinaryExpressionType.Lesser:
                return Compare(left, right, ComparisonType.Lesser, context);

            case BinaryExpressionType.LesserOrEqual:
                return Compare(left, right, ComparisonType.LesserOrEqual, context);

            case BinaryExpressionType.NotEqual:
                return Compare(left, right, ComparisonType.NotEqual, context);

            case BinaryExpressionType.Minus:
                return EvaluationHelper.Minus(left, right, context);

            case BinaryExpressionType.Modulo:
                return EvaluationHelper.Modulo(left, right, context);

            case BinaryExpressionType.Plus:
                return EvaluationHelper.Plus(left, right, context);

            case BinaryExpressionType.Times:
                return EvaluationHelper.Times(left, right, context);

            case BinaryExpressionType.BitwiseAnd:
                return Convert.ToUInt64(left, context.CultureInfo) &
                       Convert.ToUInt64(right, context.CultureInfo);

            case BinaryExpressionType.BitwiseOr:
                return Convert.ToUInt64(left, context.CultureInfo) |
                       Convert.ToUInt64(right, context.CultureInfo);

            case BinaryExpressionType.BitwiseXOr:
                return Convert.ToUInt64(left, context.CultureInfo) ^
                       Convert.ToUInt64(right, context.CultureInfo);

            case BinaryExpressionType.LeftShift:
                return Convert.ToUInt64(left, context.CultureInfo) <<
                       Convert.ToInt32(right, context.CultureInfo);

            case BinaryExpressionType.RightShift:
                return Convert.ToUInt64(left, context.CultureInfo) >>
                       Convert.ToInt32(right, context.CultureInfo);

            case BinaryExpressionType.Exponentiation:
                return MathHelper.Pow(left, right, context.MathHelperOptions);

            case BinaryExpressionType.In:
                return EvaluationHelper.In(right, left, context);

            case BinaryExpressionType.NotIn:
                return !EvaluationHelper.In(right, left, context);

            case BinaryExpressionType.Like:
                return LikeOperatorHelper.Like(left, right, context);

            case BinaryExpressionType.NotLike:
                return !LikeOperatorHelper.Like(left, right, context);

            default:
                return null;
        }
    }

    internal static bool Compare(
        object? a,
        object? b,
        ComparisonType comparisonType,
        ExpressionContext context)
    {
        if (TypeHelper.HasNullOrTypeConflict(a, b, context.Options))
            return comparisonType == ComparisonType.NotEqual;

        var result = TypeHelper.CompareUsingMostPreciseType(a, b, context.ComparisonOptions);

        return comparisonType switch
        {
            ComparisonType.Equal => result is ComparisonResult.Equal,
            ComparisonType.Greater => result is ComparisonResult.Greater,
            ComparisonType.GreaterOrEqual => result is ComparisonResult.Greater or ComparisonResult.Equal,
            ComparisonType.Lesser => result is ComparisonResult.Less,
            ComparisonType.LesserOrEqual => result is ComparisonResult.Less or ComparisonResult.Equal,
            ComparisonType.NotEqual => result is not ComparisonResult.Equal,
            _ => throw new ArgumentOutOfRangeException(nameof(comparisonType), comparisonType, null)
        };
    }

    internal static object? GetIdentifierValue(
        Identifier identifier,
        ExpressionContext context,
        CancellationToken cancellationToken,
        IEvaluationVisitorFactory? evaluationVisitorFactory = null)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new ParameterEventArgs(identifier.Id, cancellationToken);

        context.EvaluateParameterHandler?.Invoke(identifierName, parameterArgs);

        if (parameterArgs.HasResult)
            return parameterArgs.Result;

        if (context.StaticParameters.TryGetValue(identifierName, out var parameter))
        {
            if (parameter is Expression expression)
            {
                ShareParametersWithChildExpression(expression, context, evaluationVisitorFactory);

                return expression;
            }

            return parameter;
        }

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
            return dynamicParameter(new ParameterData(identifier.Id, context, cancellationToken));

        if (identifierName.Equals("null", StringComparison.InvariantCultureIgnoreCase) &&
            context.Options.HasFlag(ExpressionOptions.AllowNullParameter))
        {
            return null;
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    internal static async Task<object?> GetIdentifierValueAsync(
        Identifier identifier,
        ExpressionContext context,
        CancellationToken cancellationToken,
        IEvaluationVisitorFactory? evaluationVisitorFactory = null)
    {
        var identifierName = identifier.Name;

        var parameterArgs = new ParameterEventArgs(identifier.Id, cancellationToken);

        context.EvaluateParameterHandler?.Invoke(identifierName, parameterArgs);

        if (!parameterArgs.HasResult)
            await (context.EvaluateAsyncParameterHandler?.Invoke(identifierName, parameterArgs) ?? Task.CompletedTask);

        if (parameterArgs.HasResult)
            return parameterArgs.Result;

        if (context.StaticParameters.TryGetValue(identifierName, out var parameter))
        {
            if (parameter is Expression expression)
            {
                ShareParametersWithChildExpression(expression, context, evaluationVisitorFactory);

                return expression;
            }

            return parameter;
        }

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
            return dynamicParameter(new ParameterData(identifier.Id, context, cancellationToken));

        if (context.AsyncParameters.TryGetValue(identifierName, out var asyncParameter))
            return await asyncParameter(new ParameterData(identifier.Id, context, cancellationToken));

        if (identifierName.Equals("null", StringComparison.InvariantCultureIgnoreCase) &&
            context.Options.HasFlag(ExpressionOptions.AllowNullParameter))
        {
            return null;
        }

        throw new NCalcParameterNotDefinedException(identifierName);
    }

    private static void ShareParametersWithChildExpression(
        Expression expression,
        ExpressionContext context,
        IEvaluationVisitorFactory? evaluationVisitorFactory)
    {
        foreach (var p in context.StaticParameters)
            expression.Parameters[p.Key] = p.Value;

        foreach (var p in context.DynamicParameters)
            expression.DynamicParameters[p.Key] = p.Value;

        foreach (var p in context.AsyncParameters)
            expression.AsyncParameters[p.Key] = p.Value;

        expression.SetEvaluationVisitorFactory(evaluationVisitorFactory);

        expression.EvaluateFunction += context.EvaluateFunctionHandler;
        expression.EvaluateAsyncFunction += context.EvaluateAsyncFunctionHandler;

        expression.EvaluateParameter += context.EvaluateParameterHandler;
        expression.EvaluateAsyncParameter += context.EvaluateAsyncParameterHandler;

        expression.EvaluateBinary += context.EvaluateBinaryHandler;
        expression.EvaluateBinaryAsync += context.EvaluateBinaryAsyncHandler;
    }
}
