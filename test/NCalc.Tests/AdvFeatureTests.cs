#nullable enable
#define UNDERSCORE_IN_DECIMALS

namespace NCalc.Tests;

[Trait("Category", "Advanced")]
public class AdvFeatureTests
{
    [Theory]
    [InlineData("#1@6@2025#", "@", AdvancedExpressionOptions.DateOrderKind.MDY, new int[] { 2025, 1, 6 })]
    [InlineData("#1/6/2025#", "/", AdvancedExpressionOptions.DateOrderKind.DMY, new int[] { 2025, 6, 1 })]
    [InlineData("#2025.06.01#", ".", AdvancedExpressionOptions.DateOrderKind.YMD, new int[] { 2025, 6, 1 })]
    public void ShouldParseDatesCustom(string input, string separator, AdvancedExpressionOptions.DateOrderKind dateOrder, int[] expectedValue)
    {
        var expression = new Expression(input);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.DateSeparator = separator;
        expression.AdvancedOptions.DateOrder = dateOrder;

        var result = expression.Evaluate();

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedDate, result);
    }

    /// <summary>
    /// These tests test how ncalc standard parsing is done when a custom format is specified
    /// Standard parsing uses the x/y/z format but uses current culture to define the order of elements, i.e., how x,y, and z correspond to years, months, and days
    /// </summary>
    [Theory]
    [InlineData("#1@6@2025#", "@", AdvancedExpressionOptions.DateOrderKind.MDY, new int[] { 2025, 1, 6 })]
    [InlineData("#1/6/2025#", "@", AdvancedExpressionOptions.DateOrderKind.MDY, new int[] { 2025, 6, 1 })]
    [InlineData("#1/6/2025#", "$", AdvancedExpressionOptions.DateOrderKind.DMY, new int[] { 2025, 6, 1 })]
    [InlineData("#2025/06/01#", ".", AdvancedExpressionOptions.DateOrderKind.YMD, new int[] { 2025, 6, 1 })]
    public void ShouldParseDatesCustomWithDefault(string input, string separator, AdvancedExpressionOptions.DateOrderKind dateOrder, int[] expectedValue)
    {
        var expression = new Expression(input);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags = AdvExpressionOptions.None;
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.DateSeparator = separator;
        expression.AdvancedOptions.DateOrder = dateOrder;

        var result = expression.Evaluate();

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("#1/6/2025#", "en-US", new int[] { 2025, 1, 6 })]
    [InlineData("#1.6.2025#", "de-DE", new int[] { 2025, 6, 1 })]
    [InlineData("#2025/06/01#", "ja-JP", new int[] { 2025, 6, 1 })]
    public void ShouldParseDatesCulture(string input, string cultureName, int[] expectedValue)
    {
        var expression = new Expression(input);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.CultureInfo = new CultureInfo(cultureName);

        var result = expression.Evaluate();

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("#1/6/2025#", new int[] { 2025, 6, 1 })]
    public void ShouldParseDatesBuiltin(string input, int[] expectedValue)
    {
        var expression = new Expression(input);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.BuiltIn;

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate =
            (currentDateFormat.IndexOf('M') < (currentDateFormat.IndexOf('d')) // are we running in the US?
            ? new DateTime(expectedValue[0], expectedValue[2], expectedValue[1])
            : new DateTime(expectedValue[0], expectedValue[1], expectedValue[2]));

        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("#1:11:12#", ":", new int[] { 01, 11, 12 })]
    [InlineData("#10:1:12#", ":", new int[] { 10, 01, 12 })]
    [InlineData("#1:2:3#", ":", new int[] { 1, 02, 03 })]
    [InlineData("#10:11:12#", ":", new int[] { 10, 11, 12 })]
    [InlineData("#22:11:12#", ":", new int[] { 22, 11, 12 })]
    [InlineData("#22@11@12#", "@", new int[] { 22, 11, 12 })]
    [InlineData("#22@1@2#", "@", new int[] { 22, 1, 2 })]
    [InlineData("#22@11#", "@", new int[] { 22, 11, 00 })]
    [InlineData("#22@1#", "@", new int[] { 22, 1, 00 })]
    [InlineData("#22@01#", "@", new int[] { 22, 01, 00 })]

    public void ShouldParseTimesCustom(string input, string separator, int[] expectedValue)
    {
        var expression = new Expression(input);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = separator;

        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#22:11#", "de-DE", new int[] { 22, 11, 00 })]
    [InlineData("#22:11:12#", "de-DE", new int[] { 22, 11, 12 })]

    [InlineData("#1:11:12#", "de-DE", new int[] { 01, 11, 12 })]
    [InlineData("#10:1:12#", "de-DE", new int[] { 10, 01, 12 })]
    [InlineData("#1:2:3#", "de-DE", new int[] { 1, 02, 03 })]

    [InlineData("#10:11:12#", "en-US", new int[] { 10, 11, 12 })]
    [InlineData("#10:11:12p#", "en-US", new int[] { 22, 11, 12 })]
    [InlineData("#10:11:12pm#", "en-US", new int[] { 22, 11, 12 })]
    [InlineData("#10:11:12 p#", "en-US", new int[] { 22, 11, 12 })]
    [InlineData("#10:11:12 pm#", "en-US", new int[] { 22, 11, 12 })]
    [InlineData("#10:11#", "en-US", new int[] { 10, 11, 00 })]
    [InlineData("#10:11p#", "en-US", new int[] { 22, 11, 00 })]
    [InlineData("#10:11pm#", "en-US", new int[] { 22, 11, 00 })]
    [InlineData("#10:11 p#", "en-US", new int[] { 22, 11, 00 })]
    [InlineData("#10:11 pm#", "en-US", new int[] { 22, 11, 00 })]

    public void ShouldParseTimesCulture(string input, string cultureName, int[] expectedValue)
    {
        CultureInfo cultureInfo = new CultureInfo(cultureName);

        var expression = new Expression(input);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.CultureInfo = cultureInfo;

        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
#if UNDERSCORE_IN_DECIMALS
    [InlineData("12_3", 123)]
    [InlineData("1234.5_6", 1234.56)]
#endif
    [InlineData("0x12_34", 0x1234)]
    [InlineData("0o0_100", 64)]
    public void ShouldHandleUnderscores(string formula, object expectedValue)
    {
        var expression = new Expression(formula, CultureInfo.InvariantCulture);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.AcceptUnderscoresInNumbers;
        var result = expression.Evaluate();
        Assert.Equal(expectedValue, result);
    }
}