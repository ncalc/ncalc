using NCalc.Factories;
using System.Threading.Tasks;

namespace NCalc.Tests;
[Property("Category", "DependencyInjection")]

[ClassDataSource<FactoriesFixture>(Shared = SharedType.PerClass)]
public class FactoriesTests(FactoriesFixture fixture)
{
    private readonly IExpressionFactory _expressionFactory = fixture.ExpressionFactory;
    private readonly ILogicalExpressionFactory _logicalExpressionFactory = fixture.LogicalExpressionFactory;

    [Test]
    public async Task Expression_From_Factory_Should_Evaluate()
    {
        await Assert.That(_expressionFactory.Create("2+2").Evaluate(CancellationToken.None)).IsEqualTo(4);
    }

    [Test]
    public async Task Expression_From_Factory_Should_Use_Specified_Culture()
    {
        var culture = CultureInfo.GetCultureInfo("de-DE");

        var expression = _expressionFactory.Create("[value] + 2", cultureInfo: culture);
        expression.Parameters["value"] = "1,5";

        await Assert.That(expression.CultureInfo).IsEqualTo(culture);
        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(3.5d);
    }

    [Test]
    public async Task Logical_Expression_From_Factory_Should_Evaluate()
    {
        await Assert.That(_expressionFactory.Create(_logicalExpressionFactory.Create("2+2", cancellationToken: CancellationToken.None))
            .Evaluate(CancellationToken.None)).IsEqualTo(4);
    }
}
