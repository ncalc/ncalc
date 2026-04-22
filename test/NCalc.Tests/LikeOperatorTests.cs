using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Evaluations")]
public class LikeOperatorTests
{
    [Test]
    public async Task ShouldMatchSingleCharacterUsingLike()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "A1B2C";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B2C'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldNotMatchWhenSingleCharacterDoesNotMatch()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "A1B2C";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_12C'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldMatchAtStartUsingSingleCharacterWildcard()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "X12345";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '_12345'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldMatchAtEndUsingMultipleCharactersWildcard()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABCX23YZ";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'ABCX__YZ'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldMatchMultipleWildcardsInString()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "A1B2C3D";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B_C_D'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldNotMatchWhenStringLengthDiffers()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABC";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithWildcardAtEnd()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithWildcardAtStart()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABC66";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '%66'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithWildcardAtBothEnds()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABC66XYZ";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '%66%'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithExactMatch()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithNoMatch()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "77ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldEvaluateNotLikeOperator()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "77ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} NOT LIKE '66%'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task LikeOperatorShouldRespectStringComparer()
    {
        ExpressionContext context = ExpressionOptions.CaseInsensitiveStringComparer;
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66abc%'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task LikeOperatorShouldWorkAsync()
    {
        AsyncExpressionContext context = ExpressionOptions.CaseInsensitiveStringComparer;
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66ABC";
        await Assert.That(await new AsyncExpression("{LEP_COD_SAP_PROD} LIKE '66%'", context)
            .EvaluateAsync(CancellationToken.None)).IsEqualTo(true);
        await Assert.That(await new AsyncExpression("{LEP_COD_SAP_PROD} LIKE '66abc%'", context)
            .EvaluateAsync(CancellationToken.None)).IsEqualTo(true);
    }
}