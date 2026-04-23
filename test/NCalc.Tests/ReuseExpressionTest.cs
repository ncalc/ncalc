using System.Threading.Tasks;

namespace NCalc.Tests
{
    public class ReuseExpressionTest
    {
        [Test]
        public async Task ShouldAllowReuseAfterFail()
        {
            var e = new Expression("#27.05.2025 12:00:00#", ExpressionOptions.None, CultureInfo.InvariantCulture);

            try
            {
                e.Evaluate(CancellationToken.None);
                Assert.Fail("Assertion failure");
            }
            catch (Exception)
            {
                await Assert.That(e.HasErrors(CancellationToken.None)).IsTrue();

                e.CultureInfo = CultureInfo.GetCultureInfo("ru-RU");

                var res = e.Evaluate(CancellationToken.None);

                var expected = new DateTime(2025, 05, 27, 12, 0, 0);
                await Assert.That(res).IsEqualTo(expected);
                await Assert.That(e.HasErrors(CancellationToken.None)).IsFalse();
            }
        }

        [Test]
        public async Task HasErrorsShouldReturnCorrectResultAfterFail()
        {
            var e = new Expression("#27.05.2025 12:00:00#", ExpressionOptions.None, CultureInfo.InvariantCulture);
            await Assert.That(e.HasErrors(CancellationToken.None)).IsTrue();

            e.CultureInfo = CultureInfo.GetCultureInfo("ru-RU");
            await Assert.That(e.HasErrors(CancellationToken.None)).IsFalse();
        }
    }
}