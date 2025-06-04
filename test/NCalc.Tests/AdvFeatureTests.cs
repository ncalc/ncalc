#nullable enable

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
        var expression = new Expression(input, ExpressionOptions.NoCache);
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
    [InlineData("#1/6/2025#", "$", AdvancedExpressionOptions.DateOrderKind.DMY, new int[] { 2025, 6, 1 })]
    [InlineData("#2025/06/01#", ".", AdvancedExpressionOptions.DateOrderKind.YMD, new int[] { 2025, 6, 1 })]
    public void ShouldParseDatesCustomWithDefault(string input, string separator, AdvancedExpressionOptions.DateOrderKind dateOrder, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
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
    [InlineData("#1/06/2025#", "en-US", new int[] { 2025, 1, 6 })]
    [InlineData("#01/06/2025#", "en-US", new int[] { 2025, 1, 6 })]
    [InlineData("#1.6.2025#", "de-DE", new int[] { 2025, 6, 1 })]
    [InlineData("#01.6.2025#", "de-DE", new int[] { 2025, 6, 1 })]
    [InlineData("#1.06.2025#", "de-DE", new int[] { 2025, 6, 1 })]
    [InlineData("#2025/06/01#", "ja-JP", new int[] { 2025, 6, 1 })]
    [InlineData("#2025/6/1#", "ja-JP", new int[] { 2025, 6, 1 })]
    public void ShouldParseDatesCulture(string input, string cultureName, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.CultureInfo = new CultureInfo(cultureName);

        var result = expression.Evaluate();

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("#1/6/2025#", new int[] { 2025, 6, 1 })]
    [InlineData("#1/06/2025#", new int[] { 2025, 6, 1 })]
    [InlineData("#1/12/2025#", new int[] { 2025, 12, 1 })]
    [InlineData("#01/12/2025#", new int[] { 2025, 12, 1 })]
    public void ShouldParseDatesBuiltin(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.BuiltIn;

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate =
            (currentDateFormat.IndexOf('M') < (currentDateFormat.IndexOf('d')) // are we running this test in the US locale?
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

    public void ShouldParseTimesCustom24hr(string input, string separator, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = separator;
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.Always24Hour;

        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#1:11:12#", ":", new int[] { 01, 11, 12 })]
    [InlineData("#10:1:12#", ":", new int[] { 10, 01, 12 })]
    [InlineData("#1:2:3#", ":", new int[] { 1, 02, 03 })]
    [InlineData("#10:11:12#", ":", new int[] { 10, 11, 12 })]
    [InlineData("#10:11:12pm#", ":", new int[] { 22, 11, 12 })]
    [InlineData("#10@11@12pm#", "@", new int[] { 22, 11, 12 })]
    [InlineData("#10@1@2 pm#", "@", new int[] { 22, 1, 2 })]
    [InlineData("#10@11 pm#", "@", new int[] { 22, 11, 00 })]
    [InlineData("#10@1 p#", "@", new int[] { 22, 1, 00 })]
    [InlineData("#10@01 p#", "@", new int[] { 22, 01, 00 })]

    public void ShouldParseTimesCustom12hr(string input, string separator, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = separator;
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.Always12Hour;

        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#1:11:12#", ":", new int[] { 01, 11, 12 })]
    [InlineData("#10:1:12#", ":", new int[] { 10, 01, 12 })]
    [InlineData("#1:2:3#", ":", new int[] { 1, 02, 03 })]
    [InlineData("#10:11:12#", ":", new int[] { 10, 11, 12 })]
    [InlineData("#10:11:12pm#", ":", new int[] { 22, 11, 12 })]
    [InlineData("#10@11@12pm#", "@", new int[] { 22, 11, 12 })]
    [InlineData("#10@1@2 pm#", "@", new int[] { 22, 1, 2 })]
    [InlineData("#10@11 pm#", "@", new int[] { 22, 11, 00 })]
    [InlineData("#10@1 p#", "@", new int[] { 22, 1, 00 })]
    [InlineData("#10@01 p#", "@", new int[] { 22, 01, 00 })]

    public void ShouldParseTimesCustom12HrCulture(string input, string separator, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = separator;
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.FromCulture;
        expression.AdvancedOptions.CultureInfo = new CultureInfo("en-US");

        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
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

    public void ShouldParseTimesCustom24hrCulture(string input, string separator, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = separator;
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.FromCulture;
        expression.AdvancedOptions.CultureInfo = new CultureInfo("de-DE");

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

    [InlineData("#1:1:1#", "en-US", new int[] { 1, 1, 1 })]
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
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.FromCulture;
        CultureInfo cultureInfo = new CultureInfo(cultureName);
        expression.CultureInfo = cultureInfo;

        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#10:11#", new int[] { 10, 11, 00 })]
    [InlineData("#10:11:12#", new int[] { 10, 11, 12 })]
    [InlineData("#1:1:1#", new int[] { 1, 1, 1 })]
    [InlineData("#1:12:12#", new int[] { 1, 12, 12 })]

    public void ShouldParseTimesBuiltIn(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);

        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#1/6/2025 10:11#", new int[] { 2025, 6, 1, 10, 11, 00 })]
    [InlineData("#1/12/2025 10:11#", new int[] { 2025, 12, 1, 10, 11, 00 })]
    [InlineData("#1/6/2025 10:11:12#", new int[] { 2025, 6, 1, 10, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12#", new int[] { 2025, 12, 1, 10, 11, 12 })]
    [InlineData("#1/6/2025 1:1:1#", new int[] { 2025, 6, 1, 1, 1, 1 })]
    [InlineData("#1/12/2025 1:1:1#", new int[] { 2025, 12, 1, 1, 1, 1 })]
    [InlineData("#1/6/2025 1:12:12#", new int[] { 2025, 6, 1, 1, 12, 12 })]
    [InlineData("#1/12/2025 1:12:12#", new int[] { 2025, 12, 1, 1, 12, 12 })]

    public void ShouldParseDatesBuiltinTimesBuiltIn(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.BuiltIn;
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.BuiltIn;

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate =
            (currentDateFormat.IndexOf('M') < (currentDateFormat.IndexOf('d')) // are we running this test in the US locale?
            ? new DateTime(expectedValue[0], expectedValue[2], expectedValue[1], expectedValue[3], expectedValue[4], expectedValue[5])
            : new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]));

        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("#1/6/2025 22:11#", "de-DE", new int[] { 2025, 6, 1, 22, 11, 00 })]
    [InlineData("#1/12/2025 22:11#", "de-DE", new int[] { 2025, 12, 1, 22, 11, 00 })]

    [InlineData("#1/6/2025 22:11:12#", "de-DE", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1/12/2025 22:11:12#", "de-DE", new int[] { 2025, 12, 1, 22, 11, 12 })]

    [InlineData("#1/6/2025 1:11:12#", "de-DE", new int[] { 2025, 6, 1, 1, 11, 12 })]
    [InlineData("#1/12/2025 1:11:12#", "de-DE", new int[] { 2025, 12, 1, 1, 11, 12 })]

    [InlineData("#1/6/2025 10:1:12#", "de-DE", new int[] { 2025, 6, 1, 10, 1, 12 })]
    [InlineData("#1/12/2025 10:1:12#", "de-DE", new int[] { 2025, 12, 1, 10, 1, 12 })]

    [InlineData("#1/6/2025 1:2:3#", "de-DE", new int[] { 2025, 6, 1, 1, 2, 3 })]
    [InlineData("#1/12/2025 1:2:3#", "de-DE", new int[] { 2025, 12, 1, 1, 2, 3})]

    [InlineData("#1/6/2025 1:1:1#", "en-US", new int[] { 2025, 6, 1, 1, 1, 1 })]
    [InlineData("#1/12/2025 1:1:1#", "en-US", new int[] { 2025, 12, 1, 1, 1, 1})]

    [InlineData("#1/6/2025 10:11:12#", "en-US", new int[] { 2025, 6, 1, 10, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12#", "en-US", new int[] { 2025, 12, 1, 10, 11, 12})]

    [InlineData("#1/6/2025 10:11:12p#", "en-US", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12p#", "en-US", new int[] { 2025, 12, 1, 22, 11, 12})]

    [InlineData("#1/6/2025 10:11:12pm#", "en-US", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12pm#", "en-US", new int[] { 2025, 12, 1, 22, 11, 12})]

    [InlineData("#1/6/2025 10:11:12 p#", "en-US", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12 p#", "en-US", new int[] { 2025, 12, 1, 22, 11, 12})]

    [InlineData("#1/6/2025 10:11:12 pm#", "en-US", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12 pm#", "en-US", new int[] { 2025, 12, 1, 22, 11, 12})]

    [InlineData("#1/6/2025 10:11#", "en-US", new int[] { 2025, 6, 1, 10, 11, 0 })]
    [InlineData("#1/12/2025 10:11#", "en-US", new int[] { 2025, 12, 1, 10, 11, 0})]

    [InlineData("#1/6/2025 10:11p#", "en-US", new int[] { 2025, 6, 1, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11p#", "en-US", new int[] { 2025, 12, 1, 22, 11, 0})]

    [InlineData("#1/6/2025 10:11pm#", "en-US", new int[] { 2025, 6, 1, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11pm#", "en-US", new int[] { 2025, 12, 1, 22, 11, 0})]

    [InlineData("#1/6/2025 10:11 p#", "en-US", new int[] { 2025, 6, 1, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11 p#", "en-US", new int[] { 2025, 12, 1, 22, 11, 0})]

    [InlineData("#1/6/2025 10:11 pm#", "en-US", new int[] { 2025, 6, 1, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11 pm#", "en-US", new int[] { 2025, 12, 1, 22, 11, 0})]
    public void ShouldParseDatesBuiltinTimesCulture(string input, string cultureName, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.BuiltIn;
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.FromCulture;
        expression.CultureInfo = new CultureInfo(cultureName);

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate =
            (currentDateFormat.IndexOf('M') < (currentDateFormat.IndexOf('d')) // are we running this test in the US locale?
            ? new DateTime(expectedValue[0], expectedValue[2], expectedValue[1], expectedValue[3], expectedValue[4], expectedValue[5])
            : new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]));

        Assert.Equal(expectedDate, result);
    }

    [Theory]

    [InlineData("#1/6/2025 22@11@12#", "@", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1/12/2025 22@11@12#", "@", new int[] { 2025, 12, 1, 22, 11, 12 })]
    [InlineData("#1/6/2025 22@1@2#", "@", new int[] { 2025, 6, 1, 22, 1, 2 })]
    [InlineData("#1/12/2025 22@1@2#", "@", new int[] { 2025, 12, 1, 22, 1, 2 })]
    [InlineData("#1/6/2025 22@11#", "@", new int[] { 2025, 6, 1, 22, 11, 00 })]
    [InlineData("#1/12/2025 22@11#", "@", new int[] { 2025, 12, 1, 22, 11, 00 })]
    [InlineData("#1/6/2025 22@1#", "@", new int[] { 2025, 6, 1, 22, 1, 00 })]
    [InlineData("#1/12/2025 22@1#", "@", new int[] { 2025, 12, 1, 22, 1, 00 })]
    [InlineData("#1/6/2025 22@01#", "@", new int[] { 2025, 6, 1, 22, 1, 00 })]
    [InlineData("#1/12/2025 22@01#", "@", new int[] { 2025, 12, 1, 22, 1, 00 })]
    public void ShouldParseDatesBuiltinTimesCustom(string input, string separator, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.BuiltIn;
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = separator;
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.Always24Hour; // for simplicity

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate =
            (currentDateFormat.IndexOf('M') < (currentDateFormat.IndexOf('d')) // are we running this test in the US locale?
            ? new DateTime(expectedValue[0], expectedValue[2], expectedValue[1], expectedValue[3], expectedValue[4], expectedValue[5])
            : new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]));

        Assert.Equal(expectedDate, result);
    }

    [Theory]

    [InlineData("#1/6/2025 10:11#", "en-US", new int[] { 2025, 1, 6, 10, 11, 00 })]
    [InlineData("#1/6/2025 10:11:12#", "en-US", new int[] { 2025, 1, 6, 10, 11, 12 })]
    [InlineData("#1/6/2025 22:11#", "en-US", new int[] { 2025, 1, 6, 22, 11, 00 })]
    [InlineData("#1/6/2025 22:11:12#", "en-US", new int[] { 2025, 1, 6, 22, 11, 12 })]

    [InlineData("#1.6.2025 10:11#", "de-DE", new int[] { 2025, 6, 1, 10, 11, 00 })]
    [InlineData("#1.6.2025 10:11:12#", "de-DE", new int[] { 2025, 6, 1, 10, 11, 12})]
    [InlineData("#1.6.2025 22:11#", "de-DE", new int[] { 2025, 6, 1, 22, 11, 00 })]
    [InlineData("#1.6.2025 22:11:12#", "de-DE", new int[] { 2025, 6, 1, 22, 11, 12 })]

    [InlineData("#2025/06/01 10:11#", "ja-JP", new int[] { 2025, 6, 1, 10, 11, 00 })]
    [InlineData("#2025/06/01 10:11:12#", "ja-JP", new int[] { 2025, 6, 1, 10, 11, 12 })]
    [InlineData("#2025/06/01 22:11#", "ja-JP", new int[] { 2025, 6, 1, 22, 11, 00 })]
    [InlineData("#2025/06/01 22:11:12#", "ja-JP", new int[] { 2025, 6, 1, 22, 11, 12 })]
    public void ShouldParseDatesCultureTimesBuiltIn(string input, string cultureName, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.BuiltIn;
        expression.CultureInfo = new CultureInfo(cultureName);

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("#1.6.2025 22:11#", "de-DE", new int[] { 2025, 6, 1, 22, 11, 00 })]
    [InlineData("#1.12.2025 22:11#", "de-DE", new int[] { 2025, 12, 1, 22, 11, 00 })]

    [InlineData("#1.6.2025 22:11:12#", "de-DE", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1.12.2025 22:11:12#", "de-DE", new int[] { 2025, 12, 1, 22, 11, 12 })]

    [InlineData("#1.6.2025 1:11:12#", "de-DE", new int[] { 2025, 6, 1, 1, 11, 12 })]
    [InlineData("#1.12.2025 1:11:12#", "de-DE", new int[] { 2025, 12, 1, 1, 11, 12 })]

    [InlineData("#1.6.2025 10:1:12#", "de-DE", new int[] { 2025, 6, 1, 10, 1, 12 })]
    [InlineData("#1.12.2025 10:1:12#", "de-DE", new int[] { 2025, 12, 1, 10, 1, 12 })]

    [InlineData("#1.6.2025 1:2:3#", "de-DE", new int[] { 2025, 6, 1, 1, 2, 3 })]
    [InlineData("#1.12.2025 1:2:3#", "de-DE", new int[] { 2025, 12, 1, 1, 2, 3 })]

    [InlineData("#2025/06/01 22:11#", "ja-JP", new int[] { 2025, 6, 1, 22, 11, 00 })]
    [InlineData("#2025/12/1 22:11#", "ja-JP", new int[] { 2025, 12, 1, 22, 11, 00 })]

    [InlineData("#2025/06/01 22:11:12#", "ja-JP", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#2025/12/1 22:11:12#", "ja-JP", new int[] { 2025, 12, 1, 22, 11, 12 })]

    [InlineData("#2025/06/01 1:11:12#", "ja-JP", new int[] { 2025, 6, 1, 1, 11, 12 })]
    [InlineData("#2025/12/1 1:11:12#", "ja-JP", new int[] { 2025, 12, 1, 1, 11, 12 })]

    [InlineData("#2025/06/01 10:1:12#", "ja-JP", new int[] { 2025, 6, 1, 10, 1, 12 })]
    [InlineData("#2025/12/1 10:1:12#", "ja-JP", new int[] { 2025, 12, 1, 10, 1, 12 })]

    [InlineData("#2025/06/01 1:2:3#", "ja-JP", new int[] { 2025, 6, 1, 1, 2, 3 })]
    [InlineData("#2025/12/1 1:2:3#", "ja-JP", new int[] { 2025, 12, 1, 1, 2, 3 })]

    [InlineData("#1/6/2025 1:1:1#", "en-US", new int[] { 2025, 1, 6, 1, 1, 1 })]
    [InlineData("#1/12/2025 1:1:1#", "en-US", new int[] { 2025, 1, 12, 1, 1, 1 })]

    [InlineData("#1/6/2025 10:11:12#", "en-US", new int[] { 2025, 1, 6, 10, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12#", "en-US", new int[] { 2025, 1, 12, 10, 11, 12 })]

    [InlineData("#1/6/2025 10:11:12p#", "en-US", new int[] { 2025, 1, 6, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12p#", "en-US", new int[] { 2025, 1, 12, 22, 11, 12 })]

    [InlineData("#1/6/2025 10:11:12pm#", "en-US", new int[] { 2025, 1, 6, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12pm#", "en-US", new int[] { 2025, 1, 12, 22, 11, 12 })]

    [InlineData("#1/6/2025 10:11:12 p#", "en-US", new int[] { 2025, 1, 6, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12 p#", "en-US", new int[] { 2025, 1, 12, 22, 11, 12 })]

    [InlineData("#1/6/2025 10:11:12 pm#", "en-US", new int[] { 2025, 1, 6, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12 pm#", "en-US", new int[] { 2025, 1, 12, 22, 11, 12 })]

    [InlineData("#1/6/2025 10:11#", "en-US", new int[] { 2025, 1, 6, 10, 11, 0 })]
    [InlineData("#1/12/2025 10:11#", "en-US", new int[] { 2025, 1, 12, 10, 11, 0 })]

    [InlineData("#1/6/2025 10:11p#", "en-US", new int[] { 2025, 1, 6, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11p#", "en-US", new int[] { 2025, 1, 12, 22, 11, 0 })]

    [InlineData("#1/6/2025 10:11pm#", "en-US", new int[] { 2025, 1, 6, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11pm#", "en-US", new int[] { 2025, 1, 12, 22, 11, 0 })]

    [InlineData("#1/6/2025 10:11 p#", "en-US", new int[] { 2025, 1, 6, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11 p#", "en-US", new int[] { 2025, 1, 12, 22, 11, 0 })]

    [InlineData("#1/6/2025 10:11 pm#", "en-US", new int[] { 2025, 1, 6, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11 pm#", "en-US", new int[] { 2025, 1, 12, 22, 11, 0 })]
    public void ShouldParseDatesCultureTimesCulture(string input, string cultureName, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.FromCulture;
        expression.CultureInfo = new CultureInfo(cultureName);

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]

    [InlineData("#1/6/2025 10@11@12#", "en-US", "@", new int[] { 2025, 1, 6, 10, 11, 12 })]
    [InlineData("#1/6/2025 10@11@12pm#", "en-US",  "@", new int[] { 2025, 1, 6, 22, 11, 12 })] // custom time will follow 12-hour format of the specified culture

    [InlineData("#1.6.2025 1@11@12#", "de-DE", "@", new int[] { 2025, 6, 1, 1, 11, 12 })]
    [InlineData("#1.6.2025 22@11@12#", "de-DE", "@", new int[] { 2025, 6, 1, 22, 11, 12 })]

    [InlineData("#2025/06/01 1@11@12#", "ja-JP", "@", new int[] { 2025, 6, 1, 1, 11, 12 })]
    [InlineData("#2025/06/01 22@11@12#", "ja-JP", "@", new int[] { 2025, 6, 1, 22, 11, 12 })]
    public void ShouldParseDatesCultureTimesCustom(string input, string cultureName, string timeSeparator, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.AdvancedOptions.CultureInfo = new CultureInfo(cultureName);
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = timeSeparator;
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.FromCulture;

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]

    [InlineData("#1@6@2025 11:12:25#", "@", AdvancedExpressionOptions.DateOrderKind.MDY, new int[] { 2025, 1, 6, 11, 12, 25 })]
    [InlineData("#1@6@2025 1:12#", "@", AdvancedExpressionOptions.DateOrderKind.MDY, new int[] { 2025, 1, 6, 1, 12, 00 })]

    [InlineData("#1/6/2025 11:12:25#", "/", AdvancedExpressionOptions.DateOrderKind.DMY, new int[] { 2025, 6, 1, 11, 12, 25 })]
    [InlineData("#1/6/2025 1:12#", "/", AdvancedExpressionOptions.DateOrderKind.DMY, new int[] { 2025, 6, 1, 1, 12, 00 })]

    [InlineData("#1.6.2025 11:12:25#", ".", AdvancedExpressionOptions.DateOrderKind.DMY, new int[] { 2025, 6, 1, 11, 12, 25 })]
    [InlineData("#1.6.2025 1:12#", ".", AdvancedExpressionOptions.DateOrderKind.DMY, new int[] { 2025, 6, 1, 1, 12, 00 })]

    [InlineData("#2025.06.01 11:12:25#", ".", AdvancedExpressionOptions.DateOrderKind.YMD, new int[] { 2025, 6, 1, 11, 12, 25 })]
    [InlineData("#2025.06.01 1:12#", ".", AdvancedExpressionOptions.DateOrderKind.YMD, new int[] { 2025, 6, 1, 1, 12, 00 })]
    public void ShouldParseDatesCustomTimesBuiltin(string input, string dateSeparator, AdvancedExpressionOptions.DateOrderKind dateOrder, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.DateSeparator = dateSeparator;
        expression.AdvancedOptions.DateOrder = dateOrder;

        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.BuiltIn;

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("#1@6@2025 11:12:25#", "@", AdvancedExpressionOptions.DateOrderKind.MDY, "de-DE", new int[] { 2025, 1, 6, 11, 12, 25 })]
    [InlineData("#1@6@2025 1:12#", "@", AdvancedExpressionOptions.DateOrderKind.MDY, "de-DE", new int[] { 2025, 1, 6, 1, 12, 00 })]

    [InlineData("#1/6/2025 11:12:25#", "/", AdvancedExpressionOptions.DateOrderKind.DMY, "en_US", new int[] { 2025, 6, 1, 11, 12, 25 })]
    [InlineData("#1/6/2025 1:12pm#", "/", AdvancedExpressionOptions.DateOrderKind.DMY, "en_US", new int[] { 2025, 6, 1, 13, 12, 00 })]

    [InlineData("#1.6.2025 11:12:25#", ".", AdvancedExpressionOptions.DateOrderKind.DMY, "de-DE", new int[] { 2025, 6, 1, 11, 12, 25 })]
    [InlineData("#1.6.2025 1:12#", ".", AdvancedExpressionOptions.DateOrderKind.DMY, "de-DE", new int[] { 2025, 6, 1, 1, 12, 00 })]

    [InlineData("#2025.06.01 11:12:25#", ".", AdvancedExpressionOptions.DateOrderKind.YMD, "de-DE", new int[] { 2025, 6, 1, 11, 12, 25 })]
    [InlineData("#2025.06.01 1:12#", ".", AdvancedExpressionOptions.DateOrderKind.YMD, "de-DE", new int[] { 2025, 6, 1, 1, 12, 00 })]
    public void ShouldParseDatesCustomTimesCulture(string input, string dateSeparator, AdvancedExpressionOptions.DateOrderKind dateOrder, string cultureName, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.DateSeparator = dateSeparator;
        expression.AdvancedOptions.DateOrder = dateOrder;
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.FromCulture;
        expression.AdvancedOptions.CultureInfo = new CultureInfo(cultureName);

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("#1$6$2025 22@11@12#", "$", AdvancedExpressionOptions.DateOrderKind.DMY, "@", false, new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1$6$2025 22@11@12#", "$", AdvancedExpressionOptions.DateOrderKind.MDY, "@", false, new int[] { 2025, 1, 6, 22, 11, 12 })]
    [InlineData("#2025$6$1 22@11@12#", "$", AdvancedExpressionOptions.DateOrderKind.MDY, "@", false, new int[] { 2025, 6, 1, 22, 11, 12 })]

    [InlineData("#1$6$2025 10@11@12 pm#", "$", AdvancedExpressionOptions.DateOrderKind.DMY, "@", true, new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1$6$2025 10@11@12 p#", "$", AdvancedExpressionOptions.DateOrderKind.MDY, "@", true, new int[] { 2025, 1, 6, 22, 11, 12 })]
    [InlineData("#2025$6$1 10@11@12pm#", "$", AdvancedExpressionOptions.DateOrderKind.MDY, "@", true, new int[] { 2025, 6, 1, 22, 11, 12 })]
    public void ShouldParseDatesCustomTimesCustom(string input, string dateSeparator, AdvancedExpressionOptions.DateOrderKind dateOrder, string timeSeparator, bool use12Hours, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.DateSeparator = dateSeparator;
        expression.AdvancedOptions.DateOrder = dateOrder;
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = timeSeparator;
        expression.AdvancedOptions.HoursFormat = use12Hours ? AdvancedExpressionOptions.HoursFormatKind.Always12Hour : AdvancedExpressionOptions.HoursFormatKind.Always24Hour;

        var result = expression.Evaluate();

        string currentDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]

    [InlineData("12_3", 123)]
    [InlineData("1234.5_6", 1234.56)]
    public void ShouldHandleUnderscoresInDecimal(string formula, object expectedValue)
    {
        // Support of underscores in decimal literals requires a patch in Parlot,
        // currently available in https://github.com/Allied-Bits-Ltd/parlot
        // and offered to the main project as a pull request https://github.com/sebastienros/parlot/pull/221
        if (Enum.GetNames(typeof(Parlot.Fluent.NumberOptions)).Contains("AllowUnderscore"))
        {
            var expression = new Expression(formula, CultureInfo.InvariantCulture);
            expression.AdvancedOptions = new AdvancedExpressionOptions();
            expression.AdvancedOptions.Flags |= AdvExpressionOptions.AcceptUnderscoresInNumbers;
            var result = expression.Evaluate();
            Assert.Equal(expectedValue, result);
        }
    }

    [Theory]
    [InlineData("0x10_00", 4096)]
    [InlineData("0o_0_100", 64)]
    public void ShouldHandleUnderscoresInHexOct(string formula, object expectedValue)
    {
        var expression = new Expression(formula, CultureInfo.InvariantCulture);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.AcceptUnderscoresInNumbers;
        var result = expression.Evaluate();
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("00100", 64)]
    public void ShouldHandleCStyleOctals(string formula, object expectedValue)
    {
        var expression = new Expression(formula, CultureInfo.InvariantCulture);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.AcceptCStyleOctals;
        var result = expression.Evaluate();
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("1@5", "@", 1.5)]
    [InlineData("1:5", ":", 1.5)]

    public void ShouldHandleDecimalSeparatorCustom(string input, string separator, double expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DecimalSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.DecimalSeparator = separator;

        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("1.5", "en-US", 1.5)]
    [InlineData("1,5", "de-DE", 1.5)]
    [InlineData("1.5", "ja-JP", 1.5)]

    public void ShouldHandleDecimalSeparatorCulture(string input, string cultureName, double expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DecimalSeparatorType = AdvancedExpressionOptions.SeparatorType.FromCulture;
        expression.AdvancedOptions.CultureInfo = new CultureInfo(cultureName);

        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("1@000", "@", 1000)]
    [InlineData("1:000", ":", 1000)]

    public void ShouldHandleNumberGroupSeparatorCustom(string input, string separator, object expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.NumberGroupSeparatorType = AdvancedExpressionOptions.GroupSeparatorType.Custom;
        expression.AdvancedOptions.NumberGroupSeparator = separator;

        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("1,500", "en-US", 1500)]
    [InlineData("1,500.50", "en-US", 1500.50)]
    [InlineData("1,500", "ja-JP", 1500)]
    [InlineData("1,500.50", "ja-JP", 1500.50)]

    public void ShouldHandleNumberGroupSeparatorCulture(string input, string cultureName, object expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.NumberGroupSeparatorType = AdvancedExpressionOptions.GroupSeparatorType.FromCulture;
        expression.AdvancedOptions.CultureInfo = new CultureInfo(cultureName);

        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("1+@", 2, 3)]
    [InlineData("1+@*2", 2, 5)]
    public void ShouldHandleResultReference(string input, int previousResult, object expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.UseResultReference;
        expression.EvaluateFunction += (string name, Handlers.FunctionArgs args) => { if (name.Equals("@")) args.Result = previousResult; };

        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("20*5%", 1)]
    [InlineData("20/5%", 400)]
    [InlineData("20/2.5%", 800)]
    [InlineData("100+5%", 105)]
    [InlineData("100-5%", 95)]
    public void ShouldCalculatePercentAsNumber(string input, double expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.CalculatePercent;

        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("5%+2%", "7%")]
    [InlineData("3.5% + 2.5%", "6%")]
    [InlineData("5%-2%", "3%")]
    [InlineData("5%*2", "10%")]
    [InlineData("10%/2", "5%")]
    public void ShouldCalculatePercentAsPercent(string input, string expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.CalculatePercent;

        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result?.ToString());
    }

    [Theory]
    [InlineData("20*5%", 1)]
    [InlineData("20*2.5%", 0.5)]
    [InlineData("20/5%", 400)]
    [InlineData("20/2.5%", 800)]
    [InlineData("100+5%", 105)]
    [InlineData("100-5%", 95)]
    public void ShouldCalculatePercentAsNumberLambda(string input, double expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.CalculatePercent;

        var sut = expression.ToLambda<double>();
        Assert.Equal(expectedValue, sut());
    }

    [Theory]
    [InlineData("5%+2%", "7%")]
    [InlineData("3.5% + 2.5%", "6%")]
    [InlineData("5%-2%", "3%")]
    [InlineData("5%*2", "10%")]
    [InlineData("10%/2", "5%")]
    public void ShouldCalculatePercentAsPercentLambda(string input, string expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.CalculatePercent;

        var sut = expression.ToLambda<string>();
        Assert.Equal(expectedValue, sut());
    }

    [Theory]
    [InlineData("500.50", 0, 500.50)]
    [InlineData("500.50", 1, 500.50)]
    [InlineData("500.50", 2, 500.50)]
    [InlineData("500.50", 3, 500.50)]
    [InlineData("500", 0, 500)]
    [InlineData("500", 1, 500)]
    [InlineData("500", 2, 500)]
    [InlineData("500", 3, 500)]

    public void ShouldAcceptCurrencyCulture(string input, int position, double expectedValue)
    {
        string sym = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
        switch (position)
        {
            case 0: input = sym + input; break;
            case 1: input = input + sym; break;
            case 2: input = sym + ' ' + input; break;
            case 3: input = input + ' ' + sym; break;
        }
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.AcceptCurrencySymbol;
        expression.AdvancedOptions.CurrencySymbolsType = AdvancedExpressionOptions.CurrencySymbolType.FromCulture;

        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("500.50", "\x20ac", "", 0, 500.50)] // \x20ac stands for the euro symbol
    [InlineData("500.50", "\x20ac", "", 1, 500.50)]
    [InlineData("500.50", "\x20ac", "", 2, 500.50)]
    [InlineData("500.50", "\x20ac", "", 3, 500.50)]

    [InlineData("500.50", "\x20ac", "$", 0, 500.50)]
    [InlineData("500.50", "\x20ac", "$", 1, 500.50)]
    [InlineData("500.50", "\x20ac", "$", 2, 500.50)]
    [InlineData("500.50", "\x20ac", "$", 3, 500.50)]

    public void ShouldAcceptCurrencyCustom(string input, string currencySymbol, string currencySymbol2, int position, double expectedValue)
    {
        string sym = currencySymbol2.Length > 0 ? currencySymbol2 : currencySymbol;
        switch (position)
        {
            case 0: input = sym + input; break;
            case 1: input = input + sym; break;
            case 2: input = sym + ' ' + input; break;
            case 3: input = input + ' ' + sym; break;
        }
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.AcceptCurrencySymbol;
        expression.AdvancedOptions.CurrencySymbolsType = AdvancedExpressionOptions.CurrencySymbolType.Custom;
        expression.AdvancedOptions.CurrencySymbol = currencySymbol;
        expression.AdvancedOptions.CurrencySymbol2 = currencySymbol2;
        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("EUR500", 500)]
    [InlineData("EUR 500", 500)]
    [InlineData("500EUR", 500)]
    [InlineData("500\x20ac", 500)] // \x20ac stands for the euro symbol
    [InlineData("500 EUR", 500)]
    public void ShouldAcceptCurrencyEUR(string input, double expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.AcceptCurrencySymbol;
        expression.AdvancedOptions.CurrencySymbolsType = AdvancedExpressionOptions.CurrencySymbolType.Custom;
        expression.AdvancedOptions.CurrencySymbol = "EUR";
        expression.AdvancedOptions.CurrencySymbol2 = "\x20ac";
        var result = expression.Evaluate();

        Assert.Equal(expectedValue, result);
    }
}