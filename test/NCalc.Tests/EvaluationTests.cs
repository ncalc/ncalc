#nullable enable
using NCalc.Exceptions;
using NCalc.Helpers;

namespace NCalc.Tests;

[Property("Category", "Evaluations")]
public class EvaluationTests
{
    [Test]
    public async Task ShouldApplyConfiguration()
    {
        var expression = new Expression("Round(22.5, 0)")
        {
            Configuration = new ExpressionConfiguration
            {
                Evaluation = new ExpressionEvaluationOptions
                {
                    Math = new MathOptions { MidpointRounding = MidpointRounding.AwayFromZero }
                }
            }
        };

        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(23d);
    }
}
