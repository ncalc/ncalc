using NCalc.Exceptions;
using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "DateTime")]
public class DateTimeTests
{
    [Test]
    public async Task ShouldHandleDateRuntimeCultureChange()
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        try
        {
            var expr = new Expression("#05/27/2025#", ExpressionOptions.None);
            var res = expr.Evaluate(CancellationToken.None);

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            var expr2 = new Expression("#27.05.2025#");
            var res2 = expr2.Evaluate(CancellationToken.None);

            var dt = new DateTime(2025, 05, 27);

            await Assert.That(res).IsEqualTo(dt);
            await Assert.That(res2).IsEqualTo(dt);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Test]
    public async Task ShouldHandleRuntimeCultureChange()
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        try
        {
            var expr = new Expression("#05/27/2025 12:00:00#", ExpressionOptions.None);
            var res = expr.Evaluate(CancellationToken.None);

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            var expr2 = new Expression("#27.05.2025 12:00:00#");
            var res2 = expr2.Evaluate(CancellationToken.None);

            var dt = new DateTime(2025, 05, 27, 12, 0, 0);

            await Assert.That(res).IsEqualTo(dt);
            await Assert.That(res2).IsEqualTo(dt);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Test]
    public async Task ShouldHandleSpecifiedExpressionCulture()
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        try
        {
            var expr = new Expression("#05/27/2025 12:00:00#", ExpressionOptions.None);
            var res = expr.Evaluate(CancellationToken.None);

            var ruCulture = CultureInfo.GetCultureInfo("ru-RU");
            var expr2 = new Expression("#27.05.2025 12:00:00#", ExpressionOptions.None, null,ruCulture);
            var res2 = expr2.Evaluate(CancellationToken.None);

            var dt = new DateTime(2025, 05, 27, 12, 0, 0);

            await Assert.That(res).IsEqualTo(dt);
            await Assert.That(res2).IsEqualTo(dt);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }
}