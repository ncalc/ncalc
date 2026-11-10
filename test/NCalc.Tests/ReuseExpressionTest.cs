using System.Threading.Tasks;
using NCalc.Exceptions;

namespace NCalc.Tests
{
    public class ReuseExpressionTest
    {
        [Test]
        public async Task ShouldAllowReuseAfterFail()
        {
            // Other culture tests cache this same text; exercise recovery from an actual parse failure.
            var e = new Expression("#27.05.2025 12:00:00#", ExpressionOptions.NoCache, null, CultureInfo.InvariantCulture);

            Assert.Throws<NCalcParserException>(() => e.Evaluate(CancellationToken.None));
            await Assert.That(e.HasErrors(CancellationToken.None)).IsTrue();

            e.CultureInfo = CultureInfo.GetCultureInfo("ru-RU");

            var result = e.Evaluate(CancellationToken.None);
            await Assert.That(result).IsEqualTo(new DateTime(2025, 5, 27, 12, 0, 0));
            await Assert.That(e.HasErrors(CancellationToken.None)).IsFalse();
        }

        [Test]
        public async Task HasErrorsShouldReturnCorrectResultAfterFail()
        {
            var e = new Expression("#27.05.2025 12:00:00#", ExpressionOptions.None,null, CultureInfo.InvariantCulture);
            await Assert.That(e.HasErrors(CancellationToken.None)).IsTrue();

            e.CultureInfo = CultureInfo.GetCultureInfo("ru-RU");
            await Assert.That(e.HasErrors(CancellationToken.None)).IsFalse();
        }
    }
}