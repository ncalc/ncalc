using NCalc.Factories;

namespace NCalc.Tests;

[Trait("Category", "Plugins")]
public class MemoryCacheTests(FactoriesWithMemoryCacheFixture fixture) : IClassFixture<FactoriesWithMemoryCacheFixture>
{
    private readonly IExpressionFactory _expressionFactory = fixture.ExpressionFactory;

    [Fact]
    public void Logical_Expression_Without_Cache_Should_Not_Be_The_Same()
    {
        var expression = _expressionFactory.Create("'Sergio' != 'Bella'");

        Assert.Equal(true, expression.Evaluate(TestContext.Current.CancellationToken));

        var anotherExpression = _expressionFactory.Create("'Sergio' != 'Bella'", ExpressionOptions.NoCache);

        Assert.Equal(true, anotherExpression.Evaluate(TestContext.Current.CancellationToken));

        Assert.NotEqual(expression.LogicalExpression, anotherExpression.LogicalExpression);
    }
}