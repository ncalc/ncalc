namespace NCalc.Tests.Extensions;

public static class AssertExtensions
{
    extension(Assert)
    {
        public static void Expression(object expected, string expression)
        {
            var expressionObject = new Expression(expression);

            Assert.Expression(expected, expressionObject);
        }

        public static void Expression(object expected, string expression, Dictionary<string, object> parameters)
        {
            var expressionObject = new Expression(expression, new ExpressionContext
            {
                StaticParameters = parameters
            });

            Assert.Expression(expected, expressionObject);
        }

        public static void Expression(object expected, Expression expression)
        {
            Assert.Equal(expected, expression.Evaluate(TestContext.Current.CancellationToken));
        }
    }
}