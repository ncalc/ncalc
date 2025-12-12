namespace NCalc.Tests;

[Property("Category", "Custom Culture")]
public class CustomCultureTests
{
    [Test]
    public async Task ShouldCorrectlyParseCustomCultureParameter(CancellationToken cancellationToken)
    {
        var cultureDot = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultureDot.NumberFormat.NumberGroupSeparator = " ";
        var cultureComma = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultureComma.NumberFormat.NumberDecimalSeparator = ",";
        cultureComma.NumberFormat.NumberGroupSeparator = " ";

        //use 1*[A] to avoid evaluating expression parameters as string - force numeric conversion
        await ExecuteTestAsync("1*[A]-[B]", 1.5);
        await ExecuteTestAsync("1*[A]+[B]", 2.5);
        await ExecuteTestAsync("1*[A]/[B]", 4d);
        await ExecuteTestAsync("1*[A]*[B]", 1d);
        await ExecuteTestAsync("1*[A]>[B]", true);
        await ExecuteTestAsync("1*[A]<[B]", false);

        async Task ExecuteTestAsync(string formula, object expectedValue)
        {
            //Correctly evaluate with decimal dot culture and parameter with dot
            await Assert.That(new Expression(formula, cultureDot)
            {
                Parameters =
                {
                    {"A","2.0"},
                    {"B","0.5"}
                }
            }.Evaluate(cancellationToken)).IsEqualTo(expectedValue);

            //Correctly evaluate with decimal comma and parameter with comma
            await Assert.That(new Expression(formula, cultureComma)
            {
                Parameters =
                {
                    {"A","2,0"},
                    {"B","0,5"}
                }
            }.Evaluate(cancellationToken)).IsEqualTo(expectedValue);

            //combining decimal dot and comma fails
            await Assert.That(() => new Expression(formula, cultureComma)
            {
                Parameters =
                {
                    {"A","2,0"},
                    {"B","0.5"}
                }
            }.Evaluate(cancellationToken)).ThrowsExactly<FormatException>();

            //combining decimal dot and comma fails
            await Assert.That(() => new Expression(formula, cultureDot)
            {
                Parameters =
                {
                    {"A","2,0"},
                    {"B","0.5"}
                }
            }.Evaluate(cancellationToken)).ThrowsExactly<FormatException>();
        }
    }

    [Test]
    public async Task ShouldParseInvariantCulture(CancellationToken cancellationToken)
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
                expr.Evaluate(cancellationToken);
            }
            catch (FormatException)
            {
                exceptionThrown = true;
            }

            await Assert.That(exceptionThrown).IsTrue();

            var e = new Expression("[a]<2.0", CultureInfo.InvariantCulture);
            e.Parameters["a"] = "1.7";
            await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(true);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
    [Test]
    public async Task ShouldConvertToStringUsingCultureInfo(CancellationToken cancellationToken)
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
            await Assert.That(expr.Evaluate(cancellationToken)).IsEqualTo("1.72.5");
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
}