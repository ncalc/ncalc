# Advanced Value Formats and Operations

Some of the behavior and support for advanced parsing features is controlled by the advanced options. This includes

* Advanced date and time parsing
* Currency support
* Underscores in numbers and currency
* Custom decimal and group separators in numbers and currency
* C-Style octal literals
* Result Reference character
* Percent calculations

Advanced options are configured by assigning an instance of the `AdvancedExpressionOptions` class to the 
`AdvancedOptions` property of the expression you create and adjusting its properties:

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

AdvancedExpressionOptions let one specify, which format to use for parsing dates. 

The `DateSeparatorType` property lets you choose between 
* `BuiltIn` : parsing of dates is done as if there were no AdvancedExpressionOptions set, i.e., dates are expected in #X/Y/Z# format where current culture defines what X, Y, and Z are (i.e., which one is day, month, and year).
* `FromCulture` : the separator defined in the current culture (which is either CultureInfo.CurrentCulture or a custom culture that you specify in the constructor or the `CultureInfo` property) is used.
* `Custom` : the separator defined in the `DateSeparator` property

When `DateSeparatorType` is set to `FromCulture` or `Custom`, the parser will try to parse the value using the corresponding format and optionally try the built-in format. Whether the built-in format is tried or skipped in this case, is defined by the `SkipBuiltInDateSeparator` flag. To skip the built-in format, include the `SkipBuiltInDateSeparator` flag to the `Flags` property of an instance of the `AdvancedExpressionOptions` class.

```c#
var expression = new NCalc.Expression("10 * 2%");
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= NCalc.AdvExpressionOptions.SkipBuiltInDateSeparator;
```

#### Order of Values in Dates

When `DateSeparatorType` is set to `FromCulture`, the order of day, month, and year parts is defined by the culture. When it is set to `Custom`, the order is defined by the `DateOrder` property and can be one of `YMD`, `DMY`, or `MDY`. This order tells the parser how to build a custom pattern for parsing a date.

**NOTE:** if a `Custom` format or a `FromCulture` format with a custom culture is used and it has a different year-month-day order than the system-current culture format, the parser will likely recognize the date as the one matching a custom format first and will not try the built-in format, which can lead to unexpected results. So, it is recommended that the `SkipBuiltInDateSeparator` flag is set when you use a format type different from `BuiltIn`. To put it simply, don't mix the regular and US formats in one configuration.

### Time Formats

AdvancedExpressionOptions let one specify, which format to use for parsing times. 

The `TimeSeparatorType` property lets you choose between 
* `BuiltIn` : parsing of times is done as if there were no AdvancedExpressionOptions set, i.e., times are expected in #H:m:s# format and the string is parsed using current culture.
* `CurrentCulture` : the separator defined in the current culture (which is either CultureInfo.CurrentCulture or a custom culture that you specify in the constructor or the `CultureInfo` property) is used.
* `Custom` : the separator defined in the `TimeSeparator` property

When `TimeSeparatorType` is set to `CurrentCulture` or `Custom`, the parser will try to parse the value using the corresponding format and optionally try the built-in format. Whether the built-in format is tried or skipped in this case, is defined by the `SkipBuiltInTimeSeparator` flag. To skip the built-in format, include the `SkipBuiltInTimeSeparator` flag to the `Flags` property of an instance of the `AdvancedExpressionOptions` class.

#### 12-hour and 24-hour times

AdvancedExpressionOptions let one specify, whether to use 12-hour time format.

The `HoursFormat` property lets you choose between 
* `BuiltIn` : The ShortTimeFormat property of the CultureInfo.CurrentCulture's date/time information is used to determine if 12-hour format is to be used. 
* `CurrentCulture` : The ShortTimeFormat property of the  date/time information of either the custom culture or CultureInfo.CurrentCulture is used to determine if 12-hour format is to be used. 
* `Always12Hour` : use 12-hour format
* `Always24Hour` : use 24-hour format

When handling the 12-hour format, the parser will try values with and without a space before the am/pm value and with short (a/p) and normal (am/pm forms). This algorithm applies to all types of date formats including the built-in parsing.

## Currency support 

This version of NCalc supports parsing of currency values (i.e., the numbers accompanied by the currency symbol or, in the case of Euro, by "EUR"). 
To enable currency support, include the `AcceptCurrencySymbol ` flag to the `Flags` property of an instance of the `AdvancedExpressionOptions` class:

```c#
var expression = new NCalc.Expression("$10000000 / 2");
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= NCalc.AdvExpressionOptions.AcceptCurrencySymbol;
```

The parser supports three currency symbols (used for a currency symbol, a currency name, and optionally, for a currency name with a dot).

The `CurrencySymbolsType` type property lets you choose between 
* `CurrentCulture` : The symbol defined in CultureInfo.CurrentCulture is used. If the symbol is for Euro and EUR is not detected as a second symbol, "EUR" is used as a third symbol.
* `FromCulture` : The symbol defined in the current culture (which is either CultureInfo.CurrentCulture or a custom culture that you specify in the constructor or the `CultureInfo` property) is used. If the symbol is for Euro and EUR is not detected as a second symbol, "EUR" is used as a third symbol.
* `Custom` : Two symbols can be set via the `CurrencySymbol`, `CurrencySymbol2`, and `CurrencySymbol3`.

## Underscores in numbers and currency

This version of NCalc supports underscore characters (`_`) in numeric literals. Such characters are treated as whitespace and are stripped when the value is converted into a number. Modern programming languages support this notation for better readability of large numbers.

**NOTE:** Support for underscores requires [a custom version of the Parlot parser as provided by Allied Bits Ltd](https://github.com/Allied-Bits-Ltd/parlot).

To enable underscores in numbers, include the `AcceptUnderscoresInNumbers` flag to the `Flags` property of an instance of the `AdvancedExpressionOptions` class:

```c#
var expression = new NCalc.Expression("100_000 * 2 + 0x_DEAD_BEAF");
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= NCalc.AdvExpressionOptions.AcceptUnderscoresInNumbers;
```

## Custom Decimal and Group Separators in Numbers and Currency

AdvancedExpressionOptions let one specify, which character to use for a decimal separator and a number group separator when parsing numbers or currency values. 

The `DecimalSeparatorType` and `CurrencyDecimalSeparatorType` properties lets you choose between 
* `BuiltIn` : the separator defined in the default parser (Parlot), which is a dot (`.`)
* `CurrentCulture` : the separator defined in the current culture (which is either CultureInfo.CurrentCulture or a custom culture that you specify in the constructor or the `CultureInfo` property) 
* `Custom` : the separator defined in the `DecimalSeparator` and `CurrencyDecimalSeparator` property respectively

The `NumberGroupSeparatorType` and `CurrencyNumberGroupSeparatorType` property lets you choose between 
* `BuiltIn` : the separator defined in the default parser (Parlot), which is a comma (`,`)
* `CurrentCulture` : the separator defined in the current culture (which is either CultureInfo.CurrentCulture or a custom culture that you specify in the constructor or the `CultureInfo` property) 
* `Custom` : the separator defined in the `NumberGroupSeparator` and `CurrencyNumberGroupSeparator` property respectively

A number group separator may be empty.

## C-Style Octal Literals

This version of NCalc supports the `C` notation for octal numbers, where a literal that starts with zero and contains only digits 0-7 is considered to be an octal number.

To enable C-style octal literals, include the `AcceptCStyleOctals` flag to the `Flags` property of an instance of the `AdvancedExpressionOptions` class:

```c#
var expression = new NCalc.Expression("0100"); // produces 64
expression.AdvancedOptions = new NCalc.AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= NCalc.AdvExpressionOptions.AcceptCStyleOctals;
```

## Result Reference Character

A Result Reference is a handy way for a user to refernce the result of the previous calculation, when it needs to be included into an expression multiple times.
The reference is inserted as a `@` character. A calling code should handle the EvaluateFunction event of an expression and provide an appropriate value when the function name is `@`.

To enable the result reference, include the `UseResultReference` flag to the `Flags` property of an instance of the `AdvancedExpressionOptions` class and handle the `EvaluateFunction` event of the `Expression` class:

```c#
var expression = new NCalc.Expression("@");
expression.AdvancedOptions = new AdvancedExpressionOptions();
expression.AdvancedOptions.Flags |= AdvExpressionOptions.UseResultReference;
expression.EvaluateFunction += (string name, NCalc.Handlers.FunctionArgs args) => { if (name.Equals("@")) args.Result = 42; };
```

## Percent calculations
This version of NCalc supports operations with percent. To enable percent calculations, include the `CalculatePercent` flag to the `Flags` property of an instance of the `AdvancedExpressionOptions` class:

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

