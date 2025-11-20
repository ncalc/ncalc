using NCalc.Exceptions;
using NCalc.Parser;

namespace NCalc.Tests;

public class ArgumentSeparatorTests
{
    [Theory]
    [InlineData("Max(1, 2)", 2, ArgumentSeparator.Comma)]
    [InlineData("Max(1; 2)", 2, ArgumentSeparator.Semicolon)]
    [InlineData("Min(3, 1)", 1, ArgumentSeparator.Comma)]
    [InlineData("Min(3; 1)", 1, ArgumentSeparator.Semicolon)]
    [InlineData("Round(3.14159, 2)", 3.14, ArgumentSeparator.Comma)]
    [InlineData("Round(3.14159; 2)", 3.14, ArgumentSeparator.Semicolon)]
    public void Should_Parse_Functions_With_Different_Separators(string expression, double expected, ArgumentSeparator separator)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate();

        // Assert
        Assert.Equal(expected, Convert.ToDouble(evaluationResult), 2);
    }

    [Theory]
    [InlineData("if(true, 'yes', 'no')", "yes", ArgumentSeparator.Comma)]
    [InlineData("if(true; 'yes'; 'no')", "yes", ArgumentSeparator.Semicolon)]
    [InlineData("if(1 > 2, 10, 20)", 20, ArgumentSeparator.Comma)]
    [InlineData("if(1 > 2; 10; 20)", 20, ArgumentSeparator.Semicolon)]
    public void Should_Parse_Conditional_Functions_With_Different_Separators(string expression, object expected, ArgumentSeparator separator)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate();

        // Assert
        Assert.Equal(expected, evaluationResult);
    }

    [Theory]
    [InlineData("Max(1, 2)", ArgumentSeparator.Semicolon)] // Using comma in expression but semicolon separator
    [InlineData("Max(1; 2)", ArgumentSeparator.Comma)] // Using semicolon in expression but comma separator
    [InlineData("Min(1, 2)", ArgumentSeparator.Semicolon)] // Multiple arguments with wrong separator
    [InlineData("Round(3.14; 2)", ArgumentSeparator.Comma)] // Different function with wrong separator
    public void Should_Throw_Exception_With_Incorrect_Separator(string expression, ArgumentSeparator separator)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act & Assert
        Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse(context));
    }

    [Fact]
    public void Should_Support_Mixed_Separators_In_Different_Parsers()
    {
        // Arrange
        var commaOptions = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Comma);
        var semicolonOptions = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);

        var commaExpression = "Max(1, 2)";
        var semicolonExpression = "Max(3; 4)";

        var commaContext = new LogicalExpressionParserContext(commaExpression, ExpressionOptions.None)
        {
            ParserOptions = commaOptions
        };

        var semicolonContext = new LogicalExpressionParserContext(semicolonExpression, ExpressionOptions.None)
        {
            ParserOptions = semicolonOptions
        };

        // Act
        var commaResult = LogicalExpressionParser.Parse(commaContext);
        var semicolonResult = LogicalExpressionParser.Parse(semicolonContext);

        var commaValue = new Expression(commaResult).Evaluate();
        var semicolonValue = new Expression(semicolonResult).Evaluate();

        // Assert
        Assert.Equal(2, commaValue);
        Assert.Equal(4, semicolonValue);
    }

    [Fact]
    public void Should_Cache_Parsers_For_Different_Separator_Options()
    {
        // Arrange
        var options1 = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Comma);
        var options2 = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        var options3 = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Comma); // Same as options1

        // Act
        var parser1 = LogicalExpressionParser.GetOrCreateExpressionParser(options1);
        var parser2 = LogicalExpressionParser.GetOrCreateExpressionParser(options2);
        var parser3 = LogicalExpressionParser.GetOrCreateExpressionParser(options3);

        // Assert
        Assert.NotSame(parser1, parser2); // Different separators should have different parsers
        Assert.Same(parser1, parser3); // Same options should return cached parser
    }

    [Fact]
    public void Should_Support_Culture_And_Separator_Combination()
    {
        // Arrange
        var germanCulture = new CultureInfo("de-DE");
        var options = LogicalExpressionParserOptions.Create(germanCulture, ArgumentSeparator.Semicolon);
        var expression = "Max(1.5; 2.3)"; // Using dots for decimals to avoid confusion with argument separator

        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate();

        // Assert
        Assert.Equal(2.3, Convert.ToDouble(evaluationResult), 1);
    }

    [Theory]
    [InlineData(ArgumentSeparator.Comma)]
    [InlineData(ArgumentSeparator.Semicolon)]
    [InlineData(ArgumentSeparator.Colon)]
    public void Should_Support_Various_Separator_Characters(ArgumentSeparator separator)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var separatorChar = separator switch
        {
            ArgumentSeparator.Comma => ',',
            ArgumentSeparator.Semicolon => ';',
            ArgumentSeparator.Colon => ':',
            _ => ';'
        };
        var expression = $"Max(1{separatorChar}3)";

        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate();

        // Assert
        Assert.True(Math.Abs(Convert.ToDouble(evaluationResult) - 3.0) < 0.0001,
            $"Expected 3, but got {evaluationResult} (type: {evaluationResult?.GetType()})");
    }

    [Fact]
    public void Should_Maintain_Backward_Compatibility_With_Default_Comma_Separator()
    {
        // Arrange
        const string expression = "Max(2, 3)";
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None);
        // Not setting ParserOptions should default to comma separator

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate();

        // Assert
        Assert.True(Math.Abs(Convert.ToDouble(evaluationResult) - 3.0) < 0.0001,
            $"Expected 3, but got {evaluationResult} (type: {evaluationResult?.GetType()})");
    }

    [Fact]
    public void Should_Support_Semicolon_Separator_With_Explicit_Options()
    {
        // Arrange
        const string expression = "Max(2; 3)";
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate();

        // Assert
        Assert.True(Math.Abs(Convert.ToDouble(evaluationResult) - 3.0) < 0.0001,
            $"Expected 3, but got {evaluationResult} (type: {evaluationResult?.GetType()})");
    }

    [Fact]
    public void Should_Support_Nested_Functions_With_Custom_Separator()
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        var expression = "Max(Min(1; 2); Max(3; 4))";

        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate();

        // Assert
        Assert.Equal(4, evaluationResult);
    }

    [Theory]
    [InlineData("Max(1 , 2)", ArgumentSeparator.Comma)] // Spaces around separator
    [InlineData("Max(1 ; 2)", ArgumentSeparator.Semicolon)]
    [InlineData("Max( 1, 2 )", ArgumentSeparator.Comma)]  // Spaces around arguments
    [InlineData("Max( 1; 2 )", ArgumentSeparator.Semicolon)]
    public void Should_Handle_Whitespace_Around_Separators(string expression, ArgumentSeparator separator)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate();

        // Assert
        Assert.Equal(2, evaluationResult);
    }

    [Fact]
    public void Should_Handle_Single_Argument_Functions_Regardless_Of_Separator()
    {
        // Arrange
        var commaOptions = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Comma);
        var semicolonOptions = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        var expression = "Abs(-5)"; // Single argument function

        var commaContext = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = commaOptions
        };

        var semicolonContext = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = semicolonOptions
        };

        // Act
        var commaResult = LogicalExpressionParser.Parse(commaContext);
        var semicolonResult = LogicalExpressionParser.Parse(semicolonContext);

        var commaValue = new Expression(commaResult).Evaluate();
        var semicolonValue = new Expression(semicolonResult).Evaluate();

        // Assert
        Assert.Equal(5.0, commaValue);
        Assert.Equal(5.0, semicolonValue);
    }
}
