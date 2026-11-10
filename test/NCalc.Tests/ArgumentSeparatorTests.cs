using NCalc.Exceptions;
using System.Threading.Tasks;

namespace NCalc.Tests;

public class ArgumentSeparatorTests
{
    [Test]
    public async Task ShouldKeepSeparatorOptionsIndependentAcrossAlternatingCalls()
    {
        var culture = CultureInfo.InvariantCulture;
        var commaOptions = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Comma
        };
        var semicolonOptions = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Semicolon
        };
        var otherCommaOptions = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Comma
        };

        for (var i = 0; i < 2; i++)
        {
            var commaResult = LogicalExpressionParser.Parse("Max(1, 2)", commaOptions, culture);
            await Assert.That(new Expression(commaResult).Evaluate(CancellationToken.None)).IsEqualTo(2);
            Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse("Max(1; 2)", commaOptions, culture));

            var semicolonResult = LogicalExpressionParser.Parse("Max(3; 4)", semicolonOptions, culture);
            await Assert.That(new Expression(semicolonResult).Evaluate(CancellationToken.None)).IsEqualTo(4);
            Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse("Max(3, 4)", semicolonOptions, culture));

            var otherCommaResult = LogicalExpressionParser.Parse("Max(5, 6)", otherCommaOptions, culture);
            await Assert.That(new Expression(otherCommaResult).Evaluate(CancellationToken.None)).IsEqualTo(6);
            Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse("Max(5; 6)", otherCommaOptions, culture));
        }
    }
}