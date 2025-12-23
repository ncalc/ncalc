using NCalc.Factories;
using NCalc.Tests.Fixtures;

namespace NCalc.Tests;

[Property("Category", "DependencyInjection")]
[ClassDataSource<FactoriesFixture>(Shared = SharedType.PerClass)]
public class FactoriesTests(FactoriesFixture fixture)
{
    private readonly IExpressionFactory _expressionFactory = fixture.ExpressionFactory;
    private readonly ILogicalExpressionFactory _logicalExpressionFactory = fixture.LogicalExpressionFactory;

    [Test]
    public async Task Expression_From_Factory_Should_Evaluate(CancellationToken cancellationToken)
    {
        await Assert.That(_expressionFactory.Create("2+2").Evaluate(cancellationToken)).IsEqualTo(4);
    }

    [Test]
    public async Task Logical_Expression_From_Factory_Should_Evaluate(CancellationToken cancellationToken)
    {
        await Assert.That(_expressionFactory.Create(_logicalExpressionFactory.Create("2+2", ct: cancellationToken))
            .Evaluate(cancellationToken)).IsEqualTo(4);
    }
}