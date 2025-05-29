#define UNDERSCORE_IN_DECIMALS

using NCalc.Domain;
using NCalc.Exceptions;

using Parlot;
using Parlot.Fluent;

using static Parlot.Fluent.Parsers;

using Identifier = NCalc.Domain.Identifier;

namespace NCalc.Parser;

/// <summary>
/// Class responsible for parsing strings into <see cref="LogicalExpression"/> objects.
/// </summary>
public static class LogicalExpressionParser
{
    private static Parser<LogicalExpression> Parser;

    private static readonly ValueExpression True = new(true);
    private static readonly ValueExpression False = new(false);

    private static readonly double MinDecDouble = (double)decimal.MinValue;
    private static readonly double MaxDecDouble = (double)decimal.MaxValue;

    private const string InvalidTokenMessage = "Invalid token in expression";

    class NCalcFormatProvider : IFormatProvider
    {
        public object GetFormat(Type? formatType)
        {
            if (formatType?.Equals(typeof(DateTimeFormatInfo)) == true)
            {
                return CultureInfo.CurrentCulture.DateTimeFormat;
            }
            else
            {
                return null!;
            }
        }
    }

    static LogicalExpressionParser()
    {
        // InternalInit sets Parser (as before), and then we set it again here to satisfy the compiler's requirements
        Parser = InternalInit(ExpressionOptions.None, ExtendedExpressionOptions.DefaultOptions);
    }

    /// <summary>
    /// Creates the parser with the options that exist at the moment of call
    /// </summary>
    public static void ReInit()
    {
        Parser = InternalInit();
    }

    /// <summary>
    /// Creates the parser with the options that exist at the moment of call
    /// </summary>
    /// <returns>An instance of the newly created parser</returns>
    private static Parser<LogicalExpression> InternalInit()
    {
        return InternalInit(ExpressionOptions.None, ExtendedExpressionOptions.DefaultOptions);
    }

    private static Parser<LogicalExpression> InternalInit(ExpressionOptions options, ExtendedExpressionOptions extOptions)
    {
        /*
         * Grammar:
         * expression     => ternary ( ( "-" | "+" ) ternary )* ;
         * ternary        => logical ( "?" logical ":" logical)?
         * logical        => equality ( ( "and" | "or" ) equality )* ;
         * equality       => relational ( ( "=" | "!=" | ... ) relational )* ;
         * relational     => shift ( ( ">=" | ">" | ... ) shift )* ;
         * shift          => additive ( ( "<<" | ">>" ) additive )* ;
         * additive       => multiplicative ( ( "-" | "+" ) multiplicative )* ;
         * multiplicative => exponential ( "/" | "*" | "%") exponential )* ;
         * exponential    => unary ( "**" ) unary )* ;
         * unary          => ( "-" | "not" | "!" ) primary
         *
         * primary        => NUMBER
         *                  | STRING
         *                  | "true"
         *                  | "false"
         *                  | ("[" | "{") anything ("]" | "}")
         *                  | function
         *                  | list
         *                  | "(" expression ")" ;
         *
         * function       => Identifier "(" arguments ")"
         * arguments      => expression ( ("," | ";") expression )*
         */
        // The Deferred helper creates a parser that can be referenced by others before it is defined
        var expression = Deferred<LogicalExpression>();

        Parser<long> hexNumber;

        if (options.HasFlag(ExpressionOptions.AcceptUnderscoresInNumbers))
        {
            hexNumber = Terms.Text("0x")
            .SkipAnd(Terms.Pattern(c => "0123456789abcdefABCDEF_".Contains(c)))
            .Then(x => Convert.ToInt64(x.ToString()?.Replace("_", ""), 16));
        }
        else
        {
            hexNumber = Terms.Text("0x")
            .SkipAnd(Terms.Pattern(c => "0123456789abcdefABCDEF".Contains(c)))
            .Then(x => Convert.ToInt64(x.ToString(), 16));
        }

        Parser<long> octalNumber;
        if (options.HasFlag(ExpressionOptions.AcceptUnderscoresInNumbers))
        {
            octalNumber = Terms.Text("0o")
                .SkipAnd(Terms.Pattern(c => "01234567_".Contains(c)))
                .Then(x => Convert.ToInt64(x.ToString()?.Replace("_", ""), 8));
        }
        else
        {
            octalNumber = Terms.Text("0o")
                .SkipAnd(Terms.Pattern(c => "01234567".Contains(c)))
                .Then(x => Convert.ToInt64(x.ToString(), 8));
        }

        Parser<long> binaryNumber;
        if (options.HasFlag(ExpressionOptions.AcceptUnderscoresInNumbers))
        {
            binaryNumber = Terms.Text("0b")
                .SkipAnd(Terms.Pattern(c => c == '0' || c == '1' || c == '_'))
                .Then(x => Convert.ToInt64(x.ToString()?.Replace("_", ""), 2));
        }
        else
        {
            binaryNumber = Terms.Text("0b")
                .SkipAnd(Terms.Pattern(c => c == '0' || c == '1'))
                .Then(x => Convert.ToInt64(x.ToString(), 2));
        }

        var hexOctBinNumber = OneOf(hexNumber, octalNumber, binaryNumber)
                .Then<LogicalExpression>(d =>
                {
                    if (d is > int.MaxValue or < int.MinValue)
                        return new ValueExpression(d);

                    return new ValueExpression((int)d);
                });

        char decimalSeparator = extOptions.GetDecimalSeparatorChar(); // this method will return the default separator, if needed
        char numGroupSeparator = extOptions.GetNumberGroupSeparatorChar(); // this method will return the default separator, if needed

        var intNumber = Terms.Number<int>(NumberOptions.Integer
#if UNDERSCORE_IN_DECIMALS
            | (options.HasFlag(ExpressionOptions.AcceptUnderscoresInNumbers) ? NumberOptions.AllowUnderscore : 0)
#endif
            , decimalSeparator, numGroupSeparator)
            .AndSkip(Not(OneOf(Terms.Text("."), Terms.Text("E", true))))
            .Then<LogicalExpression>(d => new ValueExpression(d));

        var longNumber = Terms.Number<long>(NumberOptions.Integer
#if UNDERSCORE_IN_DECIMALS
            | (options.HasFlag(ExpressionOptions.AcceptUnderscoresInNumbers) ? NumberOptions.AllowUnderscore : 0)
#endif
            , decimalSeparator, numGroupSeparator)
            .AndSkip(Not(OneOf(Terms.Text("."), Terms.Text("E", true))))
            .Then<LogicalExpression>(d => new ValueExpression(d));

        var decimalNumber = Terms.Number<decimal>(NumberOptions.Float
#if UNDERSCORE_IN_DECIMALS
            | (options.HasFlag(ExpressionOptions.AcceptUnderscoresInNumbers) ? NumberOptions.AllowUnderscore : 0)
#endif
            , decimalSeparator, numGroupSeparator)
            .Then<LogicalExpression>(static (ctx, val) =>
            {
                bool useDecimal = ((LogicalExpressionParserContext)ctx).Options.HasFlag(ExpressionOptions.DecimalAsDefault);
                if (useDecimal)
                    return new ValueExpression(val);

                return new ValueExpression((double)val);
            });

        var doubleNumber = Terms.Number<double>(NumberOptions.Float
#if UNDERSCORE_IN_DECIMALS
            | (options.HasFlag(ExpressionOptions.AcceptUnderscoresInNumbers) ? NumberOptions.AllowUnderscore : 0)
#endif
            , decimalSeparator, numGroupSeparator)
            .Then<LogicalExpression>(static (ctx, val) =>
            {
                bool useDecimal = ((LogicalExpressionParserContext)ctx).Options.HasFlag(ExpressionOptions.DecimalAsDefault);
                if (useDecimal)
                {
                    if (val > MaxDecDouble)
                        return new ValueExpression(double.PositiveInfinity);

                    if (val < MinDecDouble)
                        return new ValueExpression(double.NegativeInfinity);

                    return new ValueExpression((decimal)val);
                }

                return new ValueExpression(val);
            });

        var decimalOrDoubleNumber = OneOf(decimalNumber, doubleNumber);

        var percent = Terms.Char('%');

        Parser<LogicalExpression>? numberPercent = null;

        if (options.HasFlag(ExpressionOptions.CalculatePercent))
        {
            numberPercent = OneOf(intNumber, longNumber, decimalNumber, doubleNumber)
                .And(percent)
                .Then<LogicalExpression>(static (ctx, val) =>
                {
                    //todo: write a rule for percent properly
                    return new PercentExpression(val.Item1);
                });
        }

        var comma = Terms.Char(',');
        var divided = Terms.Text("/");
        var times = Terms.Text("*");
        var modulo = options.HasFlag(ExpressionOptions.CalculatePercent) ? Terms.Text("mod", true) : Terms.Text("%");
        var minus = Terms.Text("-");
        var plus = Terms.Text("+");

        var equal = OneOf(Terms.Text("=="), Terms.Text("="));
        var notEqual = OneOf(Terms.Text("<>"), Terms.Text("!="));
        var @in = Terms.Text("in", true);
        var notIn = Terms.Text("not in", true);

        var like = Terms.Text("like", true);
        var notLike = Terms.Text("not like", true);

        var greater = Terms.Text(">");
        var greaterOrEqual = Terms.Text(">=");
        var lesser = Terms.Text("<");
        var lesserOrEqual = Terms.Text("<=");

        var leftShift = Terms.Text("<<");
        var rightShift = Terms.Text(">>");

        var exponent = Terms.Text("**");
        var openParen = Terms.Char('(');
        var closeParen = Terms.Char(')');
        var openBrace = Terms.Char('[');
        var closeBrace = Terms.Char(']');
        var openCurlyBrace = Terms.Char('{');
        var closeCurlyBrace = Terms.Char('}');
        var questionMark = Terms.Char('?');
        var colon = Terms.Char(':');
        var semicolon = Terms.Char(';');

        var identifier = Terms.Identifier();

        var not = OneOf(
            Terms.Text("NOT", true).AndSkip(OneOf(Literals.WhiteSpace().Or(Not(AnyCharBefore(openParen))))),
            Terms.Text("!"));
        var and = OneOf(Terms.Text("AND", true), Terms.Text("&&"));
        var or = OneOf(Terms.Text("OR", true), Terms.Text("||"));

        var bitwiseAnd = Terms.Text("&");
        var bitwiseOr = Terms.Text("|");
        var bitwiseXOr = Terms.Text("^");
        var bitwiseNot = Terms.Text("~");

        // "(" expression ")"
        var groupExpression = Between(openParen, expression, closeParen);

        var braceIdentifier = openBrace
            .SkipAnd(AnyCharBefore(closeBrace, consumeDelimiter: true, failOnEof: true).ElseError("Brace not closed."));

        var curlyBraceIdentifier =
            openCurlyBrace.SkipAnd(AnyCharBefore(closeCurlyBrace, consumeDelimiter: true, failOnEof: true)
                .ElseError("Brace not closed."));

        // ("[" | "{") identifier ("]" | "}")
        var identifierExpression = OneOf(
                braceIdentifier,
                curlyBraceIdentifier,
                identifier)
            .Then<LogicalExpression>(x => new Identifier(x.ToString()!));

        // list => "(" (expression ("," expression)*)? ")"
        var populatedList =
            Between(openParen, Separated(comma.Or(semicolon), expression),
                    closeParen.ElseError("Parenthesis not closed."))
                .Then<LogicalExpression>(values => new LogicalExpressionList(values));

        var emptyList = openParen.AndSkip(closeParen).Then<LogicalExpression>(_ => new LogicalExpressionList());

        var list = OneOf(emptyList, populatedList);

        var function = identifier
            .And(list)
            .Then<LogicalExpression>(static x =>
                new Function(new Identifier(x.Item1.ToString()!), (LogicalExpressionList)x.Item2));

        var booleanTrue = Terms.Text("true", true)
            .Then<LogicalExpression>(True);
        var booleanFalse = Terms.Text("false", true)
            .Then<LogicalExpression>(False);

        var singleQuotesStringValue =
            Terms.String(quotes: StringLiteralQuotes.Single)
                .Then<LogicalExpression>(static (ctx, value) =>
                {
                    if (value.Length == 1 &&
                        ((LogicalExpressionParserContext)ctx).Options.HasFlag(ExpressionOptions.AllowCharValues))
                    {
                        return new ValueExpression(value.Span[0]);
                    }

                    return new ValueExpression(value.ToString());
                });

        var doubleQuotesStringValue =
            Terms
                .String(quotes: StringLiteralQuotes.Double)
                .Then<LogicalExpression>(value => new ValueExpression(value.ToString()!));

        var stringValue = OneOf(singleQuotesStringValue, doubleQuotesStringValue);

        var charIsNumber = Literals.Pattern(char.IsNumber);
        var charIsNumberWithWhitespace = Terms.Pattern(char.IsNumber);

        DateTimeFormatInfo dateTimeFormat = extOptions?.GetFormat(typeof(DateTimeFormatInfo)) as DateTimeFormatInfo ?? CultureInfo.CurrentCulture.DateTimeFormat;

        Sequence<TextSpan, TextSpan, TextSpan> dateDefinition;

        Parser<LogicalExpression> date;

        string[] ncalcDateMasks = new string[2];
        string[] ncalcDateTimeMasks = new string[2];
        string[] ncalcDateShortTimeMasks = new string[2];
        CultureInfo culture = CultureInfo.CurrentCulture;

        string datePattern = culture.DateTimeFormat.ShortDatePattern;
        if (string.IsNullOrEmpty(datePattern))
        {
            ncalcDateMasks[0] = "d/M/yyyy";
            ncalcDateMasks[1] = "d/M/yy";
        }
        else
            switch (datePattern[0])
            {
                case 'd':
                    ncalcDateMasks[0] = "d/M/yyyy";
                    ncalcDateMasks[1] = "d/M/yy";
                    break;
                case 'M':
                    ncalcDateMasks[0] = "M/d/yyyy";
                    ncalcDateMasks[1] = "M/d/yy";
                    break;
                case 'y':
                    ncalcDateMasks[0] = "yyyy/M/d";
                    ncalcDateMasks[1] = "yy/M/d";
                    break;
                default:
                    ncalcDateMasks[0] = "d/M/yyyy";
                    ncalcDateMasks[1] = "d/M/yy";
                    break;
            }

        ncalcDateTimeMasks[0] = string.Join(" ", ncalcDateMasks[0], "h:m:s");
        ncalcDateTimeMasks[1] = string.Join(" ", ncalcDateMasks[1], "h:m:s");
        ncalcDateShortTimeMasks[0] = string.Join(" ", ncalcDateMasks[0], "h:m");
        ncalcDateShortTimeMasks[1] = string.Join(" ", ncalcDateMasks[1], "h:m");

        bool useSecondDate = false;
        bool onlyCustomDateTranslation = false;
        string customDateSep = "/";

        if (extOptions != null && extOptions.DateSeparatorType != ExtendedExpressionOptions.SeparatorType.BuiltIn)
        {
            customDateSep = extOptions.GetDateSeparator();
            if (customDateSep != "/" && !options.HasFlag(ExpressionOptions.SkipBuiltInDateSeparator))
                useSecondDate = true; // we use the second date separator when both custom separator and the default slash are enabled
            else
            if (customDateSep == "/")
            {
                onlyCustomDateTranslation = true;
            }
        }

        var secondDateSep = Terms.Text(customDateSep); // this may be a custom separator or "/"
        if (useSecondDate)
        {
            if (customDateSep.Contains(' '))
            {
                // If the date separator contains spaces (sk-SK, we salute you), we need to let people enter both "12.05.2025" and "12. 05. 2025"
                // And for this, we use a third separator - a trimmed version of the one we have from the culture info or custom settings.
                var thirdDateSep = Terms.Text(customDateSep.Trim());
                dateDefinition = charIsNumber
                    .AndSkip(OneOf(divided, secondDateSep, thirdDateSep))
                    .And(charIsNumber)
                    .AndSkip(OneOf(divided, secondDateSep, thirdDateSep))
                    .And(charIsNumber);
            }
            else
            {
                dateDefinition = charIsNumber
                    .AndSkip(OneOf(divided, secondDateSep))
                    .And(charIsNumber)
                    .AndSkip(OneOf(divided, secondDateSep))
                    .And(charIsNumber);
            }
        }
        else
        {
            dateDefinition = charIsNumber
                .AndSkip(secondDateSep)
                .And(charIsNumber)
                .AndSkip(secondDateSep)
                .And(charIsNumber);
        }

        // date => number/number/number or custom
        date = dateDefinition.Then<LogicalExpression>(date =>
        {
            if (useSecondDate || onlyCustomDateTranslation)
            {
                if (DateTime.TryParse($"{date.Item1}{customDateSep}{date.Item2}{customDateSep}{date.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                {
                    return new ValueExpression(result);
                }
            }
            if (useSecondDate || !onlyCustomDateTranslation)
            {
                // Use the existing ncalc approach with the current culture
                if (DateTime.TryParseExact($"{date.Item1}/{date.Item2}/{date.Item3}", ncalcDateMasks, new NCalcFormatProvider(), DateTimeStyles.None, out var result))
                {
                    return new ValueExpression(result);
                }
            }

            throw new FormatException("Invalid DateTime format.");
        });

        Sequence<TextSpan, TextSpan, TextSpan, string>? time12Definition = null;
        Sequence<TextSpan, TextSpan, TextSpan> timeDefinition;
        Sequence<TextSpan, TextSpan, string>? shortTime12Definition = null;
        Sequence<TextSpan, TextSpan> shortTimeDefinition;

        bool use12HourTime = dateTimeFormat.ShortTimePattern.Contains("tt");

        Parser<String>? amTimeIndicator = use12HourTime ? Terms.Text(dateTimeFormat.AMDesignator, true) : null;
        Parser<String>? pmTimeIndicator = use12HourTime ? Terms.Text(dateTimeFormat.PMDesignator, true) : null;

        bool useSecondTime = false;
        bool onlyCustomTimeTranslation = false;
        string customTimeSep = ":";

        if (extOptions != null && extOptions.TimeSeparatorType != ExtendedExpressionOptions.SeparatorType.BuiltIn)
        {
            customTimeSep = extOptions.TimeSeparator;
            if (customTimeSep != ":" && !options.HasFlag(ExpressionOptions.SkipBuiltInTimeSeparator))
                useSecondTime = true; // we use the second date separator when both custom separator and the default slash are enabled
            else
            if (customTimeSep == ":")
            {
                onlyCustomTimeTranslation = true;
            }
        }

        var secondTimeSep = Terms.Text(customTimeSep); // this may be a custom separator or ":"
        if (useSecondTime)
        {
            if (customDateSep.Contains(' '))
            {
                // If the time separator by chance contains spaces, we need to let people enter both "10:10:00" and "10: 10: 00"
                // And for this, we use a third separator - a trimmed version of the one we have from the culture info.
                var thirdTimeSep = Terms.Text(customDateSep.Trim());
                if (use12HourTime)
                {
                    time12Definition = charIsNumber
                        .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                        .And(charIsNumber)
                        .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                        .And(OneOf(charIsNumber, charIsNumberWithWhitespace))
                        .And(OneOf(amTimeIndicator!, pmTimeIndicator!));
                    shortTime12Definition = charIsNumber
                        .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                        .And(OneOf(charIsNumber, charIsNumberWithWhitespace))
                        .And(OneOf(amTimeIndicator!, pmTimeIndicator!));
                }

                timeDefinition = charIsNumber
                    .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                    .And(charIsNumber)
                    .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                    .And(charIsNumber);
                shortTimeDefinition = charIsNumber
                    .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                    .And(charIsNumber);
            }
            else
            {
                if (use12HourTime)
                {
                    time12Definition = charIsNumber
                        .AndSkip(OneOf(divided, secondTimeSep))
                        .And(charIsNumber)
                        .AndSkip(OneOf(divided, secondTimeSep))
                        .And(OneOf(charIsNumber, charIsNumberWithWhitespace))
                        .And(OneOf(amTimeIndicator!, pmTimeIndicator!));
                    shortTime12Definition = charIsNumber
                        .AndSkip(OneOf(divided, secondTimeSep))
                        .And(OneOf(charIsNumber, charIsNumberWithWhitespace))
                        .And(OneOf(amTimeIndicator!, pmTimeIndicator!));
                }

                timeDefinition = charIsNumber
                    .AndSkip(OneOf(divided, secondTimeSep))
                    .And(charIsNumber)
                    .AndSkip(OneOf(divided, secondTimeSep))
                    .And(charIsNumber);

                shortTimeDefinition = charIsNumber
                    .AndSkip(OneOf(divided, secondTimeSep))
                    .And(charIsNumber);
            }
        }
        else
        {
            if (use12HourTime)
            {
                time12Definition = charIsNumber
                    .AndSkip(secondTimeSep)
                    .And(charIsNumber)
                    .AndSkip(secondTimeSep)
                    .And(OneOf(charIsNumber, charIsNumberWithWhitespace))
                    .And(OneOf(amTimeIndicator!, pmTimeIndicator!));
                shortTime12Definition = charIsNumber
                    .AndSkip(secondTimeSep)
                    .And(OneOf(charIsNumber, charIsNumberWithWhitespace))
                    .And(OneOf(amTimeIndicator!, pmTimeIndicator!));
            }

            // time => number:number:number
            timeDefinition = charIsNumber
                .AndSkip(secondTimeSep)
                .And(charIsNumber)
                .AndSkip(secondTimeSep)
                .And(charIsNumber);

            shortTimeDefinition = charIsNumber
                .AndSkip(secondTimeSep)
                .And(charIsNumber);
        }

        Parser<LogicalExpression>? time12 = null;
        Parser<LogicalExpression>? shortTime12 = null;

        var time = timeDefinition.Then<LogicalExpression>(time =>
        {
            if (useSecondTime || onlyCustomTimeTranslation)
            {
                if (DateTime.TryParse($"{time.Item1}{customTimeSep}{time.Item2}{customTimeSep}{time.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                {
                    return new ValueExpression(result);
                }
            }
            if (useSecondTime || !onlyCustomTimeTranslation)
            {
                if (TimeSpan.TryParse($"{time.Item1}:{time.Item2}:{time.Item3}", out var result))
                {
                    return new ValueExpression(result);
                }
            }

            throw new FormatException("Invalid TimeSpan format.");
        });

        var shortTime = shortTimeDefinition.Then<LogicalExpression>(time =>
        {
            if (useSecondTime || onlyCustomTimeTranslation)
            {
                if (DateTime.TryParse($"{time.Item1}{customTimeSep}{time.Item2}", dateTimeFormat, DateTimeStyles.None, out var result))
                {
                    return new ValueExpression(result);
                }
            }
            if (useSecondTime || !onlyCustomTimeTranslation)
            {
                if (TimeSpan.TryParse($"{time.Item1}:{time.Item2}", out var result))
                {
                    return new ValueExpression(result);
                }
            }

            throw new FormatException("Invalid TimeSpan format.");
        });

        if (use12HourTime)
        {
            string amSpacer = "";
            if (dateTimeFormat.ShortTimePattern.Contains(" tt"))
                amSpacer = " ";

            time12 = time12Definition!.Then<LogicalExpression>(time =>
            {
                if (useSecondTime || onlyCustomTimeTranslation)
                {
                    if (DateTime.TryParse($"{time.Item1}{customTimeSep}{time.Item2}{customTimeSep}{time.Item3}{amSpacer}{time.Item4}", dateTimeFormat, DateTimeStyles.None, out var result))
                    {
                        return new ValueExpression(result);
                    }
                }
                if (useSecondTime || !onlyCustomTimeTranslation)
                {
                    if (TimeSpan.TryParse($"{time.Item1}:{time.Item2}:{time.Item3}{amSpacer}{time.Item4}", out var result))
                    {
                        return new ValueExpression(result);
                    }
                }

                throw new FormatException("Invalid TimeSpan format.");
            });

            shortTime12 = shortTime12Definition!.Then<LogicalExpression>(time =>
            {
                if (useSecondTime || onlyCustomTimeTranslation)
                {
                    if (DateTime.TryParse($"{time.Item1}{customTimeSep}{time.Item2}{amSpacer}{time.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                    {
                        return new ValueExpression(result);
                    }
                }
                if (useSecondTime || !onlyCustomTimeTranslation)
                {
                    if (TimeSpan.TryParse($"{time.Item1}:{time.Item2}{amSpacer}{time.Item3}", out var result))
                    {
                        return new ValueExpression(result);
                    }
                }

                throw new FormatException("Invalid TimeSpan format.");
            });
        }

        // dateAndTime => number/number/number number:number:number or custom
        var dateAndTime = dateDefinition.AndSkip(Literals.WhiteSpace()).And(timeDefinition).Then<LogicalExpression>(
            dateTime =>
            {
                if (useSecondDate || onlyCustomDateTranslation)
                {
                    if (useSecondTime || onlyCustomTimeTranslation)
                    {
                        if (DateTime.TryParse($"{dateTime.Item1}{customDateSep}{dateTime.Item2}{customDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSep}{dateTime.Item4.Item2}{customTimeSep}{dateTime.Item4.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                        {
                            return new ValueExpression(result);
                        }
                    }
                    if (useSecondTime || !onlyCustomTimeTranslation)
                    {
                        if (DateTime.TryParse($"{dateTime.Item1}{customDateSep}{dateTime.Item2}{customDateSep}{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}:{dateTime.Item4.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                        {
                            return new ValueExpression(result);
                        }
                    }
                }
                if (useSecondDate || !onlyCustomDateTranslation)
                {
                    if (useSecondTime || onlyCustomTimeTranslation)
                    {
                        if (DateTime.TryParse($"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSep}{dateTime.Item4.Item2}{customTimeSep}{dateTime.Item4.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                        {
                            return new ValueExpression(result);
                        }
                    }

                    if (useSecondTime || !onlyCustomTimeTranslation)
                    {
                        // Use the existing approach
                        if (DateTime.TryParseExact($"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}:{dateTime.Item4.Item3}", ncalcDateTimeMasks, new NCalcFormatProvider(), DateTimeStyles.None, out var result))
                        {
                            return new ValueExpression(result);
                        }
                    }
                }

                throw new FormatException("Invalid DateTime format.");
            });

        var dateAndShortTime = dateDefinition.AndSkip(Literals.WhiteSpace()).And(shortTimeDefinition).Then<LogicalExpression>(
            dateTime =>
            {
                if (useSecondDate || onlyCustomDateTranslation)
                {
                    if (useSecondTime || onlyCustomTimeTranslation)
                    {
                        if (DateTime.TryParse($"{dateTime.Item1}{customDateSep}{dateTime.Item2}{customDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSep}{dateTime.Item4.Item2}", dateTimeFormat, DateTimeStyles.None, out var result))
                        {
                            return new ValueExpression(result);
                        }
                    }
                    if (useSecondTime || !onlyCustomTimeTranslation)
                    {
                        if (DateTime.TryParse($"{dateTime.Item1}{customDateSep}{dateTime.Item2}{customDateSep}{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}", dateTimeFormat, DateTimeStyles.None, out var result))
                        {
                            return new ValueExpression(result);
                        }
                    }
                }
                if (useSecondDate || !onlyCustomDateTranslation)
                {
                    if (useSecondTime || onlyCustomTimeTranslation)
                    {
                        if (DateTime.TryParse($"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSep}{dateTime.Item4.Item2}{customTimeSep}{dateTime.Item4}", dateTimeFormat, DateTimeStyles.None, out var result))
                        {
                            return new ValueExpression(result);
                        }
                    }

                    if (useSecondTime || !onlyCustomTimeTranslation)
                    {
                        // Use the existing approach
                        if (DateTime.TryParseExact($"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}", ncalcDateShortTimeMasks, new NCalcFormatProvider(), DateTimeStyles.None, out var result))
                        {
                            return new ValueExpression(result);
                        }
                    }
                }

                throw new FormatException("Invalid DateTime format.");
            });

        Parser<LogicalExpression>? dateAndTime12 = null;
        Parser<LogicalExpression>? dateAndShortTime12 = null;

        if (use12HourTime)
        {
            string amSpacer = "";
            if (dateTimeFormat.ShortTimePattern.Contains(" tt"))
                amSpacer = " ";

            dateAndTime12 = dateDefinition.AndSkip(Literals.WhiteSpace()).And(time12Definition!).Then<LogicalExpression>(
                dateTime =>
                {
                    if (useSecondDate || onlyCustomDateTranslation)
                    {
                        if (useSecondTime || onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}{customDateSep}{dateTime.Item2}{customDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSep}{dateTime.Item4.Item2}{customTimeSep}{dateTime.Item4.Item3}{amSpacer}{dateTime.Item4.Item4}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }
                        if (useSecondTime || !onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}{customDateSep}{dateTime.Item2}{customDateSep}{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}:{dateTime.Item4.Item3}{amSpacer}{dateTime.Item4.Item4}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }
                    }
                    if (useSecondDate || !onlyCustomDateTranslation)
                    {
                        if (useSecondTime || onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSep}{dateTime.Item4.Item2}{customTimeSep}{dateTime.Item4.Item3}{amSpacer}{dateTime.Item4.Item4}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }

                        if (useSecondTime || !onlyCustomTimeTranslation)
                        {
                            // Use the existing approach
                            if (DateTime.TryParse($"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}:{dateTime.Item4.Item3}{amSpacer}{dateTime.Item4.Item4}", out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }
                    }

                    throw new FormatException("Invalid DateTime format.");
                });

            dateAndShortTime12 = dateDefinition.AndSkip(Literals.WhiteSpace()).And(shortTime12Definition!).Then<LogicalExpression>(
                dateTime =>
                {
                    if (useSecondDate || onlyCustomDateTranslation)
                    {
                        if (useSecondTime || onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}{customDateSep}{dateTime.Item2}{customDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSep}{dateTime.Item4.Item2}{amSpacer}{dateTime.Item4.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }
                        if (useSecondTime || !onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}{customDateSep}{dateTime.Item2}{customDateSep}{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}{amSpacer}{dateTime.Item4.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }
                    }
                    if (useSecondDate || !onlyCustomDateTranslation)
                    {
                        if (useSecondTime || onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSep}{dateTime.Item4.Item2}{amSpacer}{dateTime.Item4.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }

                        if (useSecondTime || !onlyCustomTimeTranslation)
                        {
                            // Use the existing approach
                            if (DateTime.TryParse($"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}{amSpacer}{dateTime.Item4.Item3}", out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }
                    }

                    throw new FormatException("Invalid DateTime format.");
                });
        }

        // datetime => '#' dateAndTime | date | shortTime | time  '#';
        SequenceAndSkip<LogicalExpression, char> dateTime;
        if (use12HourTime)
        {
            dateTime = Terms
            .Char('#')
            .SkipAnd(OneOf(dateAndTime12!, dateAndShortTime12!, dateAndTime, dateAndShortTime, date, time, shortTime))
            .AndSkip(Literals.Char('#'));
        }
        else
        {
            dateTime = Terms
                .Char('#')
                .SkipAnd(OneOf(dateAndTime, dateAndShortTime, date, time, shortTime))
                .AndSkip(Literals.Char('#'));
        }

        var isHexDigit = Character.IsHexDigit;

        var eightHexSequence = Terms
            .Pattern(isHexDigit, 8, 8);

        var fourHexSequence = Terms
            .Pattern(isHexDigit, 4, 4);

        var twelveHexSequence = Terms
            .Pattern(isHexDigit, 12, 12);

        var thirtyTwoHexSequence = Terms
            .Pattern(isHexDigit, 32, 32);

        var guidWithHyphens = eightHexSequence
                .AndSkip(minus)
                .And(fourHexSequence)
                .AndSkip(minus)
                .And(fourHexSequence)
                .AndSkip(minus)
                .And(fourHexSequence)
                .AndSkip(minus)
                .And(twelveHexSequence)
            .Then<LogicalExpression>(static g =>
                    new ValueExpression(Guid.Parse(g.Item1.ToString() + g.Item2 + g.Item3 + g.Item4 + g.Item5)));

        var guidWithoutHyphens = thirtyTwoHexSequence
            .AndSkip(Not(decimalOrDoubleNumber))
            .Then<LogicalExpression>(static g => new ValueExpression(Guid.Parse(g.ToString()!)));

        var guid = OneOf(guidWithHyphens, guidWithoutHyphens);

        // primary => GUID | NUMBER | identifier| DateTime | string | function | boolean | groupExpression | list ;

        var primary = OneOf(
            guid,
            hexOctBinNumber,
            intNumber,
            longNumber,
            decimalOrDoubleNumber,
            booleanTrue,
            booleanFalse,
            dateTime,
            stringValue,
            function,
            groupExpression,
            identifierExpression,
            list);

        // exponential => unary ( "**" unary )* ;
        var exponential = primary.And(ZeroOrMany(exponent.And(primary)))
            .Then(static x =>
            {
                LogicalExpression result = null!;

                switch (x.Item2.Count)
                {
                    case 0:
                        result = x.Item1;
                        break;
                    case 1:
                        result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item1, x.Item2[0].Item2);
                        break;
                    default:
                    {
                        for (int i = x.Item2.Count - 1; i > 0; i--)
                        {
                            result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item2[i - 1].Item2,
                                x.Item2[i].Item2);
                        }

                        result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item1, result);
                        break;
                    }
                }

                return result;
            });

        // ( "-" | "not" ) unary | primary;
        var unary = exponential.Unary(
            (not, static value => new UnaryExpression(UnaryExpressionType.Not, value)),
            (minus, static value => new UnaryExpression(UnaryExpressionType.Negate, value)),
            (bitwiseNot, static value => new UnaryExpression(UnaryExpressionType.BitwiseNot, value))
        );

        // multiplicative => unary ( ( "/" | "*" | "%" ) unary )* ;
        var multiplicative = unary.LeftAssociative(
            (divided, static (a, b) => new BinaryExpression(BinaryExpressionType.Div, a, b)),
            (times, static (a, b) => new BinaryExpression(BinaryExpressionType.Times, a, b)),
            (modulo, static (a, b) => new BinaryExpression(BinaryExpressionType.Modulo, a, b))
        );

        // additive => multiplicative ( ( "-" | "+" ) multiplicative )* ;
        var additive = multiplicative.LeftAssociative(
            (plus, static (a, b) => new BinaryExpression(BinaryExpressionType.Plus, a, b)),
            (minus, static (a, b) => new BinaryExpression(BinaryExpressionType.Minus, a, b))
        );

        // shift => additive ( ( "<<" | ">>" ) additive )* ;
        var shift = additive.LeftAssociative(
            (leftShift, static (a, b) => new BinaryExpression(BinaryExpressionType.LeftShift, a, b)),
            (rightShift, static (a, b) => new BinaryExpression(BinaryExpressionType.RightShift, a, b))
        );

        // relational => shift ( ( ">=" | "<=" | "<" | ">" | "in" | "not in" ) shift )* ;
        var relational = shift.And(ZeroOrMany(OneOf(
                    greaterOrEqual.Then(BinaryExpressionType.GreaterOrEqual),
                    lesserOrEqual.Then(BinaryExpressionType.LesserOrEqual),
                    lesser.Then(BinaryExpressionType.Lesser),
                    greater.Then(BinaryExpressionType.Greater),
                    @in.Then(BinaryExpressionType.In),
                    notIn.Then(BinaryExpressionType.NotIn),
                    like.Then(BinaryExpressionType.Like),
                    notLike.Then(BinaryExpressionType.NotLike)
                )
                .And(shift)))
            .Then(ParseBinaryExpression);

        var equality = relational.And(ZeroOrMany(OneOf(
                    equal.Then(BinaryExpressionType.Equal),
                    notEqual.Then(BinaryExpressionType.NotEqual))
                .And(relational)))
            .Then(ParseBinaryExpression);

        var andTypeParser = and.Then(BinaryExpressionType.And)
            .Or(bitwiseAnd.Then(BinaryExpressionType.BitwiseAnd));

        var orTypeParser = or.Then(BinaryExpressionType.Or)
            .Or(bitwiseOr.Then(BinaryExpressionType.BitwiseOr));

        var xorTypeParser = bitwiseXOr.Then(BinaryExpressionType.BitwiseXOr);

        // "and" has higher precedence than "or"
        var andParser = equality.And(ZeroOrMany(andTypeParser.And(equality)))
            .Then(ParseBinaryExpression);

        var orParser = andParser.And(ZeroOrMany(orTypeParser.And(andParser)))
            .Then(ParseBinaryExpression);

        // logical => equality ( ( "and" | "or" | "xor" ) equality )* ;
        var logical = orParser.And(ZeroOrMany(xorTypeParser.And(orParser)))
            .Then(ParseBinaryExpression);

        // ternary => logical("?" logical ":" logical) ?
        var ternary = logical.And(ZeroOrOne(questionMark.SkipAnd(logical).AndSkip(colon).And(logical)))
            .Then(static x => x.Item2.Item1 == null
                ? x.Item1
                : new TernaryExpression(x.Item1, x.Item2.Item1, x.Item2.Item2))
            .Or(logical);

        var operatorSequence = ternary.LeftAssociative(
            (OneOrMany(OneOf(
                    divided, times, modulo, plus,
                    minus, leftShift, rightShift, greaterOrEqual,
                    lesserOrEqual, greater, lesser, equal,
                    notEqual)),
                static (_, _) => throw new InvalidOperationException("Unknown operator sequence.")));

        expression.Parser = operatorSequence;
        var expressionParser = expression.AndSkip(ZeroOrMany(Literals.WhiteSpace(true))).Eof()
            .ElseError(InvalidTokenMessage);

        AppContext.TryGetSwitch("NCalc.EnableParlotParserCompilation", out var enableParserCompilation);

        Parser<LogicalExpression>? result = enableParserCompilation ? expressionParser.Compile() : expressionParser;
        return result;
    }

    private static LogicalExpression ParseBinaryExpression((LogicalExpression, IReadOnlyList<(BinaryExpressionType, LogicalExpression)>) x)
    {
        var result = x.Item1;

        foreach (var op in x.Item2)
        {
            result = new BinaryExpression(op.Item1, result, op.Item2);
        }

        return result;
    }

    public static LogicalExpression Parse(LogicalExpressionParserContext context)
    {
        Parser<LogicalExpression> parserToUse;
        if (context.ExtendedOptions is not null)
            parserToUse = InternalInit(context.Options, context.ExtendedOptions);
        else
            parserToUse = Parser;

        if (parserToUse.TryParse(context, out var result, out var error))
            return result;

        string message;
        if (error != null)
            message = $"{error.Message} at position {error.Position}";
        else
            message = $"Error parsing the expression at position {context.Scanner.Cursor.Position}";

        throw new NCalcParserException(message);
    }
}