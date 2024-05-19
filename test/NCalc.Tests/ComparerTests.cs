using Xunit;

namespace NCalc.Tests;

[Trait("Category","Comparer")]
public class ComparerTests
{
    [Fact]
    public void Should_Use_Case_Insensitive_Comparer_Issue_85()
    {
        var eif = new Expression("PageState == 'LIST'", ExpressionOptions.CaseInsensitiveComparer);
        eif.Parameters["PageState"] = "List";

        Assert.True((bool)eif.Evaluate());
    }
    
    [Theory]
    [InlineData(null, 2, false)]
    [InlineData(2, 2L, true)]
    [InlineData("Hello", "World", false)]
    public void Compare_Using_Most_Precise_Type_Issue_102(object a, object b, bool expectedResult)
    {
        var issueExp = new Expression("a == b")
        {
            Parameters =
            {
                ["a"] = a,
                ["b"] = b
            }
        };

        Assert.Equal(expectedResult, (bool)issueExp.Evaluate());
    }
}