using NCalc.Tests.TestData;

namespace NCalc.Tests;

[Trait("Category","Async")]
public class AsyncTests
{
    [Theory]
    [ClassData(typeof(EvaluationTestData))]
    public async Task Expression_Should_Evaluate_Async(string expression, object expected)
    {
        Assert.Equal(expected, await new Expression(expression).EvaluateAsync());
    }
}