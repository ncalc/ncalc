using NCalc.Factories;
using NCalc.Tests.Fixtures;

namespace NCalc.Tests;

[Trait("Category", "Plugins")]
public class MemoryCacheTests(FactoriesWithMemoryCacheFixture fixture) : IClassFixture<FactoriesWithMemoryCacheFixture>
{
    private readonly IExpressionFactory _expressionFactory = fixture.ExpressionFactory;

    [Fact]
    public void Logical_Expression_Without_Cache_Should_Not_Be_The_Same()
    {
        var expression = _expressionFactory.Create("'Sergio' != 'Bella'");

        Assert.Equal(true, expression.Evaluate());

        var anotherExpression = _expressionFactory.Create("'Sergio' != 'Bella'", ExpressionOptions.NoCache);

        Assert.Equal(true, anotherExpression.Evaluate());

        Assert.NotEqual(expression.LogicalExpression, anotherExpression.LogicalExpression);
    }
}