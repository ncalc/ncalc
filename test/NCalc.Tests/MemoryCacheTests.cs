using NCalc.Factories;
using NCalc.Tests.Fixtures;

namespace NCalc.Tests;

[Property("Category", "Plugins")]
[ClassDataSource<FactoriesWithMemoryCacheFixture>(Shared = SharedType.PerClass)]
public class MemoryCacheTests(FactoriesWithMemoryCacheFixture fixture)
{
    private readonly IExpressionFactory _expressionFactory = fixture.ExpressionFactory;

    [Test]
    public async Task Logical_Expression_Without_Cache_Should_Not_Be_The_Same(CancellationToken cancellationToken)
    {
        var expression = _expressionFactory.Create("'Sergio' != 'Bella'");

        await Assert.That(expression.Evaluate(cancellationToken)).IsEqualTo(true);

        var anotherExpression = _expressionFactory.Create("'Sergio' != 'Bella'", ExpressionOptions.NoCache);

        await Assert.That(anotherExpression.Evaluate(cancellationToken)).IsEqualTo(true);

        await Assert.That(anotherExpression.LogicalExpression).IsNotEqualTo(expression.LogicalExpression);
    }
}