#nullable enable

using System.Numerics;

using NCalc.Domain;
using NCalc.Tests.TestData;

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
    [InlineData("#1/12/2025 1:2:3#", "de-DE", new int[] { 2025, 12, 1, 1, 2, 3 })]

    [InlineData("#1/6/2025 1:1:1#", "en-US", new int[] { 2025, 6, 1, 1, 1, 1 })]
    [InlineData("#1/12/2025 1:1:1#", "en-US", new int[] { 2025, 12, 1, 1, 1, 1 })]

    [InlineData("#1/6/2025 10:11:12#", "en-US", new int[] { 2025, 6, 1, 10, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12#", "en-US", new int[] { 2025, 12, 1, 10, 11, 12 })]

    [InlineData("#1/6/2025 10:11:12p#", "en-US", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12p#", "en-US", new int[] { 2025, 12, 1, 22, 11, 12 })]

    [InlineData("#1/6/2025 10:11:12pm#", "en-US", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12pm#", "en-US", new int[] { 2025, 12, 1, 22, 11, 12 })]

    [InlineData("#1/6/2025 10:11:12 p#", "en-US", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12 p#", "en-US", new int[] { 2025, 12, 1, 22, 11, 12 })]

    [InlineData("#1/6/2025 10:11:12 pm#", "en-US", new int[] { 2025, 6, 1, 22, 11, 12 })]
    [InlineData("#1/12/2025 10:11:12 pm#", "en-US", new int[] { 2025, 12, 1, 22, 11, 12 })]

    [InlineData("#1/6/2025 10:11#", "en-US", new int[] { 2025, 6, 1, 10, 11, 0 })]
    [InlineData("#1/12/2025 10:11#", "en-US", new int[] { 2025, 12, 1, 10, 11, 0 })]

    [InlineData("#1/6/2025 10:11p#", "en-US", new int[] { 2025, 6, 1, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11p#", "en-US", new int[] { 2025, 12, 1, 22, 11, 0 })]

    [InlineData("#1/6/2025 10:11pm#", "en-US", new int[] { 2025, 6, 1, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11pm#", "en-US", new int[] { 2025, 12, 1, 22, 11, 0 })]

    [InlineData("#1/6/2025 10:11 p#", "en-US", new int[] { 2025, 6, 1, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11 p#", "en-US", new int[] { 2025, 12, 1, 22, 11, 0 })]

    [InlineData("#1/6/2025 10:11 pm#", "en-US", new int[] { 2025, 6, 1, 22, 11, 0 })]
    [InlineData("#1/12/2025 10:11 pm#", "en-US", new int[] { 2025, 12, 1, 22, 11, 0 })]
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
    [InlineData("#1.6.2025 10:11:12#", "de-DE", new int[] { 2025, 6, 1, 10, 11, 12 })]
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
    [InlineData("#1/6/2025 10@11@12pm#", "en-US", "@", new int[] { 2025, 1, 6, 22, 11, 12 })] // custom time will follow 12-hour format of the specified culture

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
    [InlineData("100+(3+2)%", 105)]
    [InlineData("100-5%", 95)]
    public void ShouldCalculatePercentAsNumber(string input, int expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags |= AdvExpressionOptions.CalculatePercent;

        var result = expression.Evaluate();

        if (result?.GetType() == typeof(System.Double))
        {
            double dResult = (double)result;
            Assert.Equal(expectedValue, (int)dResult);
        }
        else
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
    [InlineData("100+(3+2)%", 105)]
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
    [InlineData("#2025/06/05# + #08:00:00#", new int[] { 2025, 6, 5, 8, 0, 0 })]
    [InlineData("#2025/06/06# - #8:00:00#", new int[] { 2025, 6, 5, 16, 0, 0 })]
    public void ShoudAddSubtractDateAndTime(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.SupportTimeOperations);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.DateSeparator = "/";
        expression.AdvancedOptions.DateOrder = AdvancedExpressionOptions.DateOrderKind.YMD;
        var result = expression.Evaluate();

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("#2025/06/05# + #08:00:00#", new int[] { 2025, 6, 5, 8, 0, 0 })]
    [InlineData("#2025/06/06# - #8:00:00#", new int[] { 2025, 6, 5, 16, 0, 0 })]
    public void ShoudAddSubtractDateAndTimeLambda(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.SupportTimeOperations);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.DateSeparator = "/";
        expression.AdvancedOptions.DateOrder = AdvancedExpressionOptions.DateOrderKind.YMD;
        var sut = expression.ToLambda<DateTime>();
        var result = sut();

        DateTime expectedDate = new DateTime(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4], expectedValue[5]);

        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("#8:00:00# + #08:00:00#", new int[] { 16, 0, 0 })]
    [InlineData("#11:00:00# - #3:00:00#", new int[] { 8, 0, 0 })]
    public void ShoudAddSubtractTimes(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.SupportTimeOperations);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = ":";
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.Always24Hour;
        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#8:00:00# + #08:00:00#", new int[] { 16, 0, 0 })]
    [InlineData("#11:00:00# - #3:00:00#", new int[] { 8, 0, 0 })]
    public void ShoudAddSubtractTimesLambda(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.SupportTimeOperations);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = ":";
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.Always24Hour;
        var sut = expression.ToLambda<TimeSpan>();
        var result = sut();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#2025/06/05# - #2025/06/02#", new int[] { 72, 0, 0 })]
    public void ShoudSubtractDates(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.SupportTimeOperations);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.DateSeparator = "/";
        expression.AdvancedOptions.DateOrder = AdvancedExpressionOptions.DateOrderKind.YMD;
        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#2025/06/05# - #2025/06/02#", new int[] { 72, 0, 0 })]
    public void ShoudAddSubtractDatesLambda(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.SupportTimeOperations);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.DateSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.DateSeparator = "/";
        expression.AdvancedOptions.DateOrder = AdvancedExpressionOptions.DateOrderKind.YMD;
        var sut = expression.ToLambda<TimeSpan>();
        var result = sut();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#1day3hrs356ms#", new int[] { 1, 3, 0, 0, 356 })]
    [InlineData("#1y2wks21day#", new int[] { 400, 0, 0, 0, 0 })]
    [InlineData("#1day 3hrs 356ms#", new int[] { 1, 3, 0, 0, 356 })]
    [InlineData("#1y 2wks 21day#", new int[] { 400, 0, 0, 0, 0 })]
    [InlineData("#1 day 3 hrs 356 ms#", new int[] { 1, 3, 0, 0, 356 })]
    [InlineData("#1 y 2 wks 21 day#", new int[] { 400, 0, 0, 0, 0 })]
    public void ShoudRecognizeHumanePeriods(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags = AdvExpressionOptions.ParseHumanePeriods;
        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#1 day ago#", new int[] { -1, 0, 0, 0, 0})]
    [InlineData("#in 2 weeks#", new int[] { 14, 0, 0, 0, 0 })]
    [InlineData("#3 days later#", new int[] { 3, 0, 0, 0, 0 })]
    [InlineData("#before 1 year#", new int[] { -365, 0, 0, 0, 0 })]
    public void ShoudRecognizeHumaneDates(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags = AdvExpressionOptions.ParseHumanePeriods;
        var result = expression.Evaluate();

        DateTime expectedDate = DateTime.Now + new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4]);
        DateTime actualDate = (result as DateTime?) ?? DateTime.MinValue;

        TimeSpan expectedTime = expectedDate - actualDate;

        Assert.True(expectedTime.TotalSeconds < 10); // 10 seconds is enough for the test to run
    }

    [Theory]
    [InlineData("#1tag3std.356ms#", new int[] { 1, 3, 0, 0, 356 })]
    [InlineData("#1j.2woche21tag#", new int[] { 400, 0, 0, 0, 0 })]
    public void ShoudRecognizeHumanePeriodsCustom(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags = AdvExpressionOptions.ParseHumanePeriods;
        expression.AdvancedOptions.PeriodYearIndicators.Add("jahre"); // must be lowercase
        expression.AdvancedOptions.PeriodYearIndicators.Add("jahr"); // must be lowercase
        expression.AdvancedOptions.PeriodYearIndicators.Add("j");
        expression.AdvancedOptions.PeriodMonthIndicators.Add("monate");
        expression.AdvancedOptions.PeriodMonthIndicators.Add("monat");
        expression.AdvancedOptions.PeriodMonthIndicators.Add("mon");
        expression.AdvancedOptions.PeriodMonthIndicators.Add("m");
        expression.AdvancedOptions.PeriodWeekIndicators.Add("wochen");
        expression.AdvancedOptions.PeriodWeekIndicators.Add("woche");
        expression.AdvancedOptions.PeriodWeekIndicators.Add("wo");
        expression.AdvancedOptions.PeriodWeekIndicators.Add("w");
        expression.AdvancedOptions.PeriodDayIndicators.Add("tage");
        expression.AdvancedOptions.PeriodDayIndicators.Add("tag");
        expression.AdvancedOptions.PeriodDayIndicators.Add("tg");
        expression.AdvancedOptions.PeriodDayIndicators.Add("t");
        expression.AdvancedOptions.PeriodHourIndicators.Add("stunden");
        expression.AdvancedOptions.PeriodHourIndicators.Add("stunde");
        expression.AdvancedOptions.PeriodHourIndicators.Add("std");
        expression.AdvancedOptions.PeriodMinuteIndicators.Add("minuten");
        expression.AdvancedOptions.PeriodMinuteIndicators.Add("minute");
        expression.AdvancedOptions.PeriodMinuteIndicators.Add("min");
        expression.AdvancedOptions.PeriodSecondIndicators.Add("sekunden");
        expression.AdvancedOptions.PeriodSecondIndicators.Add("sekunde");
        expression.AdvancedOptions.PeriodSecondIndicators.Add("sek");
        expression.AdvancedOptions.PeriodSecondIndicators.Add("s");
        expression.AdvancedOptions.PeriodMSecIndicators.Add("ms");

        var result = expression.Evaluate();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#1day3hrs356ms#", new int[] { 1, 3, 0, 0, 356 })]
    [InlineData("#1y2wks21day#", new int[] { 400, 0, 0, 0, 0 })]
    [InlineData("#1day 3hrs 356ms#", new int[] { 1, 3, 0, 0, 356 })]
    [InlineData("#1y 2wks 21day#", new int[] { 400, 0, 0, 0, 0 })]
    [InlineData("#1 day 3 hrs 356 ms#", new int[] { 1, 3, 0, 0, 356 })]
    [InlineData("#1 y 2 wks 21 day#", new int[] { 400, 0, 0, 0, 0 })]
    public void ShoudRecognizeHumanePeriodsLambda(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.Flags = AdvExpressionOptions.ParseHumanePeriods;
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = ":";
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.Always24Hour;
        var sut = expression.ToLambda<TimeSpan>();
        var result = sut();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4]);

        Assert.Equal(expectedTime, result);
    }

    [Theory]
    [InlineData("#1tag3std.356ms#", new int[] { 1, 3, 0, 0, 356 })]
    [InlineData("#1j.2woche21tag#", new int[] { 400, 0, 0, 0, 0 })]
    public void ShoudRecognizeHumanePeriodsCustomLambda(string input, int[] expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        expression.AdvancedOptions = new AdvancedExpressionOptions();
        expression.AdvancedOptions.TimeSeparatorType = AdvancedExpressionOptions.SeparatorType.Custom;
        expression.AdvancedOptions.TimeSeparator = ":";
        expression.AdvancedOptions.HoursFormat = AdvancedExpressionOptions.HoursFormatKind.Always24Hour;
        expression.AdvancedOptions.Flags = AdvExpressionOptions.ParseHumanePeriods;
        expression.AdvancedOptions.PeriodYearIndicators.Add("jahre"); // must be lowercase
        expression.AdvancedOptions.PeriodYearIndicators.Add("jahr"); // must be lowercase
        expression.AdvancedOptions.PeriodYearIndicators.Add("j");
        expression.AdvancedOptions.PeriodMonthIndicators.Add("monate");
        expression.AdvancedOptions.PeriodMonthIndicators.Add("monat");
        expression.AdvancedOptions.PeriodMonthIndicators.Add("mon");
        expression.AdvancedOptions.PeriodMonthIndicators.Add("m");
        expression.AdvancedOptions.PeriodWeekIndicators.Add("wochen");
        expression.AdvancedOptions.PeriodWeekIndicators.Add("woche");
        expression.AdvancedOptions.PeriodWeekIndicators.Add("wo");
        expression.AdvancedOptions.PeriodWeekIndicators.Add("w");
        expression.AdvancedOptions.PeriodDayIndicators.Add("tage");
        expression.AdvancedOptions.PeriodDayIndicators.Add("tag");
        expression.AdvancedOptions.PeriodDayIndicators.Add("tg");
        expression.AdvancedOptions.PeriodDayIndicators.Add("t");
        expression.AdvancedOptions.PeriodHourIndicators.Add("stunden");
        expression.AdvancedOptions.PeriodHourIndicators.Add("stunde");
        expression.AdvancedOptions.PeriodHourIndicators.Add("std");
        expression.AdvancedOptions.PeriodMinuteIndicators.Add("minuten");
        expression.AdvancedOptions.PeriodMinuteIndicators.Add("minute");
        expression.AdvancedOptions.PeriodMinuteIndicators.Add("min");
        expression.AdvancedOptions.PeriodSecondIndicators.Add("sekunden");
        expression.AdvancedOptions.PeriodSecondIndicators.Add("sekunde");
        expression.AdvancedOptions.PeriodSecondIndicators.Add("sek");
        expression.AdvancedOptions.PeriodSecondIndicators.Add("s");
        expression.AdvancedOptions.PeriodMSecIndicators.Add("ms");

        var sut = expression.ToLambda<TimeSpan>();
        var result = sut();

        TimeSpan expectedTime = new TimeSpan(expectedValue[0], expectedValue[1], expectedValue[2], expectedValue[3], expectedValue[4]);

        Assert.Equal(expectedTime, result);
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

    [Theory]
    [InlineData("5!", 120)]
    [InlineData("5!!", 15)]
    [InlineData("10!!!", 280)]
    [InlineData("20!!!", 4188800)]
    public void ShouldCalculateSmallFactorials(string input, long expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        var result = expression.Evaluate();
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("22!", "1124000727777607680000")]
    [InlineData("50!!!", "13106744139423334400000")]
    public void ShouldCalculateLargeFactorials(string input, string expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        var result = expression.Evaluate();
        var expected = BigInteger.Parse(expectedValue);
        Assert.Equal(expectedValue, result?.ToString());
    }

    [Theory]
    [InlineData("2+5!", 122)]
    [InlineData("2*4!", 48)]
    [InlineData("(2+1)!", 6)]
    [InlineData("(2+1) +2!", 5)]
    [InlineData("2**3!", 64)]
    public void ShouldCalculateOperationsWithFactorials(string input, long expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        var result = expression.Evaluate();
        if (result?.GetType() == typeof(System.Double))
        {
            double dResult = (double)result;
            Assert.Equal(expectedValue, (long)dResult);
        }
        else
            Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("5!", 120)]
    [InlineData("5!!", 15)]
    [InlineData("10!!!", 280)]
    [InlineData("20!!!", 4188800)]
    public void ShouldCalculateSmallFactorialsLambda(string input, long expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        var sut = expression.ToLambda<long>();
        var result = sut();
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("2+5!", 122)]
    [InlineData("2*4!", 48)]
    [InlineData("(2+1)!", 6)]
    [InlineData("(2+1) +2!", 5)]
    [InlineData("2**3!", 64)]
    public void ShouldCalculateOperationsWithFactorialsLambda(string input, long expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache);
        var sut = expression.ToLambda<long>();
        var result = sut();
        if (result.GetType() == typeof(System.Double))
        {
            double dResult = (double)result;
            Assert.Equal(expectedValue, (long)dResult);
        }
        else
            Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void SerializeFactorialExpressionsTest()
    {
        Assert.Equal("2!!",
            new BinaryExpression(BinaryExpressionType.Factorial, new ValueExpression(2), new ValueExpression(2)).ToString());
        Assert.Equal("(2 + 2)!",
            new BinaryExpression(BinaryExpressionType.Factorial, new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(2), new ValueExpression(2)), new ValueExpression(1)).ToString());
        Assert.Equal("2 + 2!",
            new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(2), new BinaryExpression(BinaryExpressionType.Factorial, new ValueExpression(2), new ValueExpression(1))).ToString());
    }

    [Fact]
    public void SerializePercentExpressionsTest()
    {
        Assert.Equal("2 + 2%",
            new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(2), new PercentExpression(new ValueExpression(2))).ToString());
        Assert.Equal("2 * 2%",
            new BinaryExpression(BinaryExpressionType.Times, new ValueExpression(2), new PercentExpression(new ValueExpression(2))).ToString());
        Assert.Equal("2% + 2%",
            new BinaryExpression(BinaryExpressionType.Plus, new PercentExpression(new ValueExpression(2)), new PercentExpression(new ValueExpression(2))).ToString());
        Assert.Equal("2% * 2",
            new BinaryExpression(BinaryExpressionType.Times, new PercentExpression(new ValueExpression(2)), new ValueExpression(2)).ToString());
    }

    [Theory]
    [InlineData("\u221A4", 2)]
    [InlineData("\u221A(2+2)", 2)]

#if NET8_0_OR_GREATER
    [InlineData("\u221B8", 2)]
    [InlineData("\u221B(4+4)", 2)]
#endif
    [InlineData("\u221C(4*4)", 2)]
    public void ShouldCalculateRoots(string input, double expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseUnicodeCharsForOperations);
        var result = expression.Evaluate();
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("\u221A4", 2)]
    [InlineData("\u221A(2+2)", 2)]
#if NET8_0_OR_GREATER
    [InlineData("\u221B8", 2)]
    [InlineData("\u221B(4+4)", 2)]
#endif
    [InlineData("\u221C(4*4)", 2)]
    public void ShouldCalculateRootsLambda(string input, double expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseUnicodeCharsForOperations);
        var sut = expression.ToLambda<long>();
        var result = sut();
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("a := 2", 2, 2)]
    [InlineData("a := 2 + 2", 4, 4)]
    [InlineData("{a} := (2 + 2)", 4, 4)]
    public void ShouldHandleAssignment(string input, int expectedVarValue, int expectedExprValue)
    {
        bool eventFired = false;

        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseAssignments);
        expression.UpdateParameter += (name, args) =>
        {
            eventFired = true;
            Assert.Equal("a", name);
            Assert.Equal(expectedVarValue, args.Value);
        };

        var result = expression.Evaluate();

        Assert.True(eventFired);
        Assert.Equal(expectedExprValue, result);
    }

    class AssignmentLambdaTestsContext
    {
        public int a { get; set; }

        public int length(string x)
        {
            return x?.Length ?? 0;
        }
    }

    [Theory]
    [InlineData("a := 2", 2, 2)]
    [InlineData("a := 2 + 2", 4, 4)]
    [InlineData("{a} := (2 + 2)", 4, 4)]
    public void ShouldHandleAssignmentLambda(string input, int expectedVarValue, int expectedExprValue)
    {
        bool eventFired = false;

        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseAssignments);
        expression.UpdateParameter += (name, args) =>
        {
            eventFired = true;
            Assert.Equal("a", name.ToLowerInvariant());
            Assert.Equal(expectedVarValue, args.Value);
        };

        Func<AssignmentLambdaTestsContext, object> function = expression.ToLambda<AssignmentLambdaTestsContext, object>();

        var context = new AssignmentLambdaTestsContext { };
        var result = function(context);

        Assert.True(eventFired);
        Assert.Equal(expectedExprValue, result);
        Assert.Equal(expectedVarValue, context.a);
    }

    [Theory]
    [InlineData("2 + 2; 3 + 3", 6)]
    [InlineData("(2 + 2); 3 + 3", 6)]
    [InlineData("Max(2, 5); 3 + 3", 6)]
    [InlineData("Max(2; 5)", 5)]
    [InlineData("Max(2; 5); 3 + 3", 6)]
    [InlineData("Length('12;45')", 5)]
    [InlineData("'Text data with ; (semicolon)'; 3 + 3", 6)]
    public void ShouldHandleStatementSequence(string input, int expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseStatementSequences);
        expression.Functions["Length"] = (args) =>
        {
            return ((string)args[0].Evaluate()!).Length;
        };
        var result = expression.Evaluate();
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("2 + 2; 3 + 3", 6)]
    [InlineData("(2 + 2); 3 + 3", 6)]
    [InlineData("Max(2, 5); 3 + 3", 6)]
    [InlineData("Max(2; 5)", 5)]
    [InlineData("Max(2; 5); 3 + 3", 6)]
    public void ShouldHandleStatementSequenceLambda(string input, int expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseStatementSequences);
        var sut = expression.ToLambda<long>();
        var result = sut();
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("2 * 2 ",  4)]
    [InlineData("2 ** 2 ", 4)]
    [InlineData("2 || 2 ", true)]
    [InlineData("2 == 2 ", true)]
    public void ShouldHandleBinaryStatements(string input, object expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache  /*| ExpressionOptions.UseAssignments*/ | ExpressionOptions.UseStatementSequences);
        var result = expression.Evaluate();

        if (result.GetType() == typeof(System.Double))
        {
            double dResult = (double)result;
            Assert.Equal(expectedValue, (int)dResult);
        }
        else
        if (result.GetType() == typeof(System.Boolean))
        {
            bool bResult = (bool)result;
            Assert.Equal(expectedValue, bResult);
        }
        else
            Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("a := 2", 2, 2)]
    [InlineData("a := 2 + 2", 4, 4)]
    [InlineData("{a} := (2 + 2)", 4, 4)]
    [InlineData("a := 2; a + 2", 2, 4)]
    [InlineData("a := 2; {a} + 2", 2, 4)]
    [InlineData("a := 2; a + Max(2, 4)", 2, 6)]
    [InlineData("a := 2; b := 4; Max(a, b)", 2, 4)]
    [InlineData("a := 2; a + Max(2; 4)", 2, 6)]
    [InlineData("a := if (true, 2, 4); a + Max(2; 4)", 2, 6)]
    [InlineData("if (true, a := 2, a := 4); a + Max(2; 4)", 2, 6)]
    [InlineData("a := if (true; 2; 4); a + Max(2; 4)", 2, 6)]
    [InlineData("if (true; a := 2; a := 4); a + Max(2; 4)", 2, 6)]
    public void ShouldHandleStatementSequenceWithAssignment(string input, int expectedVarValue, int expectedExprValue)
    {
        bool eventFired = false;

        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseAssignments | ExpressionOptions.UseStatementSequences);
        expression.UpdateParameter += (name, args) =>
        {
            eventFired = true;

            if (name == "a")
            {
                Assert.Equal("a", name);
                Assert.Equal(expectedVarValue, args.Value);
            }
        };
        var result = expression.Evaluate();
        Assert.True(eventFired);
        Assert.Equal(expectedExprValue, result);
    }

    public class StatementSequenceWithAssignment2TestData : TheoryData<string, int>
    {
        public StatementSequenceWithAssignment2TestData()
        {
            Add("a := 2; a += 2", 4);
            Add("a := 4; a -= 2", 2);
            Add("a := 2; a *= 2", 4);
            Add("a := 4; a /= 2", 2);
            Add("a := 3; a &= 2", 2);
            Add("a := 1; a |= 2", 3);
            Add("a := 3; a ^= 2", 1);
            Add("a := 3; a ^= 1", 2);
            Add("a := 2; a ^= 1", 3);
        }
    }

    [Theory]
    [ClassData(typeof(StatementSequenceWithAssignment2TestData))]
    public void ShouldHandleStatementSequenceWithAssignment2(string input, int expectedExprValue)
    {
        bool eventFired = false;

        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseAssignments | ExpressionOptions.UseStatementSequences);
        expression.UpdateParameter += (name, args) => eventFired = true;
        var result = expression.Evaluate();

        Assert.True(eventFired);

        Assert.NotNull(result);
        if (result.GetType() == typeof(System.Double))
        {
            double dResult = (double)result;
            Assert.Equal(expectedExprValue, (double)dResult);
        }
        else
        if (result.GetType() == typeof(System.UInt64))
        {
            ulong uResult = (ulong)result;
            Assert.Equal(expectedExprValue, (int)uResult);
        }
        else
            Assert.Equal(expectedExprValue, result);

        Assert.NotNull(expression.Parameters["a"]);
        if (expression.Parameters["a"]!.GetType() == typeof(System.UInt64))
        {
            ulong uResult = (ulong)expression.Parameters["a"]!;
            Assert.Equal(expectedExprValue, (int)uResult);
        }
        else
        if (expression.Parameters["a"]!.GetType() == typeof(System.Double))
        {
            double dResult = (double)expression.Parameters["a"]!;
            Assert.Equal(expectedExprValue, (int)dResult);
        }
        else
            Assert.Equal(expectedExprValue, expression.Parameters["a"]!);
    }

    [Theory]
    [ClassData(typeof(StatementSequenceWithAssignment2TestData))]
    public void ShouldHandleStatementSequenceWithAssignment2Lambda(string input, int expectedExprValue)
    {
        bool eventFired = false;

        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseAssignments | ExpressionOptions.UseStatementSequences);
        expression.UpdateParameter += (name, args) => eventFired = true;

        Func<AssignmentLambdaTestsContext, object> function = expression.ToLambda<AssignmentLambdaTestsContext, object>();

        var context = new AssignmentLambdaTestsContext { };
        var result = function(context);

        Assert.True(eventFired);

        Assert.NotNull(result);
        if (result.GetType() == typeof(System.UInt64))
        {
            ulong uResult = (ulong)result;
            Assert.Equal(expectedExprValue, (int)uResult);
        }
        else
            Assert.Equal(expectedExprValue, result);

        /*Assert.NotNull(expression.Parameters["a"]);
        if (expression.Parameters["a"]!.GetType() == typeof(System.UInt64))
        {
            ulong uResult = (ulong)expression.Parameters["a"]!;
            Assert.Equal(expectedExprValue, (int)uResult);
        }
        else
            Assert.Equal(expectedExprValue, expression.Parameters["a"]!);*/
        Assert.Equal(expectedExprValue, context.a);
    }

    [Theory]
    [ClassData(typeof(EvaluationTestData))]
    public void ShouldHandleStatementsWithAssignmentsEnabled(string input, object expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | /*ExpressionOptions.UseAssignments | */ExpressionOptions.UseStatementSequences);
        var result = expression.Evaluate();
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("LENgth('xyz')", 3)]
    public void ShouldHandleFunctionsInLowercase(string input, object expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.LowerCaseIdentifierLookup);
        expression.Functions.Add("length", (x) => x[0].Evaluate()?.ToString()?.Length ?? 0);
        var result = expression.Evaluate();
        Assert.Equal(expectedValue, result);
    }

    class InLowercaseLambdaTestsContext
    {
        public int a { get; set; }

        public int length(string x)
        {
            return x?.Length ?? 0;
        }
    }

    [Theory]
    [InlineData("LENgth('xyz')", 3)]
    public void ShouldHandleFunctionsInLowercaseLambda(string input, object expectedValue)
    {
        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.LowerCaseIdentifierLookup);

        Func<InLowercaseLambdaTestsContext, int> function = expression.ToLambda<InLowercaseLambdaTestsContext, int>();

        var context = new InLowercaseLambdaTestsContext {  };
        var result = function(context);

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData("A := if (true, 2, 4); a + Max(2; 4) + A", 2, 8)]
    public void ShouldHandleAssignmentInLowercase(string input, int expectedVarValue, int expectedExprValue)
    {
        bool eventFired = false;

        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseAssignments | ExpressionOptions.UseStatementSequences | ExpressionOptions.LowerCaseIdentifierLookup);
        expression.UpdateParameter += (name, args) =>
        {
            eventFired = true;
            Assert.Equal("a", name.ToLowerInvariant());
            Assert.Equal(expectedVarValue, args.Value);
        };

        var result = expression.Evaluate();

        Assert.True(eventFired);
        Assert.Equal(expectedExprValue, result);
    }

    [Theory]
    [InlineData("A := if (true, 2, 4); a + Max(2; 4) + A", 2, 8)]
    public void ShouldHandleAssignmentInLowercaseLambda(string input, int expectedVarValue, int expectedExprValue)
    {
        bool eventFired = false;

        var expression = new Expression(input, ExpressionOptions.NoCache | ExpressionOptions.UseAssignments | ExpressionOptions.UseStatementSequences | ExpressionOptions.LowerCaseIdentifierLookup);
        expression.UpdateParameter += (name, args) =>
        {
            eventFired = true;
            Assert.Equal("a", name.ToLowerInvariant());
            Assert.Equal(expectedVarValue, args.Value);
        };

        Func<InLowercaseLambdaTestsContext, int> function = expression.ToLambda<InLowercaseLambdaTestsContext, int>();

        var context = new InLowercaseLambdaTestsContext { };
        var result = function(context);

        Assert.True(eventFired);
        Assert.Equal(expectedExprValue, result);
        Assert.Equal(expectedVarValue, context.a);
    }
}