using NCalc.Exceptions;
using NCalc.Parser;
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
    public async Task Should_Parse_Functions_With_Different_Separators(string expression, double expected, ArgumentSeparator separator, CancellationToken cancellationToken)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(cancellationToken);

        // Assert
        await Assert.That(Convert.ToDouble(evaluationResult)).IsEqualTo(expected).Within(2);
    }

    [Test]
    [Arguments("if(true, 'yes', 'no')", "yes", ArgumentSeparator.Comma)]
    [Arguments("if(true; 'yes'; 'no')", "yes", ArgumentSeparator.Semicolon)]
    [Arguments("if(1 > 2, 10, 20)", 20, ArgumentSeparator.Comma)]
    [Arguments("if(1 > 2; 10; 20)", 20, ArgumentSeparator.Semicolon)]
    public async Task Should_Parse_Conditional_Functions_With_Different_Separators(string expression, object expected, ArgumentSeparator separator, CancellationToken cancellationToken)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(cancellationToken);

        // Assert
        await Assert.That(evaluationResult).IsEqualTo(expected);
    }

    [Test]
    [Arguments("Max(1, 2)", ArgumentSeparator.Semicolon)] // Using comma in expression but semicolon separator
    [Arguments("Max(1; 2)", ArgumentSeparator.Comma)] // Using semicolon in expression but comma separator
    [Arguments("Min(1, 2)", ArgumentSeparator.Semicolon)] // Multiple arguments with wrong separator
    [Arguments("Round(3.14; 2)", ArgumentSeparator.Comma)] // Different function with wrong separator
    public async Task Should_Throw_Exception_With_Incorrect_SeparatorAsync(string expression, ArgumentSeparator separator)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act & Assert
        await Assert.That(() => LogicalExpressionParser.Parse(context)).ThrowsExactly<NCalcParserException>();
    }

    [Test]
    public async Task Should_Support_Mixed_Separators_In_Different_Parsers(CancellationToken cancellationToken)
    {
        // Arrange
        var commaOptions = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Comma);
        var semicolonOptions = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);

        const string commaExpression = "Max(1, 2)";
        const string semicolonExpression = "Max(3; 4)";

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

        var commaValue = new Expression(commaResult).Evaluate(cancellationToken);
        var semicolonValue = new Expression(semicolonResult).Evaluate(cancellationToken);

        // Assert
        await Assert.That(commaValue).IsEqualTo(2);
        await Assert.That(semicolonValue).IsEqualTo(4);
    }

    [Test]
    public async Task Should_Cache_Parsers_For_Different_Separator_Options()
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
        await Assert.That(parser2).IsNotSameReferenceAs(parser1); // Different separators should have different parsers
        await Assert.That(parser3).IsSameReferenceAs(parser1); // Same options should return cached parser
    }

    [Test]
    public async Task Should_Support_Culture_And_Separator_Combination(CancellationToken cancellationToken)
    {
        // Arrange
        var germanCulture = new CultureInfo("de-DE");
        var options = LogicalExpressionParserOptions.Create(germanCulture, ArgumentSeparator.Semicolon);
        const string expression = "Max(1.5; 2.3)"; // Using dots for decimals to avoid confusion with argument separator

        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(cancellationToken);

        // Assert
        await Assert.That(Convert.ToDouble(evaluationResult)).IsEqualTo(2.3).Within(1);
    }

    [Test]
    [Arguments(ArgumentSeparator.Comma)]
    [Arguments(ArgumentSeparator.Semicolon)]
    [Arguments(ArgumentSeparator.Colon)]
    public async Task Should_Support_Various_Separator_Characters(ArgumentSeparator separator, CancellationToken cancellationToken)
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
        var evaluationResult = new Expression(result).Evaluate(cancellationToken);

        // Assert
        await Assert.That(Convert.ToDouble(evaluationResult))
                             .IsEqualTo(3.0)
                             .Within(0.0001);
    }

    [Test]
    public async Task Should_Maintain_Backward_Compatibility_With_Default_Comma_Separator(CancellationToken cancellationToken)
    {
        // Arrange
        const string expression = "Max(2, 3)";
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None);
        // Not setting ParserOptions should default to comma separator

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(cancellationToken);

        // Assert
        await Assert.That(Math.Abs(Convert.ToDouble(evaluationResult))).IsEqualTo(3.0).Within(0.0001);
    }

    [Test]
    public async Task Should_Support_Semicolon_Separator_With_Explicit_Options(CancellationToken cancellationToken)
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
        var evaluationResult = new Expression(result).Evaluate(cancellationToken);

        // Assert
        await Assert.That(Math.Abs(Convert.ToDouble(evaluationResult))).IsEqualTo(3.0).Within(0.0001);
    }

    [Test]
    public async Task Should_Support_Nested_Functions_With_Custom_Separator(CancellationToken cancellationToken)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        const string expression = "Max(Min(1; 2); Max(3; 4))";

        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(cancellationToken);

        // Assert
        await Assert.That(evaluationResult).IsEqualTo(4);
    }

    [Test]
    [Arguments("Max(1 , 2)", ArgumentSeparator.Comma)] // Spaces around separator
    [Arguments("Max(1 ; 2)", ArgumentSeparator.Semicolon)]
    [Arguments("Max( 1, 2 )", ArgumentSeparator.Comma)]  // Spaces around arguments
    [Arguments("Max( 1; 2 )", ArgumentSeparator.Semicolon)]
    public async Task Should_Handle_Whitespace_Around_Separators(string expression, ArgumentSeparator separator, CancellationToken cancellationToken)
    {
        // Arrange
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(separator);
        var context = new LogicalExpressionParserContext(expression, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        // Act
        var result = LogicalExpressionParser.Parse(context);
        var evaluationResult = new Expression(result).Evaluate(cancellationToken);

        // Assert
        await Assert.That(evaluationResult).IsEqualTo(2);
    }

    [Test]
    public async Task Should_Handle_Single_Argument_Functions_Regardless_Of_Separator(CancellationToken cancellationToken)
    {
        // Arrange
        var commaOptions = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Comma);
        var semicolonOptions = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        const string expression = "Abs(-5)"; // Single argument function

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

        var commaValue = new Expression(commaResult).Evaluate(cancellationToken);
        var semicolonValue = new Expression(semicolonResult).Evaluate(cancellationToken);

        // Assert
        await Assert.That(commaValue).IsEqualTo(5.0);
        await Assert.That(semicolonValue).IsEqualTo(5.0);
    }
}