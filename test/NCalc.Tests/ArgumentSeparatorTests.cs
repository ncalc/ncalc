using NCalc.Exceptions;
using NCalc.Parser;

namespace NCalc.Tests;

public class ArgumentSeparatorTests
{
    [Theory]
    [InlineData("Max(1, 2)", 2, ',')]
    [InlineData("Max(1; 2)", 2, ';')]
    [InlineData("Min(3, 1)", 1, ',')]
    [InlineData("Min(3; 1)", 1, ';')]
    [InlineData("Round(3.14159, 2)", 3.14, ',')]
    [InlineData("Round(3.14159; 2)", 3.14, ';')]
    public void Should_Parse_Functions_With_Different_Separators(string expression, double expected, char separator)
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
    [InlineData("if(true, 'yes', 'no')", "yes", ',')]
    [InlineData("if(true; 'yes'; 'no')", "yes", ';')]
    [InlineData("if(1 > 2, 10, 20)", 20, ',')]
    [InlineData("if(1 > 2; 10; 20)", 20, ';')]
    public void Should_Parse_Conditional_Functions_With_Different_Separators(string expression, object expected, char separator)
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
    [InlineData("Max(1, 2)", ';')] // Using comma in expression but semicolon separator
    [InlineData("Max(1; 2)", ',')] // Using semicolon in expression but comma separator
    [InlineData("Min(1, 2)", ';')] // Multiple arguments with wrong separator
    [InlineData("Round(3.14; 2)", ',')] // Different function with wrong separator
    public void Should_Throw_Exception_With_Incorrect_Separator(string expression, char separator)
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
        var commaOptions = LogicalExpressionParserOptions.WithArgumentSeparator(',');
        var semicolonOptions = LogicalExpressionParserOptions.WithArgumentSeparator(';');

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
        var options1 = LogicalExpressionParserOptions.WithArgumentSeparator(',');
        var options2 = LogicalExpressionParserOptions.WithArgumentSeparator(';');
        var options3 = LogicalExpressionParserOptions.WithArgumentSeparator(','); // Same as options1

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
        var options = LogicalExpressionParserOptions.Create(germanCulture, ';');
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
    [InlineData(',')]
    [InlineData(';')]
    [InlineData(':')]
    public void Should_Support_Various_Separator_Characters(char separator)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var expression = $"Max(1{separator}3)";

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
    public void Should_Support_Nested_Functions_With_Custom_Separator()
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(';');
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
    [InlineData("Max(1 , 2)", ',')] // Spaces around separator
    [InlineData("Max(1 ; 2)", ';')]
    [InlineData("Max( 1, 2 )", ',')]  // Spaces around arguments
    [InlineData("Max( 1; 2 )", ';')]
    public void Should_Handle_Whitespace_Around_Separators(string expression, char separator)
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
        var commaOptions = LogicalExpressionParserOptions.WithArgumentSeparator(',');
        var semicolonOptions = LogicalExpressionParserOptions.WithArgumentSeparator(';');
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
