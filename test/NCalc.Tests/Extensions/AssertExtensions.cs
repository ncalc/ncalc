#nullable enable
using TUnit.Assertions.Core;
using TUnit.Assertions.Sources;
namespace NCalc.Tests.Extensions;
public static class AssertExtensions
{
    public sealed class ExpressionValueAssertion(AssertionContext<object?> context) : ValueAssertion<object?>(context);
    public sealed class ExpressionValueAssertion<T>(AssertionContext<T> context) : ValueAssertion<T>(context);

    extension(IAssertionSource<Expression> source)
    {
        public ExpressionValueAssertion Expression()
        {
            source.Context.ExpressionBuilder.Append(".Expression()");
            return new ExpressionValueAssertion(
                source.Context.Map<object?>(async expression =>
                {
                    if (expression != null)
                        return await expression.EvaluateAsync(CancellationToken.None);
                    return null;
                }));
        }

        public ExpressionValueAssertion<T> Expression<T>() where T : IConvertible
        {
            source.Context.ExpressionBuilder.Append($".Expression<{typeof(T).Name}>()");
            return new ExpressionValueAssertion<T>(
                source.Context.Map<T>(async expression =>
                {
                    if (expression != null)
                        return await expression.EvaluateAsync<T>(CancellationToken.None);
                    return default!;
                }));
        }
    }
    extension(IAssertionSource<string> source)
    {
        public ExpressionValueAssertion Expression()
        {
            source.Context.ExpressionBuilder.Append(".Expression()");
            return new ExpressionValueAssertion(
                source.Context.Map<object?>(async expressionText =>
                    await new Expression(expressionText).EvaluateAsync(CancellationToken.None)));
        }
        public ExpressionValueAssertion Expression(ExpressionContext context)
        {
            source.Context.ExpressionBuilder.Append(".Expression(parameters)");
            return new ExpressionValueAssertion(
                source.Context.Map<object?>(async expressionText =>
                    await new Expression(expressionText, context).EvaluateAsync(CancellationToken.None)));
        }
        public ExpressionValueAssertion Expression(IDictionary<string, object?> parameters)
        {
            source.Context.ExpressionBuilder.Append(".Expression(parameters)");
            return new ExpressionValueAssertion(
                source.Context.Map<object?>(async expressionText =>
                    await new Expression(expressionText, new ExpressionContext
                    {
                        StaticParameters = parameters
                    }).EvaluateAsync(CancellationToken.None)));
        }

        public ExpressionValueAssertion<T> Expression<T>() where T : IConvertible
        {
            source.Context.ExpressionBuilder.Append($".Expression<{typeof(T).Name}>()");
            return new ExpressionValueAssertion<T>(
                source.Context.Map<T>(async expressionText =>
                    await new Expression(expressionText).EvaluateAsync<T>(CancellationToken.None)));
        }
        public ExpressionValueAssertion<T> Expression<T>(ExpressionContext context) where T : IConvertible
        {
            source.Context.ExpressionBuilder.Append($".Expression<{typeof(T).Name}>(parameters)");
            return new ExpressionValueAssertion<T>(
                source.Context.Map<T>(async expressionText =>
                    await new Expression(expressionText, context).EvaluateAsync<T>(CancellationToken.None)));
        }
        public ExpressionValueAssertion<T> Expression<T>(IDictionary<string, object?> parameters) where T : IConvertible
        {
            source.Context.ExpressionBuilder.Append($".Expression<{typeof(T).Name}>(parameters)");
            return new ExpressionValueAssertion<T>(
                source.Context.Map<T>(async expressionText =>
                    await new Expression(expressionText, new ExpressionContext
                    {
                        StaticParameters = parameters
                    }).EvaluateAsync<T>(CancellationToken.None)));
        }
    }
}