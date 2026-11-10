namespace NCalc.Tests;

public class ExpressionSeparatorTests
{
    [Test]
    public async Task Expression_Should_Work_With_Custom_Functions_And_Separators()
    {
        // Arrange
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Semicolon
        };
        const string expressionText = "CustomAdd(10; 20)";

        var context = new LogicalExpressionParseContext(expressionText, options);

        var logicalExpression = LogicalExpressionParser.Parse(context);
        var expression = new Expression(logicalExpression);

        expression.EvaluateFunction += (name, args) =>
        {
            if (name == "CustomAdd" && args.Parameters.Count == 2)
            {
                args.Result = Convert.ToDouble(args.Parameters.Evaluate(0)) +
                    Convert.ToDouble(args.Parameters.Evaluate(1));
            }
        };

        // Act
        var result = expression.Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(result).IsEqualTo(30.0);
    }
}
