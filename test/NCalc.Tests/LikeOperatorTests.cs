namespace NCalc.Tests;

[Property("Category", "Evaluations")]
public class LikeOperatorTests
{
    [Test]
    public async Task ShouldMatchSingleCharacterUsingLike(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "A1B2C";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B2C'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldNotMatchWhenSingleCharacterDoesNotMatch(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "A1B2C";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_12C'", context)
            .Evaluate(cancellationToken)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldMatchAtStartUsingSingleCharacterWildcard(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "X12345";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '_12345'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldMatchAtEndUsingMultipleCharactersWildcard(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABCX23YZ";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'ABCX__YZ'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldMatchMultipleWildcardsInString(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "A1B2C3D";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B_C_D'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldNotMatchWhenStringLengthDiffers(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABC";

        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B'", context)
            .Evaluate(cancellationToken)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithWildcardAtEnd(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithWildcardAtStart(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABC66";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '%66'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithWildcardAtBothEnds(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABC66XYZ";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '%66%'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithExactMatch(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateLikeOperatorWithNoMatch(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "77ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context)
            .Evaluate(cancellationToken)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldEvaluateNotLikeOperator(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "77ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} NOT LIKE '66%'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task LikeOperatorShouldRespectStringComparer(CancellationToken cancellationToken)
    {
        ExpressionContext context = ExpressionOptions.CaseInsensitiveStringComparer;
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66ABC";
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
        await Assert.That(new Expression("{LEP_COD_SAP_PROD} LIKE '66abc%'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task LikeOperatorShouldWorkAsync(CancellationToken cancellationToken)
    {
        AsyncExpressionContext context = ExpressionOptions.CaseInsensitiveStringComparer;
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66ABC";
        await Assert.That(await new AsyncExpression("{LEP_COD_SAP_PROD} LIKE '66%'", context)
            .EvaluateAsync(cancellationToken)).IsEqualTo(true);
        await Assert.That(await new AsyncExpression("{LEP_COD_SAP_PROD} LIKE '66abc%'", context)
            .EvaluateAsync(cancellationToken)).IsEqualTo(true);
    }
}