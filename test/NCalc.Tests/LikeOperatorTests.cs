namespace NCalc.Tests;
[Trait("Category", "Evaluations")]
public class LikeOperatorTests
{
    [Fact]
    public void ShouldMatchSingleCharacterUsingLike()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "A1B2C";

        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B2C'", context).Evaluate());
    }

    [Fact]
    public void ShouldNotMatchWhenSingleCharacterDoesNotMatch()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "A1B2C";

        Assert.Equal(false, new Expression("{LEP_COD_SAP_PROD} LIKE 'A_12C'", context).Evaluate());
    }

    [Fact]
    public void ShouldMatchAtStartUsingSingleCharacterWildcard()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "X12345";

        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} LIKE '_12345'", context).Evaluate());
    }

    [Fact]
    public void ShouldMatchAtEndUsingMultipleCharactersWildcard()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABCX23YZ";

        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} LIKE 'ABCX__YZ'", context).Evaluate());
    }

    [Fact]
    public void ShouldMatchMultipleWildcardsInString()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "A1B2C3D";

        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B_C_D'", context).Evaluate());
    }

    [Fact]
    public void ShouldNotMatchWhenStringLengthDiffers()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABC";

        Assert.Equal(false, new Expression("{LEP_COD_SAP_PROD} LIKE 'A_B'", context).Evaluate());
    }

        [Fact]
    public void ShouldEvaluateLikeOperatorWithWildcardAtEnd()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66ABC";
        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context).Evaluate());
    }

    [Fact]
    public void ShouldEvaluateLikeOperatorWithWildcardAtStart()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABC66";
        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} LIKE '%66'", context).Evaluate());
    }

    [Fact]
    public void ShouldEvaluateLikeOperatorWithWildcardAtBothEnds()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "ABC66XYZ";
        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} LIKE '%66%'", context).Evaluate());
    }

    [Fact]
    public void ShouldEvaluateLikeOperatorWithExactMatch()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66";
        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} LIKE '66'", context).Evaluate());
    }

    [Fact]
    public void ShouldEvaluateLikeOperatorWithNoMatch()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "77ABC";
        Assert.Equal(false, new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context).Evaluate());
    }

    [Fact]
    public void ShouldEvaluateNotLikeOperator()
    {
        var context = new ExpressionContext();
        context.StaticParameters["LEP_COD_SAP_PROD"] = "77ABC";
        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} NOT LIKE '66%'", context).Evaluate());
    }

    [Fact]
    public void LikeOperatorShouldRespectStringComparer()
    {
        ExpressionContext context = ExpressionOptions.CaseInsensitiveStringComparer;
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66ABC";
        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} LIKE '66%'", context).Evaluate());
        Assert.Equal(true, new Expression("{LEP_COD_SAP_PROD} LIKE '66abc%'", context).Evaluate());
    }

    [Fact]
    public async Task LikeOperatorShouldWorkAsync()
    {
        AsyncExpressionContext context = ExpressionOptions.CaseInsensitiveStringComparer;
        context.StaticParameters["LEP_COD_SAP_PROD"] = "66ABC";
        Assert.Equal(true, await new AsyncExpression("{LEP_COD_SAP_PROD} LIKE '66%'", context).EvaluateAsync());
        Assert.Equal(true, await new AsyncExpression("{LEP_COD_SAP_PROD} LIKE '66abc%'", context).EvaluateAsync());
    }
}