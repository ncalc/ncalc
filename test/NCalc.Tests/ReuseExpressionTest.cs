namespace NCalc.Tests
{
    public class ReuseExpressionTest
    {
        [Fact]
        public void ShouldAllowReuseAfterFail()
        {
            var e = new Expression("#27.05.2025 12:00:00#", ExpressionOptions.None, CultureInfo.InvariantCulture);

            try
            {
                e.Evaluate(TestContext.Current.CancellationToken);
                Assert.Fail("The expression should fail with specified culture and format");
            }
            catch (Exception)
            {
                Assert.True(e.HasErrors(TestContext.Current.CancellationToken));

                e.CultureInfo = CultureInfo.GetCultureInfo("ru-RU");

                var res = e.Evaluate(TestContext.Current.CancellationToken);

                var expected = new DateTime(2025, 05, 27, 12, 0, 0);
                Assert.Equal(expected, res);
                Assert.False(e.HasErrors(TestContext.Current.CancellationToken));
            }
        }

        [Fact]
        public void HasErrorsShouldReturnCorrectResultAfterFail()
        {
            var e = new Expression("#27.05.2025 12:00:00#", ExpressionOptions.None, CultureInfo.InvariantCulture);
            Assert.True(e.HasErrors(TestContext.Current.CancellationToken));

            e.CultureInfo = CultureInfo.GetCultureInfo("ru-RU");
            Assert.False(e.HasErrors(TestContext.Current.CancellationToken));
        }
    }
}
