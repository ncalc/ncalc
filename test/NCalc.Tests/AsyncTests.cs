using NCalc.Tests.TestData;

namespace NCalc.Tests;

[Trait("Category","Async")]
public class AsyncTests
{
    [Theory]
    [ClassData(typeof(EvaluationTestData))]
    public async Task ShouldEvaluateAsync(string expression, object expected)
    {
        Assert.Equal(expected, await new Expression(expression).EvaluateAsync());
    }

    [Fact]
    public async Task ShouldEvaluateAsyncFunction()
    {
        var expression = new Expression("database_operation('SELECT FOO') == 'FOO'");
        expression.EvaluateFunctionAsync += async (name, args) =>
        {
            if (name == "database_operation")
            {
                //My heavy database work.
                await Task.Delay(100);

                args.Result = "FOO";
            }
        };
        
        var result = await expression.EvaluateAsync();
        Assert.Equal(true,result);
    }
    
    [Fact]
    public async Task ShouldEvaluateArrayParameters()
    {
        var e = new Expression("x * x", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0, 1, 2, 3, 4 }
            }
        };

        var result = (IList<object>)await e.EvaluateAsync();

        Assert.NotNull(result);
        Assert.Equal(0, result[0]);
        Assert.Equal(1, result[1]);
        Assert.Equal(4, result[2]);
        Assert.Equal(9, result[3]);
        Assert.Equal(16, result[4]);
    }
}