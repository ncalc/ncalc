using NCalc.Exceptions;
using System.Threading.Tasks;

namespace NCalc.Tests;

public class ArgumentSeparatorTests
{
    [Test]
    [Arguments("Max(1, 2)", 2, ArgumentSeparator.Comma)]
    [Arguments("Max(1; 2)", 2, ArgumentSeparator.Semicolon)]
    [Arguments("Min(3, 1)", 1, ArgumentSeparator.Comma)]
    [Arguments("Min(3; 1)", 1, ArgumentSeparator.Semicolon)]
    [Arguments("Round(3.14159, 2)", 3.14, ArgumentSeparator.Comma)]
    [Arguments("Round(3.14159; 2)", 3.14, ArgumentSeparator.Semicolon)]
    public async Task Should_Parse_Functions_With_Different_Separators(string expression, double expected, ArgumentSeparator separator)
    {
        // Arrange
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = separator
        };
        var context = new LogicalExpressionParseContext(expression, options);

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(CancellationToken.None);
        // TODO: TUnit migration - xUnit Assert.Equal had additional argument(s) (precision: 2) that could not be converted.

        // Assert
        await Assert.That(Convert.ToDouble(evaluationResult)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("if(true, 'yes', 'no')", "yes", ArgumentSeparator.Comma)]
    [Arguments("if(true; 'yes'; 'no')", "yes", ArgumentSeparator.Semicolon)]
    [Arguments("if(1 > 2, 10, 20)", 20, ArgumentSeparator.Comma)]
    [Arguments("if(1 > 2; 10; 20)", 20, ArgumentSeparator.Semicolon)]
    public async Task Should_Parse_Conditional_Functions_With_Different_Separators(string expression, object expected, ArgumentSeparator separator)
    {
        // Arrange
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = separator
        };
        var context = new LogicalExpressionParseContext(expression, options);

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(evaluationResult).IsEqualTo(expected);
    }

    [Test]
    [Arguments("Max(1, 2)", ArgumentSeparator.Semicolon)] // Using comma in expression but semicolon separator
    [Arguments("Max(1; 2)", ArgumentSeparator.Comma)] // Using semicolon in expression but comma separator
    [Arguments("Min(1, 2)", ArgumentSeparator.Semicolon)] // Multiple arguments with wrong separator
    [Arguments("Round(3.14; 2)", ArgumentSeparator.Comma)] // Different function with wrong separator
    public void Should_Throw_Exception_With_Incorrect_Separator(string expression, ArgumentSeparator separator)
    {
        // Arrange
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = separator
        };
        var context = new LogicalExpressionParseContext(expression, options);

        // Act & Assert
        Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse(context));
    }

    [Test]
    public async Task Should_Support_Mixed_Separators_In_Different_Parsers()
    {
        // Arrange
        var commaOptions = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Comma
        };
        var semicolonOptions = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Semicolon
        };

        var commaExpression = "Max(1, 2)";
        var semicolonExpression = "Max(3; 4)";

        var commaContext = new LogicalExpressionParseContext(commaExpression, commaOptions);

        var semicolonContext = new LogicalExpressionParseContext(semicolonExpression, semicolonOptions);

        // Act
        var commaResult = LogicalExpressionParser.Parse(commaContext);
        var semicolonResult = LogicalExpressionParser.Parse(semicolonContext);

        var commaValue = new Expression(commaResult).Evaluate(CancellationToken.None);
        var semicolonValue = new Expression(semicolonResult).Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(commaValue).IsEqualTo(2);
        await Assert.That(semicolonValue).IsEqualTo(4);
    }

    [Test]
    public async Task Should_Cache_Parsers_For_Different_Separator_Options()
    {
        // Arrange
        var culture = CultureInfo.InvariantCulture;

        var options1 = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Comma
        };

        var options2 = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Semicolon
        };

        var options3 = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Comma
        };

        // Act
        var parser1 = LogicalExpressionParser.GetOrCreateExpressionParser(options1, culture);
        var parser2 = LogicalExpressionParser.GetOrCreateExpressionParser(options2, culture);
        var parser3 = LogicalExpressionParser.GetOrCreateExpressionParser(options3, culture);

        // Assert
        await Assert.That(parser2).IsNotSameReferenceAs(parser1);
        await Assert.That(parser3).IsSameReferenceAs(parser1);
    }

    [Test]
    public async Task Should_Support_Culture_And_Separator_Combination()
    {
        // Arrange
        var germanCulture = new CultureInfo("de-DE");
        var options = new LogicalExpressionParserOptions()
        {
            ArgumentSeparator = ArgumentSeparator.Semicolon
        };
        var expression = "Max(1.5; 2.3)"; // Using dots for decimals to avoid confusion with argument separator

        var context = new LogicalExpressionParseContext(expression, options);

        // Act
        var result = LogicalExpressionParser.Parse(context, germanCulture);
        var evaluationResult = new Expression(result).Evaluate(CancellationToken.None);
        // TODO: TUnit migration - xUnit Assert.Equal had additional argument(s) (precision: 1) that could not be converted.

        // Assert
        await Assert.That(Convert.ToDouble(evaluationResult)).IsEqualTo(2.3);
    }

    [Test]
    [Arguments(ArgumentSeparator.Comma)]
    [Arguments(ArgumentSeparator.Semicolon)]
    [Arguments(ArgumentSeparator.Colon)]
    public async Task Should_Support_Various_Separator_Characters(ArgumentSeparator separator)
    {
        // Arrange
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = separator
        };
        var separatorChar = separator switch
        {
            ArgumentSeparator.Comma => ',',
            ArgumentSeparator.Semicolon => ';',
            ArgumentSeparator.Colon => ':',
            _ => ';'
        };
        var expression = $"Max(1{separatorChar}3)";

        var context = new LogicalExpressionParseContext(expression, options);

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(Math.Abs(Convert.ToDouble(evaluationResult) - 3.0) < 0.0001).IsTrue().Because($"Expected 3, but got {evaluationResult} (type: {evaluationResult?.GetType()})");
    }

    [Test]
    public async Task Should_Maintain_Backward_Compatibility_With_Default_Comma_Separator()
    {
        // Arrange
        const string expression = "Max(2, 3)";
        var context = new LogicalExpressionParseContext(expression);
        // Not setting ParserOptions should default to comma separator

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(Math.Abs(Convert.ToDouble(evaluationResult) - 3.0) < 0.0001).IsTrue().Because($"Expected 3, but got {evaluationResult} (type: {evaluationResult?.GetType()})");
    }

    [Test]
    public async Task Should_Support_Semicolon_Separator_With_Explicit_Options()
    {
        // Arrange
        const string expression = "Max(2; 3)";
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Semicolon
        };
        var context = new LogicalExpressionParseContext(expression, options);

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(Math.Abs(Convert.ToDouble(evaluationResult) - 3.0) < 0.0001).IsTrue().Because($"Expected 3, but got {evaluationResult} (type: {evaluationResult?.GetType()})");
    }

    [Test]
    public async Task Should_Support_Nested_Functions_With_Custom_Separator()
    {
        // Arrange
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Semicolon
        };
        var expression = "Max(Min(1; 2); Max(3; 4))";

        var context = new LogicalExpressionParseContext(expression, options);

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(evaluationResult).IsEqualTo(4);
    }

    [Test]
    [Arguments("Max(1 , 2)", ArgumentSeparator.Comma)] // Spaces around separator
    [Arguments("Max(1 ; 2)", ArgumentSeparator.Semicolon)]
    [Arguments("Max( 1, 2 )", ArgumentSeparator.Comma)]  // Spaces around arguments
    [Arguments("Max( 1; 2 )", ArgumentSeparator.Semicolon)]
    public async Task Should_Handle_Whitespace_Around_Separators(string expression, ArgumentSeparator separator)
    {
        // Arrange
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = separator
        };
        var context = new LogicalExpressionParseContext(expression, options);

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(evaluationResult).IsEqualTo(2);
    }

    [Test]
    public async Task Should_Handle_Single_Argument_Functions_Regardless_Of_Separator()
    {
        // Arrange
        var commaOptions = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Comma
        };
        var semicolonOptions = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Semicolon
        };
        var expression = "Abs(-5)"; // Single argument function

        var commaContext = new LogicalExpressionParseContext(expression, commaOptions);
        var semicolonContext = new LogicalExpressionParseContext(expression, semicolonOptions);

        // Act
        var commaResult = LogicalExpressionParser.Parse(commaContext);
        var semicolonResult = LogicalExpressionParser.Parse(semicolonContext);

        var commaValue = new Expression(commaResult).Evaluate(CancellationToken.None);
        var semicolonValue = new Expression(semicolonResult).Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(commaValue).IsEqualTo(5.0);
        await Assert.That(semicolonValue).IsEqualTo(5.0);
    }

    [Test]
    public async Task Should_Support_Multiple_Separators_In_One_Parser()
    {
        var argumentSeparators = ArgumentSeparator.Comma | ArgumentSeparator.Semicolon;

        // Arrange
        var argumentOptions = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = argumentSeparators
        };

        var commaExpression = "Max(1, 2)";
        var semicolonExpression = "Max(3; 4)";

        var commaContext = new LogicalExpressionParseContext(commaExpression, argumentOptions);

        var semicolonContext = new LogicalExpressionParseContext(semicolonExpression, argumentOptions);

        // Act
        var commaResult = LogicalExpressionParser.Parse(commaContext);
        var semicolonResult = LogicalExpressionParser.Parse(semicolonContext);

        var commaValue = new Expression(commaResult).Evaluate(CancellationToken.None);
        var semicolonValue = new Expression(semicolonResult).Evaluate(CancellationToken.None);

        // Assert
        await Assert.That(commaValue).IsEqualTo(2);
        await Assert.That(semicolonValue).IsEqualTo(4);
    }
}