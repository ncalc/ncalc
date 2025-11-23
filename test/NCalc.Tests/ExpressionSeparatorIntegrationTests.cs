using NCalc.Parser;

namespace NCalc.Tests;

public class ExpressionSeparatorIntegrationTests
{
    [Theory]
    [InlineData("Max(1, 2)", 2, ArgumentSeparator.Comma)]
    [InlineData("Max(1; 2)", 2, ArgumentSeparator.Semicolon)]
    [InlineData("Min(5, 3)", 3, ArgumentSeparator.Comma)]
    [InlineData("Min(5; 3)", 3, ArgumentSeparator.Semicolon)]
    public void Expression_Should_Support_Custom_Separators_End_To_End(string expressionText, int expected, ArgumentSeparator separator)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var context = new LogicalExpressionParserContext(expressionText, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        var logicalExpression = LogicalExpressionParser.Parse(context);

        // Act
        var expression = new Expression(logicalExpression);
        var result = expression.Evaluate(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, Convert.ToInt32(result));
    }

    [Fact]
    public void Expression_Should_Work_With_Custom_Functions_And_Separators()
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        const string expressionText = "CustomAdd(10; 20)";

        var context = new LogicalExpressionParserContext(expressionText, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        var logicalExpression = LogicalExpressionParser.Parse(context);
        var expression = new Expression(logicalExpression);

        expression.EvaluateFunction += (name, args) =>
        {
            if (name == "CustomAdd" && args.Parameters.Length == 2)
            {
                args.Result = Convert.ToDouble(args.Parameters[0].Evaluate(args.CancellationToken)) +
                    Convert.ToDouble(args.Parameters[1].Evaluate(args.CancellationToken));
            }
        };

        // Act
        var result = expression.Evaluate(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(30.0, result);
    }

    [Fact]
    public void Expression_Should_Handle_Parameters_With_Custom_Separators()
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        const string expressionText = "Max(x; y)";

        var context = new LogicalExpressionParserContext(expressionText, ExpressionOptions.None)
        {
            ParserOptions = options
        };

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
        var result = expression.Evaluate(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(10, result);
    }
}
