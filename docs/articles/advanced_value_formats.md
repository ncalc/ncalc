# Advanced Value Formats and Operations

Some of the behavior and support for advanced parsing features is controlled by the advanced options. This includes

* Advanced date and time parsing
* Parsing of humane period expressions
* Basic calculations with dates and time spans
* Currency support
* Underscores in numbers and currency
* Custom decimal and group separators in numbers and currency
* C-Style octal literals
* Result Reference character
* Percent calculations

Advanced options are configured by assigning an instance of the <xref:NCalc.AdvancedExpressionOptions> class to the 
`AdvancedOptions` property of the <xref:NCalc.Expression> you create and adjusting its properties:

```c#
var expression = new NCalc.Expression("<your expression here>");
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions. ... = ...;
```

When advanced options are used, an expression is evaluated using a new parser, which is created each time. This means that options are applied during the evaluation and may be changed between evaluations.

## Advanced Date and Time Parsing

This version of NCalc supports flexible culture-sensitive parsing of date and time literals as well as parsing short times (hours and minutes with seconds assumed to be 00). The goal is to provide natural ways for people to enter date and time values in different formats at the same time which don't necessarily align with the system date and time formats. 

The information about Date and Time formats below is also applicable to compound date-time values.

### Date Formats

<xref:NCalc.AdvancedExpressionOptions> let one specify, which format to use for parsing dates. 

The <xref:NCalc.AdvancedExpressionOptions.DateSeparatorType> property lets you choose between 
* `BuiltIn` : parsing of dates is done as if there were no AdvancedExpressionOptions set, i.e., dates are expected in #X/Y/Z# format where current culture defines what X, Y, and Z are (i.e., which one is day, month, and year).
* `FromCulture` : the separator defined in the current culture (which is either CultureInfo.CurrentCulture or a custom culture that you specify in the constructor or the <xref:NCalc.AdvancedExpressionOptions.CultureInfo> property) is used.
* `Custom` : the separator defined in the <xref:NCalc.AdvancedExpressionOptions.DateSeparator> property

When <xref:NCalc.AdvancedExpressionOptions.DateSeparatorType> is set to `FromCulture` or `Custom`, the parser will try to parse the value using the corresponding format and optionally try the built-in format. Whether the built-in format is tried or skipped in this case, is defined by the <xref:NCalc.AdvExpressionOptions.SkipBuiltInDateSeparator> flag. To skip the built-in format, include the <xref:NCalc.AdvExpressionOptions.SkipBuiltInDateSeparator> flag to the <xref:NCalc.AdvancedExpressionOptions.Flags> property of an instance of the <xref:NCalc.AdvancedExpressionOptions> class.

```c#
var expression = new NCalc.Expression("10 * 2%");
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= NCalc.AdvExpressionOptions.SkipBuiltInDateSeparator;
```

#### Order of Values in Dates

When <xref:NCalc.AdvancedExpressionOptions.DateSeparatorType> is set to `FromCulture`, the order of day, month, and year parts is defined by the culture. When it is set to `Custom`, the order is defined by the `DateOrder` property and can be one of `YMD`, `DMY`, or `MDY`. This order tells the parser how to build a custom pattern for parsing a date.

**NOTE:** if a `Custom` format or a `FromCulture` format with a custom culture is used and it has a different year-month-day order than the system-current culture format, the parser will likely recognize the date as the one matching a custom format first and will not try the built-in format, which can lead to unexpected results. So, it is recommended that the <xref:NCalc.AdvExpressionOptions.SkipBuiltInDateSeparator> flag is set when you use a format type different from `BuiltIn`. To put it simply, don't mix the regular and US formats in one configuration.

### Time Formats

<xref:NCalc.AdvancedExpressionOptions> let one specify, which format to use for parsing times. 

The <xref:NCalc.AdvancedExpressionOptions.TimeSeparatorType> property lets you choose between 
* `BuiltIn` : parsing of times is done as if there were no AdvancedExpressionOptions set, i.e., times are expected in #H:m:s# format and the string is parsed using current culture.
* `CurrentCulture` : the separator defined in the current culture (which is either CultureInfo.CurrentCulture or a custom culture that you specify in the constructor or the <xref:NCalc.AdvancedExpressionOptions.CultureInfo> property) is used.
* `Custom` : the separator defined in the <xref:NCalc.AdvancedExpressionOptions.TimeSeparator> property

When <xref:NCalc.AdvancedExpressionOptions.TimeSeparatorType> is set to `CurrentCulture` or `Custom`, the parser will try to parse the value using the corresponding format and optionally try the built-in format. Whether the built-in format is tried or skipped in this case, is defined by the <xref:NCalc.AdvExpressionOptions.SkipBuiltInTimeSeparator> flag. To skip the built-in format, include the <xref:NCalc.AdvExpressionOptions.SkipBuiltInTimeSeparator> flag to the <xref:NCalc.AdvancedExpressionOptions.Flags> property of an instance of the <xref:NCalc.AdvancedExpressionOptions> class.

#### 12-hour and 24-hour times

<xref:NCalc.AdvancedExpressionOptions> let one specify, whether to use 12-hour time format.

The <xref:NCalc.AdvancedExpressionOptions.HoursFormat> property lets you choose between 
* `BuiltIn` : The ShortTimeFormat property of the CultureInfo.CurrentCulture's date/time information is used to determine if 12-hour format is to be used. 
* `CurrentCulture` : The ShortTimeFormat property of the  date/time information of either the custom culture or CultureInfo.CurrentCulture is used to determine if 12-hour format is to be used. 
* `Always12Hour` : use 12-hour format
* `Always24Hour` : use 24-hour format

When handling the 12-hour format, the parser will try values with and without a space before the am/pm value and with short (a/p) and normal (am/pm forms). This algorithm applies to all types of date formats including the built-in parsing.

## Parsing of Humane Date and Period Expressions

<xref:NCalc.AdvancedExpressionOptions> makes it possible to write current and relative dates and time periods in a humane form as a set of numbers and period identifiers. Example: "#5 days ago#" or "#in 3 days#" for dates relative to today, or "#321 yr 3 weeks 35 s#" for a time period. In all cases, spaces are ignored, so "#321yr3weeks35s#" will work equally well. 

The result of such parsing is expressed as a DateTime or a TimeSpan respectively, suitable for date and timespan operations (see the next section).

The expression #today# evaluates to current local date with time set to 00:00, while the expression #now# evaluates to current local date and time. In both cases, the result is an instance of DateTime.

To enable Humane Period Expressions, include the <xref:NCalc.AdvExpressionOptions.ParseHumanePeriods> flag to the <xref:NCalc.AdvancedExpressionOptions.Flags> property of an instance of the <xref:NCalc.AdvancedExpressionOptions> class:
```c#
var expression = new NCalc.Expression("#321 yr 3 weeks 35 s#");
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags = AdvExpressionOptions.ParseHumanePeriods;
```

You can use standard english words and abbreviations for periods:
```c#
readonly List<string> _periodYearIndicators = ["years", "year", "yrs", "yr", "y"];
readonly List<string> _periodMonthIndicators = ["months", "month", "mon", "mos", "mo"];
readonly List<string> _periodWeekIndicators = ["weeks", "week", "wks", "wk", "w"];
readonly List<string> _periodDayIndicators = ["days", "day", "d"];
readonly List<string> _periodHourIndicators = ["hours", "hour", "hrs", "hr", "h"];
readonly List<string> _periodMinuteIndicators = ["minutes", "minute", "mins", "min", "m"];
readonly List<string> _periodSecondIndicators = ["seconds", "second", "secs", "sec", "s"];
readonly List<string> _periodMSecIndicators = ["msec", "ms"];
```

For current and relative dates, the following time relation indicators are recognized by default:
```c#

readonly List<string> _periodNowIndicators = ["now"];
readonly List<string> _periodTodayIndicators = ["today"];

readonly List<string> _periodPastIndicators = ["before", "earlier", "ago"];
readonly List<string> _periodFutureIndicators = ["after", "in", "later"];
```

And you can also replace or amend those indicators with localized ones. Notes:
1. More "complete" words must have precedence over abbreviations. Otherwise, the parser will match a shorter form and leave the rest to the next parser, which will fail.
2. Use all lowercase - the parser will take a lowercase form of the parsed text and compare it with indicators in a case-sensitive manner for efficiency.
3. (for time period indicators only) Don't use the ending dot in abbreviations - the parser is aware of it and will skip it if the dot is present at the end. 

Here's how to add German indicators and their abbreviations:
```c#
expression.AdvancedOptions.PeriodYearIndicators.Add("jahre"); 
expression.AdvancedOptions.PeriodYearIndicators.Add("jahr");
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
```

## Basic Calculations with Dates and Time Spans

This version of NCalc supports basic operations (add, subtract) between a DateTime and a TimeSpan, as well as between two TimeSpans. Also, one can subtract one DateTime from another DateTime.
To enable date and timespan calculations, include the <xref:NCalc.AdvExpressionOptions.SupportTimeOperations> flag to the <xref:NCalc.AdvancedExpressionOptions.Flags> property of an instance of the <xref:NCalc.AdvancedExpressionOptions> class:
```c#
var expression = new NCalc.Expression("#11:00:00# - #3:00:00#");
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= NCalc.AdvExpressionOptions.SupportTimeOperations;
```

When the calculations are enabled, you can write expressions like "#11:00:00# - #3:00:00#" or "#01/01/2001# + #1yr 3mon 5days#", and they will produce a new TimeSpan or DateTime depending on the type of the left operand.

## Currency Support 

This version of NCalc supports parsing of currency values (i.e., the numbers accompanied by the currency symbol, the currency name, and, in the case of Euro, by "EUR" specifically). 
To enable currency support, include the <xref:NCalc.AdvExpressionOptions.AcceptCurrencySymbol> flag to the <xref:NCalc.AdvancedExpressionOptions.Flags> property of an instance of the <xref:NCalc.AdvancedExpressionOptions> class:
```c#
var expression = new NCalc.Expression("10000000 EUR / 2");
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= NCalc.AdvExpressionOptions.AcceptCurrencySymbol;
```

The parser supports three currency symbols (used for a currency symbol, a currency name, and optionally, for a currency name with a dot). Characters are matched case-insensitively, i.e. "Eur", "EUR" and "eur" will all work equally.

The <xref:NCalc.AdvancedExpressionOptions.CurrencySymbolsType> type property lets you choose between 
* `CurrentCulture` : The symbol defined in CultureInfo.CurrentCulture is used. If the symbol is for Euro and EUR is not detected as a second symbol, "EUR" is used as a third symbol.
* `FromCulture` : The symbol defined in the current culture (which is either CultureInfo.CurrentCulture or a custom culture that you specify in the constructor or the `CultureInfo` property) is used. If the symbol is for Euro and EUR is not detected as a second symbol, "EUR" is used as a third symbol.
* `Custom` : Two symbols can be set via the <xref:NCalc.AdvancedExpressionOptions.CurrencySymbol>, <xref:NCalc.AdvancedExpressionOptions.CurrencySymbol2>,and <xref:NCalc.AdvancedExpressionOptions.CurrencySymbol3> properties.

## Underscores in Numbers and Currency

This version of NCalc supports underscore characters (`_`) in numeric literals. Such characters are treated as whitespace and are stripped when the value is converted into a number. Modern programming languages support this notation for better readability of large numbers.

**NOTE:** Support for underscores requires [a custom version of the Parlot parser as provided by Allied Bits Ltd](https://github.com/Allied-Bits-Ltd/parlot) in the `ws_in_num2` or `ABCalc` branches.

To enable underscores in numbers, include the <xref:NCalc.AdvExpressionOptions.AcceptUnderscoresInNumbers> flag to the <xref:NCalc.AdvancedExpressionOptions.Flags> property of an instance of the <xref:NCalc.AdvancedExpressionOptions> class:

```c#
var expression = new NCalc.Expression("100_000 * 2 + 0x_DEAD_BEAF");
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= NCalc.AdvExpressionOptions.AcceptUnderscoresInNumbers;
```

## Custom Decimal and Group Separators in Numbers and Currency

<xref:NCalc.AdvancedExpressionOptions> let one specify, which character to use for a decimal separator and a number group separator when parsing numbers or currency values. 

The <xref:NCalc.AdvancedExpressionOptions.DecimalSeparatorType> and <xref:NCalc.AdvancedExpressionOptions.CurrencyDecimalSeparatorType> properties lets you choose between 
* `BuiltIn` : the separator defined in the default parser (Parlot), which is a dot (`.`)
* `CurrentCulture` : the separator defined in the current culture (which is either CultureInfo.CurrentCulture or a custom culture that you specify in the constructor or the `CultureInfo` property) 
* `Custom` : the separator defined in the `DecimalSeparator` and `CurrencyDecimalSeparator` property respectively

The <xref:NCalc.AdvancedExpressionOptions.NumberGroupSeparatorType> and <xref:NCalc.AdvancedExpressionOptions.CurrencyNumberGroupSeparatorType> property lets you choose between 
* `BuiltIn` : the separator defined in the default parser (Parlot), which is a comma (`,`)
* `CurrentCulture` : the separator defined in the current culture (which is either CultureInfo.CurrentCulture or a custom culture that you specify in the constructor or the `CultureInfo` property) 
* `Custom` : the separator defined in the <xref:NCalc.AdvancedExpressionOptions.NumberGroupSeparator> and <xref:NCalc.AdvancedExpressionOptions.CurrencyNumberGroupSeparator> property respectively

A number group separator may be empty.

## C-Style Octal Literals

This version of NCalc supports the `C` notation for octal numbers, where a literal that starts with zero and contains only digits 0-7 is considered to be an octal number.

To enable C-style octal literals, include the <xref:NCalc.AdvExpressionOptions.AcceptCStyleOctals> flag to the <xref:NCalc.AdvancedExpressionOptions.Flags> property of an instance of the <xref:NCalc.AdvancedExpressionOptions> class:

```c#
var expression = new NCalc.Expression("0100"); // produces 64
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= NCalc.AdvExpressionOptions.AcceptCStyleOctals;
```

## Result Reference Character

A Result Reference is a handy way for a user to refernce the result of the previous calculation, when it needs to be included into an expression multiple times.
The reference is inserted as a `@` character. A calling code should handle the <xref:NCalc.Expression.EvaluateFunction> event of an expression and provide an appropriate value when the function name is `@`.

To enable the result reference, include the <xref:NCalc.AdvExpressionOptions.UseResultReference> flag to the <xref:NCalc.AdvancedExpressionOptions.Flags> property of an instance of the <xref:NCalc.AdvancedExpressionOptions> class and handle the <xref:NCalc.Expression.EvaluateFunction> event of the <xref:NCalc.Expression> class:

```c#
var expression = new NCalc.Expression("@");
expression.AdvancedOptions = new AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= AdvExpressionOptions.UseResultReference;
expression.EvaluateFunction += (string name, NCalc.Handlers.FunctionArgs args) => { if (name.Equals("@")) args.Result = 42; };
```

## Percent Calculations
This version of NCalc supports operations with percent. To enable percent calculations, include the <xref:NCalc.AdvExpressionOptions.CalculatePercent> flag to the <xref:NCalc.AdvancedExpressionOptions.Flags> property of an instance of the <xref:NCalc.AdvancedExpressionOptions> class:

```c#
var expression = new NCalc.Expression("10 * 2%");
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= NCalc.AdvExpressionOptions.CalculatePercent;
```

The following operations with percent are supported:
* `a * b%` : Multiply a by b per cent ( a * b / 100 )
* `a / b%` : Multiply a by b per cent ( a * 100 / b )
* `a + b%` : Add b per cent to a ( a  + ( a * b + 100 )
* `a - b%` : Subtract b per cent from  a ( a  - ( a * b + 100 )

* `a% * b` : multiply the numeric value of percent a by b ( a * b ) with a result becoming a percent. E.g.: 5% * 2 = 10%
* `a% / b` : divide the numeric value of percent a by b ( a * b ) with a result becoming a percent. E.g.: 10% / 2 = 5%
* `a% + b%` : add the numeric value of percent b to the numeric value of percent a ( a + b ) with a result becoming a percent. E.g.: 5% + 2% = 7%
* `a% - b%` : subtract the numeric value of percent b from the numeric value of percent a ( a - b ) with a result becoming a percent. E.g.: 5% - 2% = 3%

Operations that produce percent as a result return an instance of the `Percent` type, whose `Value` property contains the value of a percent (e.g., 5 for 5% and so on). 