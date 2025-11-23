namespace NCalc.Tests;

[Trait("Category", "Custom Culture")]
public class CustomCultureTests
{
    [Fact]
    public void ShouldCorrectlyParseCustomCultureParameter()
    {
        var cultureDot = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultureDot.NumberFormat.NumberGroupSeparator = " ";
        var cultureComma = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultureComma.NumberFormat.NumberDecimalSeparator = ",";
        cultureComma.NumberFormat.NumberGroupSeparator = " ";

        //use 1*[A] to avoid evaluating expression parameters as string - force numeric conversion
        ExecuteTest("1*[A]-[B]", 1.5);
        ExecuteTest("1*[A]+[B]", 2.5);
        ExecuteTest("1*[A]/[B]", 4d);
        ExecuteTest("1*[A]*[B]", 1d);
        ExecuteTest("1*[A]>[B]", true);
        ExecuteTest("1*[A]<[B]", false);

        void ExecuteTest(string formula, object expectedValue)
        {
            //Correctly evaluate with decimal dot culture and parameter with dot
            Assert.Equal(expectedValue, new Expression(formula, cultureDot)
            {
                Parameters =
                {
                    {"A","2.0"},
                    {"B","0.5"}
                }
            }.Evaluate());

            //Correctly evaluate with decimal comma and parameter with comma
            Assert.Equal(expectedValue, new Expression(formula, cultureComma)
            {
                Parameters =
                {
                    {"A","2,0"},
                    {"B","0,5"}
                }
            }.Evaluate(TestContext.Current.CancellationToken));

            //combining decimal dot and comma fails
            Assert.Throws<FormatException>(() => new Expression(formula, cultureComma)
            {
                Parameters =
                {
                    {"A","2,0"},
                    {"B","0.5"}
                }
            }.Evaluate(TestContext.Current.CancellationToken));

            //combining decimal dot and comma fails
            Assert.Throws<FormatException>(() => new Expression(formula, cultureDot)
            {
                Parameters =
                {
                    {"A","2,0"},
                    {"B","0.5"}
                }
            }.Evaluate());
        }
    }

    [Fact]
    public void ShouldParseInvariantCulture()
    {
        var originalCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        try
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ",";
            Thread.CurrentThread.CurrentCulture = culture;
            var exceptionThrown = false;
            try
            {
                var expr = new Expression("[a]<2.0") { Parameters = { ["a"] = "1.7" } };
                expr.Evaluate(TestContext.Current.CancellationToken);
            }
            catch (FormatException)
            {
                exceptionThrown = true;
            }

            Assert.True(exceptionThrown);

            var e = new Expression("[a]<2.0", CultureInfo.InvariantCulture);
            e.Parameters["a"] = "1.7";
            Assert.Equal(true, e.Evaluate(TestContext.Current.CancellationToken));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
    [Fact]
    public void ShouldConvertToStringUsingCultureInfo()
    {
        var originalCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        try
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ",";
            Thread.CurrentThread.CurrentCulture = culture;
            var context = new ExpressionContext(
                ExpressionOptions.StringConcat,
                CultureInfo.InvariantCulture);
            var expr = new Expression("[a] + 2.5", context)
            {
                Parameters = { ["a"] = 1.7 }
            };
            Assert.Equal("1.72.5", expr.Evaluate(TestContext.Current.CancellationToken));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
}