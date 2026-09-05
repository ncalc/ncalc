using System.Threading.Tasks;
using NCalc.Helpers;

namespace NCalc.Tests;

[Property("Category", "Evaluations")]
public class LikeOperatorTests
{
    [Test]
    public async Task ShouldMatchSingleCharacterUsingLike()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "A1B2C";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B2C'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldNotMatchWhenSingleCharacterDoesNotMatch()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "A1B2C";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_12C'", context))
            .Expression<bool>().IsFalse();
    }

    [Test]
    public async Task ShouldMatchAtStartUsingSingleCharacterWildcard()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "X12345";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '_12345'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldMatchAtEndUsingMultipleCharactersWildcard()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "ABCX23YZ";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'ABCX__YZ'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldMatchMultipleWildcardsInString()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "A1B2C3D";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B_C_D'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldNotMatchWhenStringLengthDiffers()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "ABC";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B'", context))
            .Expression<bool>().IsFalse();
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithWildcardAtEnd()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "66ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithWildcardAtStart()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "ABC66";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '%66'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithWildcardAtBothEnds()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "ABC66XYZ";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '%66%'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithExactMatch()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "66";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithNoMatch()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "77ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context))
            .Expression<bool>().IsFalse();
    }

    [Test]
    public async Task ShouldEvaluateNotLikeOperator()
    {
        var context = new ExpressionContext();
        context.Parameters["LEP_COD_SAP_PROD"] = "77ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} NOT LIKE '66%'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldMatchEscapedPercentUsingLike()
    {
        var context = new ExpressionContext();
        context.Parameters["Value"] = "100%";

        await Assert.That(new Expression(@"{Value} LIKE '%\%'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldMatchEscapedUnderscoreUsingLike()
    {
        var context = new ExpressionContext();
        context.Parameters["Value"] = "Hello_world";

        await Assert.That(new Expression(@"{Value} LIKE '%\_%'", context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task ShouldNotTreatEscapedWildcardsAsWildcardsUsingLike()
    {
        var context = new ExpressionContext();

        context.Parameters["Value"] = "1000";
        await Assert.That(new Expression(@"{Value} LIKE '%\%'", context))
            .Expression<bool>().IsFalse();

        context.Parameters["Value"] = "Hello-world";
        await Assert.That(new Expression(@"{Value} LIKE '%\_%'", context))
            .Expression<bool>().IsFalse();
    }

    [Test]
    public async Task ShouldEscapeLikePattern()
    {
        await Assert.That(new Expression(@"EscapeLike('100%')")
            .Evaluate(CancellationToken.None)).IsEqualTo(@"100\%");
        await Assert.That(new Expression(@"EscapeLike('Hello_world')")
            .Evaluate(CancellationToken.None)).IsEqualTo(@"Hello\_world");
        await Assert.That(LikeOperatorHelper.EscapeLike(@"C:\Temp_100%")).IsEqualTo(@"C:\\Temp\_100\%");
    }

    [Test]
    public async Task ShouldMatchEscapedUserInputUsingLike()
    {
        var context = new ExpressionContext();
        context.Parameters["Value"] = "Hello_world";

        await Assert.That(new Expression("{Value} LIKE EscapeLike('Hello_world')", context))
            .Expression<bool>().IsTrue();
        await Assert.That(new Expression("{Value} LIKE EscapeLike('Hello%world')", context))
            .Expression<bool>().IsFalse();
    }

    [Test]
    public async Task LikeOperatorShouldRespectStringComparer()
    {
        var context = new ExpressionContext();
        var configuration = ExpressionConfiguration.FromOptions(ExpressionOptions.CaseInsensitiveStringComparer);
        context.Parameters["LEP_COD_SAP_PROD"] = "66ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", configuration, context))
            .Expression<bool>().IsTrue();
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66abc%'", configuration, context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task LikeOperatorShouldWorkAcrossCodepointBoundaries()
    {
        var context = new ExpressionContext();
        var configuration = ExpressionConfiguration.FromOptions(ExpressionOptions.CaseInsensitiveStringComparer);
        const string source = "Cafe\u0301";
        const string pattern = "CAF\u00C9";

        context.Parameters["source"] = source;
        context.Parameters["pattern"] = pattern;

        await Assert.That(new Expression("source LIKE pattern", configuration, context))
            .Expression<bool>().IsTrue();
    }

    [Test]
    public async Task LikeOperatorShouldWorkAsync()
    {
        var context = new ExpressionContext();
        var configuration = ExpressionConfiguration.FromOptions(ExpressionOptions.CaseInsensitiveStringComparer);
        context.Parameters["LEP_COD_SAP_PROD"] = "66ABC";
        await Assert.That(await new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", configuration, context)
            .EvaluateAsync<bool>(CancellationToken.None)).IsTrue();
        await Assert.That(await new Expression("{LEP_COD_SAP_PROD} LIKE '66abc%'", configuration, context)
            .EvaluateAsync<bool>(CancellationToken.None)).IsTrue();
    }
}
