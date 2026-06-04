#nullable enable
using TUnit.Assertions.Core;
using TUnit.Assertions.Sources;

namespace NCalc.Tests.Extensions;

public static class AssertExtensions
{
    public sealed class ExpressionValueAssertion(AssertionContext<object?> context) : ValueAssertion<object?>(context);

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
    }
}
