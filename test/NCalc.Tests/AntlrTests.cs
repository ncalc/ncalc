using NCalc.Factories;
using System.Threading.Tasks;

namespace NCalc.Tests;
[Property("Category", "Plugins")]

[ClassDataSource<FactoriesWithAntlrFixture>(Shared = SharedType.PerClass)]
public class AntlrTests(FactoriesWithAntlrFixture fixture)
{
    private IExpressionFactory ExpressionFactory { get; } = fixture.ExpressionFactory;

    [Test]
    [MethodDataSource(typeof(EvaluationTestData), "GetEnumerator")]
    public async Task Expression_Should_Evaluate(string expression, object expected)
    {
        await Assert.That(ExpressionFactory.Create(expression, ExpressionOptions.NoCache).Evaluate(CancellationToken.None)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(typeof(ValuesTestData), "GetEnumerator")]
    public async Task Should_Parse_Values(string expressionString, object expectedValue)
    {
        var expression = ExpressionFactory.Create(expressionString, ExpressionOptions.NoCache);
        var result = expression.Evaluate(CancellationToken.None);

        if (expectedValue is double expectedDouble)
        {
            // TODO: TUnit migration - xUnit Assert.Equal had additional argument(s) (precision: 15) that could not be converted.
            await Assert.That((double)result).IsEqualTo(expectedDouble);
        }
        else
        {
            await Assert.That(result).IsEqualTo(expectedValue);
        }
    }
}