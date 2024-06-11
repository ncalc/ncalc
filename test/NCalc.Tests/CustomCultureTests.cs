using System.Globalization;

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
        cultureComma.NumberFormat.CurrencyDecimalSeparator = ",";
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
                Parameters = new Dictionary<string, object>
                {
                    {"A","2.0"},
                    {"B","0.5"}
                }
            }.Evaluate());

            //Correctly evaluate with decimal comma and parameter with comma
            Assert.Equal(expectedValue, new Expression(formula, cultureComma)
            {
                Parameters = new Dictionary<string, object>
                {
                    {"A","2.0"},
                    {"B","0.5"}
                }
            }.Evaluate());

            //combining decimal dot and comma fails
            Assert.Throws<FormatException>(() => new Expression(formula, cultureComma)
            {
                Parameters = new Dictionary<string, object>
                {
                    {"A","2,0"},
                    {"B","0.5"}
                }
            }.Evaluate());

            //combining decimal dot and comma fails
            Assert.Throws<FormatException>(() => new Expression(formula, cultureDot)
            {
                Parameters = new Dictionary<string, object>
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
        var e = new Expression("[a]<2.0", CultureInfo.InvariantCulture);
        e.Parameters["a"] = "1.7";
        Assert.Equal(true, e.Evaluate());
    }

    [Fact]
    public void ShouldParseCultureWithCommaSeparator()
    {
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ",";

        var e = new Expression("[a]<2,0", culture);
        e.Parameters["a"] = "1,7";
        Assert.Equal(true, e.Evaluate());
    }

    [Fact]
    public void ShouldParseFunctionParameters()
    {
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ",";

        var e = new Expression("Round(3,123,0)", culture);
        Assert.Equal(3d, e.Evaluate());
    }

    [Fact]
    public void PiTest()
    {
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ",";

        var e = new Expression("if([Pi] > 3,14, 3,14, 3,13)", culture);
        e.Parameters["Pi"] = Math.PI;
        Assert.Equal(3.14d, e.Evaluate());
    }
}