using NCalc.Parser;

namespace NCalc.Tests;

public class ExpressionSeparatorTests
{
    [Test]
    [Arguments("Max(1, 2)", 2, LogicalExpressionArgumentSeparator.Comma)]
    [Arguments("Max(1; 2)", 2, LogicalExpressionArgumentSeparator.Semicolon)]
    [Arguments("Min(5, 3)", 3, LogicalExpressionArgumentSeparator.Comma)]
    [Arguments("Min(5; 3)", 3, LogicalExpressionArgumentSeparator.Semicolon)]
    public async Task Expression_Should_Support_Custom_Separators_End_To_End(string expressionText, int expected, LogicalExpressionArgumentSeparator separator)
    {
        // Arrange
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = separator
        };
        var context = new LogicalExpressionParserContext(expressionText, options);

        var logicalExpression = LogicalExpressionParser.Parse(context);

        // Act
        var expression = new Expression(logicalExpression);
        var result = expression.Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(Convert.ToInt32(result)).IsEqualTo(expected);
    }

    [Test]
    public async Task Expression_Should_Work_With_Custom_Functions_And_Separators()
    {
        // Arrange
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = LogicalExpressionArgumentSeparator.Semicolon
        };
        const string expressionText = "CustomAdd(10; 20)";

        var context = new LogicalExpressionParserContext(expressionText, options);

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

    [Test]
    public async Task Expression_Should_Handle_Parameters_With_Custom_Separators()
    {
        // Arrange
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = LogicalExpressionArgumentSeparator.Semicolon
        };

        const string expressionText = "Max(x; y)";

        var context = new LogicalExpressionParserContext(expressionText, options);

        var logicalExpression = LogicalExpressionParser.Parse(context);
        var expression = new Expression(logicalExpression)
        {
            Parameters =
            {
                ["x"] = 5,
                ["y"] = 10
            }
        };

        // Act
        var result = expression.Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(result).IsEqualTo(10);
    }
}
