using NCalc.Factories;
using System.Threading.Tasks;

namespace NCalc.Tests;
[Property("Category", "Plugins")]

[ClassDataSource<FactoriesWithMemoryCacheFixture>(Shared = SharedType.PerClass)]
public class MemoryCacheTests(FactoriesWithMemoryCacheFixture fixture)
{
    private readonly IExpressionFactory _expressionFactory = fixture.ExpressionFactory;

    [Test]
    public async Task Logical_Expression_Without_Cache_Should_Not_Be_The_Same()
    {
        var expression = _expressionFactory.Create("'Sergio' != 'Bella'");

        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(true);

        var anotherExpression = _expressionFactory.Create("'Sergio' != 'Bella'", ExpressionOptions.NoCache);

        await Assert.That(anotherExpression.Evaluate(CancellationToken.None)).IsEqualTo(true);

        await Assert.That(anotherExpression.LogicalExpression).IsNotEqualTo(expression.LogicalExpression);
    }
}