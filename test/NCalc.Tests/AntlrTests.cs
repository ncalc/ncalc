using NCalc.Factories;
using NCalc.Tests.Fixtures;
using NCalc.Tests.TestData;

namespace NCalc.Tests;

[Property("Category", "Plugins")]
[ClassDataSource<FactoriesWithAntlrFixture>(Shared = SharedType.PerClass)]
public class AntlrTests(FactoriesWithAntlrFixture fixture)
{
    private IExpressionFactory ExpressionFactory { get; } = fixture.ExpressionFactory;

    [Test]
    [MethodDataSource<EvaluationTestData>(nameof(EvaluationTestData.GetTestData))]
    public async Task Expression_Should_Evaluate(string expression, object expected, CancellationToken cancellationToken)
    {
        await Assert.That(ExpressionFactory.Create(expression, ExpressionOptions.NoCache).Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource<ValuesTestData>(nameof(ValuesTestData.GetTestData))]
    public async Task Should_Parse_Values(string expressionString, object expectedValue, CancellationToken cancellationToken)
    {
        var expression = ExpressionFactory.Create(expressionString, ExpressionOptions.NoCache);
        var result = expression.Evaluate(cancellationToken);

        if (expectedValue is double expectedDouble)
        {
            await Assert.That((double)result).IsEqualTo(expectedDouble).Within(15);
        }
        else
        {
            await Assert.That(result).IsEqualTo(expectedValue);
        }
    }
}