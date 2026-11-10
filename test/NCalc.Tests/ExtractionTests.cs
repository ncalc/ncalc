using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Extraction")]
public class ExtractionTests
{
    [Test]
    public async Task ShouldGetParametersIssue103()
    {
        var expression = new Expression("PageState == 'LIST' && a == 1 && customFunction() == true || in(1 + 1, 1, 2, 3)", ExpressionOptions.CaseInsensitiveStringComparer)
        {
            Parameters =
            {
                ["a"] = 1
            }
        };
        expression.DynamicParameters["PageState"] = _ => "List";
        expression.Functions["customfunction"] = _ => true;

        var parameters = expression.GetParameterNames(CancellationToken.None);
        await Assert.That(parameters).Contains("a");
        await Assert.That(parameters).Contains("PageState");
        await Assert.That(parameters.Count).IsEqualTo(2);
    }
}