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
    private static readonly ConcurrentDictionary<CultureInfo, Parser<LogicalExpression>> Parsers = new();

    private static readonly ValueExpression True = new(true);
    private static readonly ValueExpression False = new(false);

    private static readonly double MinDecDouble = (double)decimal.MinValue;
    private static readonly double MaxDecDouble = (double)decimal.MaxValue;

    private const string InvalidTokenMessage = "Invalid token in expression";

    // Support of underscores in decimal literals requires a patch in Parlot,
    // currently available in https://github.com/Allied-Bits-Ltd/parlot
    // and offered to the main project as a pull request https://github.com/sebastienros/parlot/pull/221

    private static readonly bool _hasAllowUnderscore = Enum.GetNames(typeof(NumberOptions)).Contains("AllowUnderscore");

    const string errFailedToParsePeriodIndicator = "Failed to parse the element '{0}' of a period definition.";
    const string errDuplicatePeriodIndicator = "Period indicator '{0}' has been already used in the period definition";
    const string errUnrecognizedPeriodIndicator = "Unrecognized period indicator '{0}' in the period definition.";
    const string errUnrecognizedTimeRelationIndicator = "Unrecognized time relation indicator '{0}' in the date/time definition.";
    const string errDuplicateTimeRelationIndicators = "A date/time may contain only one time relation indicator, but two ('{0}' and '{1}') were specified.";

    class CurrentCultureDateTimeFormatProvider : IFormatProvider
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

    private static IFormatProvider _currentCultureFormatProvider = new CurrentCultureDateTimeFormatProvider();

    static LogicalExpressionParser()
    {
        // InternalInit sets Parser (as before), and then we set it again here to satisfy the compiler's requirements
        Parsers[CultureInfo.CurrentCulture] = CreateExpressionParser(CultureInfo.CurrentCulture, ExpressionOptions.None, null /*AdvancedExpressionOptions.DefaultOptions*/);
    }

    /// <summary>
    /// Creates the parser with the options that exist at the moment of call
    /// </summary>
    public static void ReInitialize()
    {
        Parsers[CultureInfo.CurrentCulture] = CreateExpressionParser();
    }

    /// <summary>
    /// Creates the parser with the options that exist at the moment of call
    /// </summary>
    /// <returns>An instance of the newly created parser</returns>
    private static Parser<LogicalExpression> CreateExpressionParser()
    {
        return CreateExpressionParser(CultureInfo.CurrentCulture, ExpressionOptions.None, null /*AdvancedExpressionOptions.DefaultOptions*/);
    }

    private static Parser<LogicalExpression> CreateExpressionParser(CultureInfo cultureInfo, ExpressionOptions options, AdvancedExpressionOptions? extOptions)
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
         * multiplicative => unary ( "/" | "*" | "%") unary )* ;
         * unary          => ( "-" | "not" | "!" ) exponential ;
         * exponential    => factorial ( "**" ) factorial )* ;
         * factorial      => primary ( "!" )* ;
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

        bool acceptUnderscores = (extOptions != null) && extOptions.Flags.HasFlag(AdvExpressionOptions.AcceptUnderscoresInNumbers);

        if (acceptUnderscores)
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
        if (acceptUnderscores)
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

        Parser<long> octalNumberCStyle;

        if (acceptUnderscores)
        {
            octalNumberCStyle = Terms.Text("0")//. And(Terms.AnyOf("01234567_"))
                .And(Terms.Pattern(c => "01234567_".Contains(c)))
                .Then(x => Convert.ToInt64(x.Item2.ToString()?.Replace("_", ""), 8));
        }
        else
        {
            octalNumberCStyle = Terms.Text("0")
                .And(Terms.Pattern(c => "01234567".Contains(c)))
                .Then(x => Convert.ToInt64(x.Item2.ToString(), 8));
        }

        Parser<long> binaryNumber;
        if (acceptUnderscores)
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

        Parser<long> hexOctBinNumberParser;

        if (extOptions != null && extOptions.Flags.HasFlag(AdvExpressionOptions.AcceptCStyleOctals))
            hexOctBinNumberParser = OneOf(octalNumberCStyle, hexNumber, octalNumber, binaryNumber);
        else
            hexOctBinNumberParser = OneOf(hexNumber, octalNumber, binaryNumber);

        var hexOctBinNumber = hexOctBinNumberParser.Then<LogicalExpression>(d =>
            {
                if (d is > int.MaxValue or < int.MinValue)
                    return new ValueExpression(d);

                return new ValueExpression((int)d);
            });

        char decimalSeparator = (extOptions != null) ? extOptions.GetDecimalSeparatorChar() : Parlot.Fluent.NumberLiterals.DefaultDecimalSeparator; // this method will return the default separator, if needed
        char numGroupSeparator = (extOptions != null) ? extOptions.GetNumberGroupSeparatorChar() : Parlot.Fluent.NumberLiterals.DefaultGroupSeparator; // this method will return the default separator, if needed

        NumberOptions useNumberGroupSeparatorFlag = ((extOptions != null) && (numGroupSeparator != '\0')) ? NumberOptions.AllowGroupSeparators : NumberOptions.None;
        NumberOptions useUnderscoreFlag = (_hasAllowUnderscore && extOptions != null && extOptions.Flags.HasFlag(AdvExpressionOptions.AcceptUnderscoresInNumbers)) ? (NumberOptions)16 : NumberOptions.None;

        var intNumber = Terms.Number<int>(NumberOptions.Integer | useNumberGroupSeparatorFlag | useUnderscoreFlag, decimalSeparator, numGroupSeparator)
            .AndSkip(Not(OneOf(Terms.Text(decimalSeparator.ToString()), Terms.Text("E", true))))
            .Then<LogicalExpression>(d => new ValueExpression(d));

        var longNumber = Terms.Number<long>(NumberOptions.Integer | useNumberGroupSeparatorFlag | useUnderscoreFlag, decimalSeparator, numGroupSeparator)
            .AndSkip(Not(OneOf(Terms.Text(decimalSeparator.ToString()), Terms.Text("E", true))))
            .Then<LogicalExpression>(d => new ValueExpression(d));

        var decimalNumber = Terms.Number<decimal>(NumberOptions.Float | useNumberGroupSeparatorFlag | useUnderscoreFlag, decimalSeparator, numGroupSeparator)
            .Then<LogicalExpression>(static (ctx, val) =>
            {
                bool useDecimal = ((LogicalExpressionParserContext)ctx).Options.HasFlag(ExpressionOptions.DecimalAsDefault);
                if (useDecimal)
                    return new ValueExpression(val);

                return new ValueExpression((double)val);
            });

        var doubleNumber = Terms.Number<double>(NumberOptions.Float | useNumberGroupSeparatorFlag | useUnderscoreFlag, decimalSeparator, numGroupSeparator)
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

        // Add currency support

        Parser<LogicalExpression>? currency = null;

        if (extOptions != null && extOptions.Flags.HasFlag(AdvExpressionOptions.AcceptCurrencySymbol))
        {
            string currencySymbol = string.Empty;
            string currencySymbol2 = string.Empty;
            string currencySymbol3 = string.Empty;

            extOptions.GetCurrencySymbols(out currencySymbol, out currencySymbol2, out currencySymbol3);

            if (!string.IsNullOrEmpty(currencySymbol) || !string.IsNullOrEmpty(currencySymbol2) || !string.IsNullOrEmpty(currencySymbol3))
            {
                char currencyDecimalSeparator = extOptions.GetCurrencyDecimalSeparatorChar(); // this method will return the default separator, if needed
                char currencyNumGroupSeparator = extOptions.GetCurrencyNumberGroupSeparatorChar(); // this method will return the default separator, if needed

                //Parser<string> currencyChar;
                List<Parser<string>> currencyChars = new List<Parser<string>>();

                if (!string.IsNullOrEmpty(currencySymbol)) currencyChars.Add(Terms.Text(currencySymbol, true));
                if (!string.IsNullOrEmpty(currencySymbol2)) currencyChars.Add(Terms.Text(currencySymbol2, true));
                if (!string.IsNullOrEmpty(currencySymbol3)) currencyChars.Add(Terms.Text(currencySymbol3, true));

                Parser<string>[] currencyCharsArray = currencyChars.ToArray();

                Parser<LogicalExpression>? currency1 = null;
                Parser<LogicalExpression>? currency2 = null;

                var decimalCurrencyNumber = Terms.Number<decimal>((NumberOptions.Float & ~NumberOptions.AllowExponent) | useNumberGroupSeparatorFlag | useUnderscoreFlag, currencyDecimalSeparator, currencyNumGroupSeparator)
                .Then<LogicalExpression>(static (ctx, val) =>
                {
                    bool useDecimal = ((LogicalExpressionParserContext)ctx).Options.HasFlag(ExpressionOptions.DecimalAsDefault);
                    if (useDecimal)
                        return new ValueExpression(val);

                    return new ValueExpression((double)val);
                });

                currency1 = OneOf(currencyCharsArray).SkipAnd(SkipWhiteSpace(OneOf(decimalCurrencyNumber, intNumber, longNumber)))
                    .Then<LogicalExpression>(static (ctx, val) =>
                    {
                        return val;
                    });

                currency2 = OneOf(decimalCurrencyNumber, intNumber, longNumber).AndSkip(SkipWhiteSpace(OneOf(currencyCharsArray)))
                    .Then<LogicalExpression>(static (ctx, val) =>
                    {
                        return val;
                    });

                currency = OneOf(currency1!, currency2!);
            }
        }

        // Add percent support

        bool useCharsForOps = !options.HasFlag(ExpressionOptions.SkipLogicalAndBitwiseOpChars);
        bool useUnicodeForOps = options.HasFlag(ExpressionOptions.UseUnicodeCharsForOperations);
        bool useAssignments = options.HasFlag(ExpressionOptions.UseAssignments);

        var percentChar = Terms.Char('%'); // CultureInfo defines a percent character, but we are yet to see another character than '%'

        var comma = Terms.Char(',');
        var divided = useUnicodeForOps ? OneOf(Terms.Text("/"), Terms.Text(":"), Terms.Text("\u00F7")) : Terms.Text("/");
        var times = useUnicodeForOps ? OneOf(Terms.Text("*"), Terms.Text("\u00D7"), Terms.Text("\u2219")) : Terms.Text("*");
        var modulo = (extOptions != null && extOptions.Flags.HasFlag(AdvExpressionOptions.CalculatePercent)) ? Terms.Text("mod", true) : Terms.Text("%");
        var minus = Terms.Text("-");
        var plus = Terms.Text("+");

        var equal = options.HasFlag(ExpressionOptions.UseCStyleAssignments) ? Terms.Text("==") : OneOf(Terms.Text("=="), Terms.Text("="));
        var notEqual = useUnicodeForOps ? OneOf(Terms.Text("<>"), Terms.Text("!="), Terms.Text("\u2260")) : OneOf(Terms.Text("<>"), Terms.Text("!="));
        var @in = useUnicodeForOps ? OneOf(Terms.Text("in", true), Terms.Text("\u2208")) : Terms.Text("in", true);
        var notIn = useUnicodeForOps ? OneOf(Terms.Text("not in", true), Terms.Text("\u2209")) : Terms.Text("not in", true);

        var like = Terms.Text("like", true);
        var notLike = Terms.Text("not like", true);

        var greater = Terms.Text(">");
        var greaterOrEqual = useUnicodeForOps ? OneOf(Terms.Text(">="), Terms.Text("\u2265")) : Terms.Text(">=");
        var less = Terms.Text("<");
        var lessOrEqual = useUnicodeForOps ? OneOf(Terms.Text("<="), Terms.Text("\u2264")) : Terms.Text("<=");

        var leftShift = Terms.Text("<<");
        var rightShift = Terms.Text(">>");

        var exponent = useUnicodeForOps
            ? (useCharsForOps
                    ? OneOf(Terms.Text("**"), Terms.Text("\u2291"))
                    : OneOf(Terms.Text("**"), Terms.Text("^"), Terms.Text("\u2291")))
            : (useCharsForOps
                    ? Terms.Text("**")
                    : OneOf(Terms.Text("**"), Terms.Text("^"))); // when useCharsForOps is true, caret is used for bitwise XOR
        var openParen = Terms.Char('(');
        var closeParen = Terms.Char(')');
        var openBrace = Terms.Char('[');
        var closeBrace = Terms.Char(']');
        var openCurlyBrace = Terms.Char('{');
        var closeCurlyBrace = Terms.Char('}');
        var questionMark = Terms.Char('?');
        var exclamationMark = Terms.Char('!');
        var colon = Terms.Char(':');
        var semicolon = Terms.Char(';');

        var dotChar = Terms.Char('.');

        var statementEnd = semicolon;

        Parser<string>? root2 = useCharsForOps ? Terms.Text("\u221A") : null;
#if NET8_0_OR_GREATER
        Parser<string>? root3 = useCharsForOps ? Terms.Text("\u221B") : null;
#endif
        Parser<string>? root4 = useCharsForOps ? Terms.Text("\u221C") : null;

        var resultRefChar = Terms.Char('@');

        var identifier = Terms.Identifier();

        Parser<string>? not;
        Parser<string>? and;
        Parser<string>? or;
        Parser<string>? xor;

        if (useCharsForOps)
        {
            and = useUnicodeForOps ? OneOf(Terms.Text("AND", true), Terms.Text("&&"), Terms.Text("\u2227")) : OneOf(Terms.Text("AND", true), Terms.Text("&&"));
            or = useUnicodeForOps ? OneOf(Terms.Text("OR", true), Terms.Text("||"), Terms.Text("\u2228")) : OneOf(Terms.Text("OR", true), Terms.Text("||"));
            not = useUnicodeForOps
                ? OneOf(Terms.Text("NOT", true).AndSkip(OneOf(Literals.WhiteSpace().Or(Not(AnyCharBefore(openParen))))), Terms.Text("!"), Terms.Text("\u00ac"))
                : OneOf(Terms.Text("NOT", true).AndSkip(OneOf(Literals.WhiteSpace().Or(Not(AnyCharBefore(openParen))))), Terms.Text("!"));
        }
        else
        {
            and = useUnicodeForOps ? OneOf(Terms.Text("AND", true), Terms.Text("\u2227")) : Terms.Text("AND", true);
            or = useUnicodeForOps ? OneOf(Terms.Text("OR", true), Terms.Text("\u2228")) : Terms.Text("OR", true);
            not = useUnicodeForOps
                ? OneOf(Terms.Text("NOT", true).AndSkip(OneOf(Literals.WhiteSpace().Or(Not(AnyCharBefore(openParen))))), Terms.Text("\u00ac"))
                : Terms.Text("NOT", true).AndSkip(OneOf(Literals.WhiteSpace().Or(Not(AnyCharBefore(openParen)))));
        }
        xor = useUnicodeForOps ? OneOf(Terms.Text("XOR", true), Terms.Text("\u2295"), Terms.Text("\u22BB")) : Terms.Text("XOR", true);

        var bitwiseAnd = useCharsForOps ? OneOf(Terms.Text("BIT_AND", true), Terms.Text("&")) : Terms.Text("BIT_AND", true);
        var bitwiseOr = useCharsForOps ? OneOf(Terms.Text("BIT_OR", true), Terms.Text("|"))  : Terms.Text("BIT_OR", true);
        var bitwiseXOr = useCharsForOps ? OneOf(Terms.Text("BIT_XOR", true), Terms.Text("^")) : Terms.Text("BIT_XOR", true);
        var bitwiseNot = useCharsForOps ? OneOf(Terms.Text("BIT_NOT", true), Terms.Text("~")) : Terms.Text("BIT_NOT", true);

        var assignmentOperator = useUnicodeForOps
                                    ? OneOf(Terms.Text("\u2254"),
                                            (options.HasFlag(ExpressionOptions.UseCStyleAssignments)
                                                ? Terms.Text("=")
                                                : Terms.Text(":=")))
                                    : options.HasFlag(ExpressionOptions.UseCStyleAssignments)
                                                ? Terms.Text("=")
                                                : Terms.Text(":=");

        var plusAssign = Terms.Text("+=");
        var minusAssign = Terms.Text("-=");
        var multiplyAssign = useUnicodeForOps ? OneOf(Terms.Text("*="), Terms.Text("\u00D7="), Terms.Text("\u2219=")) : Terms.Text("*=");
        var divAssign = Terms.Text("/=");
        var orAssign = Terms.Text("|=");
        var andAssign = Terms.Text("&=");
        var xorAssign = Terms.Text("^=");

        // "(" expression ")"
        var groupExpression = Between(openParen, expression, closeParen);

        var braceIdentifier = openBrace
            .SkipAnd(AnyCharBefore(closeBrace, consumeDelimiter: true, failOnEof: true).ElseError("Brace not closed."));

        var curlyBraceIdentifier =
            openCurlyBrace.SkipAnd(AnyCharBefore(closeCurlyBrace, consumeDelimiter: true, failOnEof: true)
                .ElseError("Brace not closed."));

        var resultReference = resultRefChar
            .Then<LogicalExpression>(static x =>
                new Function(new Identifier(x.ToString()!), new LogicalExpressionList()));

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

        Parser<LogicalExpression> functionOrResultRef;

        if (extOptions != null && extOptions.Flags.HasFlag(AdvExpressionOptions.UseResultReference))
            functionOrResultRef = OneOf(resultReference, function);
        else
            functionOrResultRef = function;

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

        // Add proper date and time support

        SequenceAndSkip<LogicalExpression, char>? dateTime = null;

        if (!options.HasFlag(ExpressionOptions.DontParseDates))
        {
            DateTimeFormatInfo dateTimeFormat = extOptions?.GetFormat(typeof(DateTimeFormatInfo)) as DateTimeFormatInfo ?? cultureInfo?.DateTimeFormat ?? CultureInfo.CurrentCulture.DateTimeFormat;

            Sequence<TextSpan, TextSpan, TextSpan> dateDefinition;

            Parser<LogicalExpression> date;

            // The following block prepares the masks for the approach to parsing used by ncalc by default -
            // parsing of "x/y/z" in dates with the current culture info (which will likely not work in some locales).
            // So, these masks below fix the format to be "x/y/z" in the order used by the current culture.
            string[] ncalcDateMasks = new string[2];
            string[] ncalcDateTimeMasks = new string[2];
            string[] ncalcDateShortTimeMasks = new string[2];
            string[] ncalcDateTime12Masks = new string[4];
            string[] ncalcDateShortTime12Masks = new string[4];

            CultureInfo culture = cultureInfo ?? CultureInfo.CurrentCulture;

            string builtInDateSep = (cultureInfo ?? CultureInfo.CurrentCulture).DateTimeFormat.DateSeparator;
            string builtInTimeSep = (cultureInfo ?? CultureInfo.CurrentCulture).DateTimeFormat.TimeSeparator;

            string datePattern = culture.DateTimeFormat.ShortDatePattern;
            if (string.IsNullOrEmpty(datePattern))
            {
                ncalcDateMasks[0] = string.Join(builtInDateSep, "d", "M", "yyyy");
                ncalcDateMasks[1] = string.Join(builtInDateSep, "d", "M", "yy");
            }
            else
                switch (datePattern[0])
                {
                    case 'd':
                        ncalcDateMasks[0] = string.Join(builtInDateSep, "d", "M", "yyyy");
                        ncalcDateMasks[1] = string.Join(builtInDateSep, "d", "M", "yy");
                        break;
                    case 'M':
                        ncalcDateMasks[0] = string.Join(builtInDateSep, "M", "d", "yyyy");
                        ncalcDateMasks[1] = string.Join(builtInDateSep, "M", "d", "yy");
                        break;
                    case 'y':
                        ncalcDateMasks[0] = string.Join(builtInDateSep, "yyyy", "M", "d");
                        ncalcDateMasks[1] = string.Join(builtInDateSep, "yy", "M", "d");
                        break;
                    default:
                        ncalcDateMasks[0] = string.Join(builtInDateSep, "d", "M", "yyyy");
                        ncalcDateMasks[1] = string.Join(builtInDateSep, "d", "M", "yy");
                        break;
                }

            // Define some masks for date-time values with both long and short time
            ncalcDateTimeMasks[0] = string.Join(" ", ncalcDateMasks[0], string.Join(builtInTimeSep, "H", "m", "s"));
            ncalcDateTimeMasks[1] = string.Join(" ", ncalcDateMasks[1], string.Join(builtInTimeSep, "H", "m", "s"));
            ncalcDateShortTimeMasks[0] = string.Join(" ", ncalcDateMasks[0], string.Join(builtInTimeSep, "H", "m"));
            ncalcDateShortTimeMasks[1] = string.Join(" ", ncalcDateMasks[1], string.Join(builtInTimeSep, "H", "m"));

            bool useSecondDate = false;
            bool onlyCustomDateTranslation = false;
            string customDateSep = builtInDateSep;

            if (extOptions != null)
            {
                customDateSep = extOptions.GetDateSeparator();
                if (customDateSep != builtInDateSep && !extOptions.Flags.HasFlag(AdvExpressionOptions.SkipBuiltInDateSeparator))
                    useSecondDate = true; // we use the second date separator when both custom separator and the default slash are enabled
                else
                if (customDateSep == builtInDateSep)
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
                string customDateSepForDT = dateTimeFormat.DateSeparator;
                if (useSecondDate || onlyCustomDateTranslation)
                {
                    if (DateTime.TryParse($"{date.Item1}{customDateSepForDT}{date.Item2}{customDateSepForDT}{date.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                    {
                        return new ValueExpression(result);
                    }
                }
                if (useSecondDate || !onlyCustomDateTranslation)
                {
                    // Use the existing ncalc approach with the current culture
                    if (DateTime.TryParseExact($"{date.Item1}{builtInDateSep}{date.Item2}{builtInDateSep}{date.Item3}", ncalcDateMasks, _currentCultureFormatProvider, DateTimeStyles.None, out var result))
                    {
                        return new ValueExpression(result);
                    }
                }

                throw new FormatException("Invalid DateTime format.");
            });

            Sequence<TextSpan, TextSpan, TextSpan, string>? time12Definition = null;
            Sequence<TextSpan, TextSpan, TextSpan> timeDefinition;
            Sequence<string, TextSpan, TextSpan, TextSpan, TextSpan> timeSpanDefinition;
            Sequence<TextSpan, TextSpan, string>? shortTime12Definition = null;
            Sequence<TextSpan, TextSpan> shortTimeDefinition;
            Sequence<string, TextSpan, TextSpan, TextSpan> shortTimeSpanDefinition;

            bool use12HourTime = (extOptions == null) ? dateTimeFormat.ShortTimePattern.Contains("t") : extOptions.Use12HourTime();

            Parser<string>? amTimeIndicator = use12HourTime ? Terms.Text(dateTimeFormat.AMDesignator, true) : null;
            Parser<string>? pmTimeIndicator = use12HourTime ? Terms.Text(dateTimeFormat.PMDesignator, true) : null;

            Parser<string>? amTimeIndicatorFirstChar = null;
            Parser<string>? pmTimeIndicatorFirstChar = null;

            string amTimeFirstChar = string.Empty;
            string pmTimeFirstChar = string.Empty;
            string amTimeFirstCharLower = string.Empty;
            string pmTimeFirstCharLower = string.Empty;

            if (use12HourTime)
            {
                if (!string.IsNullOrEmpty(dateTimeFormat.AMDesignator))
                {
                    amTimeFirstChar = dateTimeFormat.AMDesignator.Substring(0, 1);
                    amTimeFirstCharLower = dateTimeFormat.AMDesignator.Substring(0, 1).ToLower();

                    amTimeIndicatorFirstChar = Terms.Text(amTimeFirstChar, true);
                }
                if (!string.IsNullOrEmpty(dateTimeFormat.PMDesignator))
                {
                    pmTimeFirstChar = dateTimeFormat.PMDesignator.Substring(0, 1);
                    pmTimeFirstCharLower = dateTimeFormat.PMDesignator.Substring(0, 1).ToLower();
                    pmTimeIndicatorFirstChar = Terms.Text(pmTimeFirstChar, true);
                }

                ncalcDateTime12Masks[0] = string.Join(" ", ncalcDateMasks[0], "h:m:s t");
                ncalcDateTime12Masks[1] = string.Join(" ", ncalcDateMasks[1], "h:m:s t");
                ncalcDateTime12Masks[2] = string.Join(" ", ncalcDateMasks[0], "h:m:s tt");
                ncalcDateTime12Masks[3] = string.Join(" ", ncalcDateMasks[1], "h:m:s tt");
                ncalcDateShortTime12Masks[0] = string.Join(" ", ncalcDateMasks[0], "h:m t");
                ncalcDateShortTime12Masks[1] = string.Join(" ", ncalcDateMasks[1], "h:m t");
                ncalcDateShortTime12Masks[2] = string.Join(" ", ncalcDateMasks[0], "h:m tt");
                ncalcDateShortTime12Masks[3] = string.Join(" ", ncalcDateMasks[1], "h:m tt");
            }

            bool useSecondTime = false;
            bool onlyCustomTimeTranslation = false;
            string customTimeSep = builtInTimeSep;

            if (extOptions != null)
            {
                customTimeSep = extOptions.TimeSeparator;
                if (customTimeSep != builtInTimeSep && !extOptions.Flags.HasFlag(AdvExpressionOptions.SkipBuiltInTimeSeparator))
                    useSecondTime = true; // we use the second time separator when both custom separator and the default one are enabled and are different
                else
                if (customTimeSep == builtInTimeSep)
                {
                    onlyCustomTimeTranslation = true;
                }
            }

            var secondTimeSep = Terms.Text(customTimeSep); // this may be a custom separator or ":"
            if (useSecondTime)
            {
                if (customTimeSep.Contains(' '))
                {
                    // If the time separator by chance contains spaces, we need to let people enter both "10:10:00" and "10: 10: 00"
                    // And for this, we use a third separator - a trimmed version of the one we have from the culture info.
                    var thirdTimeSep = Terms.Text(customTimeSep.Trim());
                    if (use12HourTime)
                    {
                        time12Definition = charIsNumber
                            .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                            .And(charIsNumber)
                            .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                            .And(OneOf(charIsNumber, charIsNumberWithWhitespace))
                            .And(OneOf(amTimeIndicator!, pmTimeIndicator!, amTimeIndicatorFirstChar!, pmTimeIndicatorFirstChar!));
                        shortTime12Definition = charIsNumber
                            .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                            .And(OneOf(charIsNumber, charIsNumberWithWhitespace))
                            .And(OneOf(amTimeIndicator!, pmTimeIndicator!, amTimeIndicatorFirstChar!, pmTimeIndicatorFirstChar!));
                    }

                    timeDefinition = charIsNumber
                        .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                        .And(charIsNumber)
                        .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                        .And(charIsNumber);
                    shortTimeDefinition = charIsNumber
                        .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                        .And(charIsNumber);
                    timeSpanDefinition = ZeroOrOne(minus).And(ZeroOrOne(charIsNumber.AndSkip(dotChar))).And(charIsNumber)
                        .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                        .And(charIsNumber)
                        .AndSkip(OneOf(divided, secondTimeSep, thirdTimeSep))
                        .And(charIsNumber);
                    shortTimeSpanDefinition = ZeroOrOne(minus).And(ZeroOrOne(charIsNumber.AndSkip(dotChar))).And(charIsNumber)
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
                            .And(OneOf(amTimeIndicator!, pmTimeIndicator!, amTimeIndicatorFirstChar!, pmTimeIndicatorFirstChar!));
                        shortTime12Definition = charIsNumber
                            .AndSkip(OneOf(divided, secondTimeSep))
                            .And(OneOf(charIsNumber, charIsNumberWithWhitespace))
                            .And(OneOf(amTimeIndicator!, pmTimeIndicator!, amTimeIndicatorFirstChar!, pmTimeIndicatorFirstChar!));
                    }

                    timeDefinition = charIsNumber
                        .AndSkip(OneOf(divided, secondTimeSep))
                        .And(charIsNumber)
                        .AndSkip(OneOf(divided, secondTimeSep))
                        .And(charIsNumber);

                    shortTimeDefinition = charIsNumber
                        .AndSkip(OneOf(divided, secondTimeSep))
                        .And(charIsNumber);

                    timeSpanDefinition = ZeroOrOne(minus).And(ZeroOrOne((charIsNumber).AndSkip(dotChar))).And(charIsNumber)
                        .AndSkip(OneOf(divided, secondTimeSep))
                        .And(charIsNumber)
                        .AndSkip(OneOf(divided, secondTimeSep))
                        .And(charIsNumber);

                    shortTimeSpanDefinition = ZeroOrOne(minus).And(ZeroOrOne(charIsNumber.AndSkip(dotChar))).And(charIsNumber)
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
                        .And(OneOf(amTimeIndicator!, pmTimeIndicator!, amTimeIndicatorFirstChar!, pmTimeIndicatorFirstChar!));
                    shortTime12Definition = charIsNumber
                        .AndSkip(secondTimeSep)
                        .And(OneOf(charIsNumber, charIsNumberWithWhitespace))
                        .And(OneOf(amTimeIndicator!, pmTimeIndicator!, amTimeIndicatorFirstChar!, pmTimeIndicatorFirstChar!));
                }

                timeDefinition = charIsNumber
                    .AndSkip(secondTimeSep)
                    .And(charIsNumber)
                    .AndSkip(secondTimeSep)
                    .And(charIsNumber);

                shortTimeDefinition = charIsNumber
                    .AndSkip(secondTimeSep)
                    .And(charIsNumber);

                // timeSpan => [[-]number.]number:number:number
                timeSpanDefinition = ZeroOrOne(minus).And(ZeroOrOne((charIsNumber).AndSkip(dotChar))).And(charIsNumber)
                    .AndSkip(secondTimeSep)
                    .And(charIsNumber)
                    .AndSkip(secondTimeSep)
                    .And(charIsNumber);

                shortTimeSpanDefinition = ZeroOrOne(minus).And(ZeroOrOne(charIsNumber.AndSkip(dotChar))).And(charIsNumber)
                    .AndSkip(secondTimeSep)
                    .And(charIsNumber);
            }

            Parser<LogicalExpression>? time12 = null;
            Parser<LogicalExpression>? shortTime12 = null;

            var time = timeSpanDefinition.Then<LogicalExpression>(time =>
            {
                string customTimeSepForDT = dateTimeFormat.TimeSeparator;
                if (useSecondTime || onlyCustomTimeTranslation)
                {
                    if (DateTime.TryParse($"{time.Item3}{customTimeSepForDT}{time.Item4}{customTimeSepForDT}{time.Item5}", dateTimeFormat, DateTimeStyles.None, out var result))
                    {
                        TimeSpan tsResult = result.TimeOfDay;

                        if (time.Item2.Length > 0)
                        {
                            int days = Int32.Parse(time.Item2.Span.ToString());
                            tsResult = tsResult.Add(TimeSpan.FromDays(days));
                        }
                        if (time.Item1 == "-")
                        {
                            tsResult = TimeSpan.FromMilliseconds(-tsResult.TotalMilliseconds);
                        }
                        return new ValueExpression(tsResult);
                    }
                }
                if (useSecondTime || !onlyCustomTimeTranslation)
                {
                    if (TimeSpan.TryParse($"{time.Item3}{builtInTimeSep}{time.Item4}{builtInTimeSep}{time.Item5}", out var result))
                    {
                        TimeSpan tsResult = result;

                        if (time.Item2.Length > 0)
                        {
                            int days = Int32.Parse(time.Item2.Span.ToString());
                            tsResult = tsResult.Add(TimeSpan.FromDays(days));
                        }
                        if (time.Item1 == "-")
                        {
                            tsResult = TimeSpan.FromMilliseconds(-tsResult.TotalMilliseconds);
                        }
                        return new ValueExpression(tsResult);
                    }
                }

                throw new FormatException("Invalid TimeSpan format.");
            });

            var shortTime = shortTimeSpanDefinition.Then<LogicalExpression>(time =>
            {
                string customTimeSepForDT = dateTimeFormat.TimeSeparator;
                if (useSecondTime || onlyCustomTimeTranslation)
                {
                    if (DateTime.TryParse($"{time.Item3}{customTimeSepForDT}{time.Item4}", dateTimeFormat, DateTimeStyles.None, out var result))
                    {
                        TimeSpan tsResult = result.TimeOfDay;

                        if (time.Item2.Length > 0)
                        {
                            int days = Int32.Parse(time.Item2.Span.ToString());
                            tsResult = tsResult.Add(TimeSpan.FromDays(days));
                        }
                        if (time.Item1 == "-")
                        {
                            tsResult  = TimeSpan.FromMilliseconds(-tsResult.TotalMilliseconds);
                        }
                        return new ValueExpression(tsResult);
                    }
                }
                if (useSecondTime || !onlyCustomTimeTranslation)
                {
                    if (TimeSpan.TryParse($"{time.Item3}{builtInTimeSep}{time.Item4}", out var result))
                    {
                        TimeSpan tsResult = result;

                        if (time.Item2.Length > 0)
                        {
                            int days = Int32.Parse(time.Item2.Span.ToString());
                            tsResult = tsResult.Add(TimeSpan.FromDays(days));
                        }
                        if (time.Item1 == "-")
                        {
                            tsResult = TimeSpan.FromMilliseconds(-tsResult.TotalMilliseconds);
                        }
                        return new ValueExpression(tsResult);
                    }
                }

                throw new FormatException("Invalid TimeSpan format.");
            });

            if (use12HourTime)
            {
                string customTimeSepForDT = dateTimeFormat.TimeSeparator;
                string amSpacer = "";
                if (dateTimeFormat.ShortTimePattern.Contains(" t"))
                    amSpacer = " ";

                time12 = time12Definition!.Then<LogicalExpression>(time =>
                {
                    string amPMValue = time.Item4;
                    if (amPMValue.ToLower().Equals(amTimeFirstCharLower))
                        amPMValue = dateTimeFormat.AMDesignator;
                    else
                    if (amPMValue.ToLower().Equals(pmTimeFirstCharLower))
                        amPMValue = dateTimeFormat.PMDesignator;

                    if (useSecondTime || onlyCustomTimeTranslation)
                    {
                        if (DateTime.TryParse($"{time.Item1}{customTimeSepForDT}{time.Item2}{customTimeSepForDT}{time.Item3}{amSpacer}{amPMValue}", dateTimeFormat, DateTimeStyles.None, out var result))
                        {
                            return new ValueExpression(result.TimeOfDay);
                        }
                    }
                    if (useSecondTime || !onlyCustomTimeTranslation)
                    {
                        // Use the existing ncalc approach with the current culture
                        if (TimeSpan.TryParse($"{time.Item1}{builtInTimeSep}{time.Item2}{builtInTimeSep}{time.Item3}{amSpacer}{amPMValue}", out var result))
                        {
                            return new ValueExpression(result);
                        }
                    }

                    throw new FormatException("Invalid TimeSpan format.");
                });

                shortTime12 = shortTime12Definition!.Then<LogicalExpression>(time =>
                {
                    string customTimeSepForDT = dateTimeFormat.TimeSeparator;
                    string amPMValue = time.Item3;
                    if (amPMValue.ToLower().Equals(amTimeFirstCharLower))
                        amPMValue = dateTimeFormat.AMDesignator;
                    else
                    if (amPMValue.ToLower().Equals(pmTimeFirstCharLower))
                        amPMValue = dateTimeFormat.PMDesignator;

                    if (useSecondTime || onlyCustomTimeTranslation)
                    {
                        if (DateTime.TryParse($"{time.Item1}{customTimeSepForDT}{time.Item2}{amSpacer}{amPMValue}", dateTimeFormat, DateTimeStyles.None, out var result))
                        {
                            return new ValueExpression(result.TimeOfDay);
                        }
                    }
                    if (useSecondTime || !onlyCustomTimeTranslation)
                    {
                        if (TimeSpan.TryParse($"{time.Item1}{builtInTimeSep}{time.Item2}{amSpacer}{amPMValue}", out var result))
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
                    string customDateSepForDT = dateTimeFormat.DateSeparator;
                    string customTimeSepForDT = dateTimeFormat.TimeSeparator;
                    if (useSecondDate || onlyCustomDateTranslation)
                    {
                        if (useSecondTime || onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}{customDateSepForDT}{dateTime.Item2}{customDateSepForDT}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSepForDT}{dateTime.Item4.Item2}{customTimeSepForDT}{dateTime.Item4.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }
                        if (useSecondTime || !onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}{customDateSepForDT}{dateTime.Item2}{customDateSepForDT}{dateTime.Item3} {dateTime.Item4.Item1}{builtInTimeSep}{dateTime.Item4.Item2}{builtInTimeSep}{dateTime.Item4.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }
                    }
                    if (useSecondDate || !onlyCustomDateTranslation)
                    {
                        if (useSecondTime || onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}{builtInDateSep}{dateTime.Item2}{builtInDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSepForDT}{dateTime.Item4.Item2}{customTimeSepForDT}{dateTime.Item4.Item3}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }

                        if (useSecondTime || !onlyCustomTimeTranslation)
                        {
                            // Use the existing approach
                            if (DateTime.TryParseExact($"{dateTime.Item1}{builtInDateSep}{dateTime.Item2}{builtInDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{builtInTimeSep}{dateTime.Item4.Item2}{builtInTimeSep}{dateTime.Item4.Item3}", ncalcDateTimeMasks, _currentCultureFormatProvider, DateTimeStyles.None, out var result))
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
                    string customDateSepForDT = dateTimeFormat.DateSeparator;
                    string customTimeSepForDT = dateTimeFormat.TimeSeparator;
                    if (useSecondDate || onlyCustomDateTranslation)
                    {
                        if (useSecondTime || onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}{customDateSepForDT}{dateTime.Item2}{customDateSepForDT}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSepForDT}{dateTime.Item4.Item2}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }
                        if (useSecondTime || !onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}{customDateSepForDT}{dateTime.Item2}{customDateSepForDT}{dateTime.Item3} {dateTime.Item4.Item1}{builtInTimeSep}{dateTime.Item4.Item2}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }
                    }
                    if (useSecondDate || !onlyCustomDateTranslation)
                    {
                        if (useSecondTime || onlyCustomTimeTranslation)
                        {
                            if (DateTime.TryParse($"{dateTime.Item1}{builtInDateSep}{dateTime.Item2}{builtInDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSepForDT}{dateTime.Item4.Item2}", dateTimeFormat, DateTimeStyles.None, out var result))
                            {
                                return new ValueExpression(result);
                            }
                        }

                        if (useSecondTime || !onlyCustomTimeTranslation)
                        {
                            // Use the existing approach
                            if (DateTime.TryParseExact($"{dateTime.Item1}{builtInDateSep}{dateTime.Item2}{builtInDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{builtInTimeSep}{dateTime.Item4.Item2}", ncalcDateShortTimeMasks, _currentCultureFormatProvider, DateTimeStyles.None, out var result))
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
                // if there is a space expected before A/P or am/pm, we need to add it to the expression
                string amSpacer = "";
                if (dateTimeFormat.ShortTimePattern.Contains(" t"))
                    amSpacer = " ";

                dateAndTime12 = dateDefinition.AndSkip(Literals.WhiteSpace()).And(time12Definition!).Then<LogicalExpression>(
                    dateTime =>
                    {
                        string customDateSepForDT = dateTimeFormat.DateSeparator;
                        string customTimeSepForDT = dateTimeFormat.TimeSeparator;
                        string amPMValue = dateTime.Item4.Item4;
                        if (amPMValue.ToLower().Equals(amTimeFirstCharLower))
                            amPMValue = dateTimeFormat.AMDesignator;
                        else
                        if (amPMValue.ToLower().Equals(pmTimeFirstCharLower))
                            amPMValue = dateTimeFormat.PMDesignator;

                        if (useSecondDate || onlyCustomDateTranslation)
                        {
                            if (useSecondTime || onlyCustomTimeTranslation)
                            {
                                if (DateTime.TryParse($"{dateTime.Item1}{customDateSepForDT}{dateTime.Item2}{customDateSepForDT}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSepForDT}{dateTime.Item4.Item2}{customTimeSepForDT}{dateTime.Item4.Item3}{amSpacer}{amPMValue}", dateTimeFormat, DateTimeStyles.None, out var result))
                                {
                                    return new ValueExpression(result);
                                }
                            }
                            if (useSecondTime || !onlyCustomTimeTranslation)
                            {
                                if (DateTime.TryParse($"{dateTime.Item1}{customDateSepForDT}{dateTime.Item2}{customDateSepForDT}{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}:{dateTime.Item4.Item3}{amSpacer}{amPMValue}", dateTimeFormat, DateTimeStyles.None, out var result))
                                {
                                    return new ValueExpression(result);
                                }
                            }
                        }
                        if (useSecondDate || !onlyCustomDateTranslation)
                        {
                            if (useSecondTime || onlyCustomTimeTranslation)
                            {
                                if (DateTime.TryParse($"{dateTime.Item1}{builtInDateSep}{dateTime.Item2}{builtInDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSepForDT}{dateTime.Item4.Item2}{customTimeSepForDT}{dateTime.Item4.Item3}{amSpacer}{amPMValue}", dateTimeFormat, DateTimeStyles.None, out var result))
                                {
                                    return new ValueExpression(result);
                                }
                            }

                            if (useSecondTime || !onlyCustomTimeTranslation)
                            {
                                // Use the existing approach
                                if (DateTime.TryParseExact($"{dateTime.Item1}{builtInDateSep}{dateTime.Item2}{builtInDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{builtInTimeSep}{dateTime.Item4.Item2}{builtInTimeSep}{dateTime.Item4.Item3} {amPMValue}", ncalcDateTime12Masks, _currentCultureFormatProvider, DateTimeStyles.None, out var result))
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
                        string customDateSepForDT = dateTimeFormat.DateSeparator;
                        string customTimeSepForDT = dateTimeFormat.TimeSeparator;
                        string amPMValue = dateTime.Item4.Item3;
                        if (amPMValue.ToLower().Equals(amTimeFirstCharLower))
                            amPMValue = dateTimeFormat.AMDesignator;
                        else
                        if (amPMValue.ToLower().Equals(pmTimeFirstCharLower))
                            amPMValue = dateTimeFormat.PMDesignator;

                        if (useSecondDate || onlyCustomDateTranslation)
                        {
                            if (useSecondTime || onlyCustomTimeTranslation)
                            {
                                if (DateTime.TryParse($"{dateTime.Item1}{customDateSepForDT}{dateTime.Item2}{customDateSepForDT}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSepForDT}{dateTime.Item4.Item2}{amSpacer}{amPMValue}", dateTimeFormat, DateTimeStyles.None, out var result))
                                {
                                    return new ValueExpression(result);
                                }
                            }
                            if (useSecondTime || !onlyCustomTimeTranslation)
                            {
                                if (DateTime.TryParse($"{dateTime.Item1}{customDateSepForDT}{dateTime.Item2}{customDateSepForDT}{dateTime.Item3} {dateTime.Item4.Item1}{builtInTimeSep}{dateTime.Item4.Item2}{amSpacer}{amPMValue}", dateTimeFormat, DateTimeStyles.None, out var result))
                                {
                                    return new ValueExpression(result);
                                }
                            }
                        }
                        if (useSecondDate || !onlyCustomDateTranslation)
                        {
                            if (useSecondTime || onlyCustomTimeTranslation)
                            {
                                if (DateTime.TryParse($"{dateTime.Item1}{builtInDateSep}{dateTime.Item2}{builtInDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{customTimeSepForDT}{dateTime.Item4.Item2}{amSpacer}{amPMValue}", dateTimeFormat, DateTimeStyles.None, out var result))
                                {
                                    return new ValueExpression(result);
                                }
                            }

                            if (useSecondTime || !onlyCustomTimeTranslation)
                            {
                                // Use the existing approach
                                if (DateTime.TryParseExact($"{dateTime.Item1}{builtInDateSep}{dateTime.Item2}{builtInDateSep}{dateTime.Item3} {dateTime.Item4.Item1}{builtInTimeSep}{dateTime.Item4.Item2} {amPMValue}", ncalcDateShortTime12Masks, _currentCultureFormatProvider, DateTimeStyles.None, out var result))
                                {
                                    return new ValueExpression(result);
                                }
                            }
                        }

                        throw new FormatException("Invalid DateTime format.");
                    });
            }

            Parser<LogicalExpression>? humaneTimeSpan = null;

            if (extOptions != null && extOptions.Flags.HasFlag(AdvExpressionOptions.ParseHumanePeriods))
            {
                Parser<string>? alphaText = Terms.Pattern(c => char.IsLetter(c)).Then<string>(x => x.ToString() ?? string.Empty);

                var intNumberForPeriod = Terms.Number<int>(NumberOptions.Integer | useNumberGroupSeparatorFlag | useUnderscoreFlag, decimalSeparator, numGroupSeparator)
                    .AndSkip(Not(OneOf(Terms.Text(decimalSeparator.ToString()), Terms.Text("E", true))))
                    .Then<int>(d => d);

                humaneTimeSpan = ZeroOrOne(alphaText).And(OneOrMany(intNumberForPeriod.And(alphaText.AndSkip(ZeroOrOne(Terms.Char('.')))))).And(ZeroOrOne(alphaText)).Then<LogicalExpression>(val =>
                //humaneTimeSpan = OneOrMany(intNumberForPeriod.And(alphaText.AndSkip(ZeroOrOne(Terms.Char('.'))))).Then<LogicalExpression>(val =>
                {
                    string indicator;
                    int elemValue;
                    int yearValue = 0;
                    int monthValue = 0;
                    int weekValue = 0;
                    int dayValue = 0;
                    int hourValue = 0;
                    int minuteValue = 0;
                    int secondValue = 0;
                    int msecValue = 0;

                    string? prefix = val.Item1;
                    string? suffix = val.Item3;

                    for (int i = 0; i < val.Item2.Count; i++)
                    {
                        var entry = val.Item2[i];
                        elemValue = entry.Item1;
                        indicator = entry.Item2;

                        if (string.IsNullOrEmpty(indicator))
                            throw new Exception(string.Format(errFailedToParsePeriodIndicator, entry.ToString()));

                        indicator = indicator.ToLowerInvariant();
                        if (extOptions.PeriodYearIndicators.Contains(indicator))
                        {
                            if (yearValue != 0)
                                throw new FormatException(string.Format(errDuplicatePeriodIndicator, entry.Item2.ToString()));
                            yearValue = elemValue;
                        }
                        else
                        if (extOptions.PeriodMonthIndicators.Contains(indicator))
                        {
                            if (monthValue != 0)
                                throw new FormatException(string.Format(errDuplicatePeriodIndicator, entry.Item2.ToString()));
                            monthValue = elemValue;
                        }
                        else
                        if (extOptions.PeriodWeekIndicators.Contains(indicator))
                        {
                            if (weekValue != 0)
                                throw new FormatException(string.Format(errDuplicatePeriodIndicator, entry.Item2.ToString()));
                            weekValue = elemValue;
                        }
                        else
                        if (extOptions.PeriodDayIndicators.Contains(indicator))
                        {
                            if (dayValue != 0)
                                throw new FormatException(string.Format(errDuplicatePeriodIndicator, entry.Item2.ToString()));
                            dayValue = elemValue;
                        }
                        else
                        if (extOptions.PeriodHourIndicators.Contains(indicator))
                        {
                            if (hourValue != 0)
                                throw new FormatException(string.Format(errDuplicatePeriodIndicator, entry.Item2.ToString()));
                            hourValue = elemValue;
                        }
                        else
                        if (extOptions.PeriodMinuteIndicators.Contains(indicator))
                        {
                            if (minuteValue != 0)
                                throw new FormatException(string.Format(errDuplicatePeriodIndicator, entry.Item2.ToString()));
                            minuteValue = elemValue;
                        }
                        else
                        if (extOptions.PeriodSecondIndicators.Contains(indicator))
                        {
                            if (secondValue != 0)
                                throw new FormatException(string.Format(errDuplicatePeriodIndicator, entry.Item2.ToString()));
                            secondValue = elemValue;
                        }
                        else
                        if (extOptions.PeriodMSecIndicators.Contains(indicator))
                        {
                            if (msecValue != 0)
                                throw new FormatException(string.Format(errDuplicatePeriodIndicator, entry.Item2.ToString()));
                            msecValue = elemValue;
                        }
                        else
                            throw new FormatException(string.Format(errUnrecognizedPeriodIndicator, entry.Item2.ToString()));
                    }

                    if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
                    {
                        DateTime current = DateTime.UtcNow;
                        DateTime dt = current;
                        if (yearValue != 0)
                            dt = dt.AddYears(yearValue);
                        if (monthValue != 0)
                            dt = dt.AddMonths(monthValue);
                        if (weekValue != 0)
                            dt = dt.AddDays(weekValue * 7);
                        if (dayValue != 0)
                            dt = dt.AddDays(dayValue);
                        if (hourValue != 0)
                            dt = dt.AddHours(hourValue);
                        if (minuteValue != 0)
                            dt = dt.AddMinutes(minuteValue);
                        if (secondValue != 0)
                            dt = dt.AddSeconds(secondValue);
                        if (msecValue != 0)
                            dt = dt.AddMilliseconds(msecValue);
                        return new ValueExpression(dt - current);
                    }
                    else
                    {
                        if (!(string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(suffix)))
                        {
                            throw new FormatException(string.Format(errDuplicateTimeRelationIndicators, prefix, suffix));
                        }

                        bool pastTime = false;

                        prefix = prefix?.ToLowerInvariant();
                        suffix = suffix?.ToLowerInvariant();

                        if ((prefix != null && extOptions.PeriodPastIndicators.Contains(prefix)) || (suffix != null && extOptions.PeriodPastIndicators.Contains(suffix)))
                            pastTime = true;
                        else
                        if ((prefix != null && extOptions.PeriodFutureIndicators.Contains(prefix)) || (suffix != null && extOptions.PeriodFutureIndicators.Contains(suffix)))
                            pastTime = false;
                        else
                            throw new FormatException(string.Format(errUnrecognizedTimeRelationIndicator, prefix));

                        DateTime dt = DateTime.Now; // people are interested in local time before or after current moment
                        if (pastTime)
                        {
                            yearValue = -yearValue;
                            monthValue = -monthValue;
                            weekValue = -weekValue;
                            dayValue = -dayValue;
                            hourValue = -hourValue;
                            minuteValue = -minuteValue;
                            secondValue = -secondValue;
                            msecValue = -msecValue;
                        }
                        if (yearValue != 0)
                            dt = dt.AddYears(yearValue);
                        if (monthValue != 0)
                            dt = dt.AddMonths(monthValue);
                        if (weekValue != 0)
                            dt = dt.AddDays(weekValue * 7);
                        if (dayValue != 0)
                            dt = dt.AddDays(dayValue);
                        if (hourValue != 0)
                            dt = dt.AddHours(hourValue);
                        if (minuteValue != 0)
                            dt = dt.AddMinutes(minuteValue);
                        if (secondValue != 0)
                            dt = dt.AddSeconds(secondValue);
                        if (msecValue != 0)
                            dt = dt.AddMilliseconds(msecValue);
                        return new ValueExpression(dt);
                    }
                });
            }
            List<Parser<LogicalExpression>> timeParts = use12HourTime
                ? [dateAndTime12!, dateAndShortTime12!, dateAndTime, dateAndShortTime, date, time12!, shortTime12!, time, shortTime]
                : [dateAndTime, dateAndShortTime, date, time, shortTime];

            if (humaneTimeSpan != null)
                timeParts.Add(humaneTimeSpan);

            // datetime => '#' dateAndTime | date | shortTime | time  '#';
            dateTime = Terms
                .Char('#')
                .SkipAnd(OneOf(timeParts.ToArray()))
                .AndSkip(Literals.Char('#'));
        }

        var isHexDigit = Character.IsHexDigit;

        Parser<LogicalExpression>? guid = null;

        if (!options.HasFlag(ExpressionOptions.DontParseGuids))
        {
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

            guid = OneOf(guidWithHyphens, guidWithoutHyphens);
        }

        // primary => GUID | Percent | NUMBER | identifier| DateTime | string | resultReference | function | boolean | groupExpression | identifier | list ;

        List<Parser<LogicalExpression>> enabledParsers = new List<Parser<LogicalExpression>>();

        if (guid != null)
            enabledParsers.Add(guid);
        enabledParsers.Add(hexOctBinNumber);
        if (currency != null)
            enabledParsers.Add(currency);
        enabledParsers.Add(intNumber);
        enabledParsers.Add(longNumber);
        enabledParsers.Add(decimalOrDoubleNumber);
        enabledParsers.Add(booleanTrue);
        enabledParsers.Add(booleanFalse);
        if (dateTime != null) // dateTime will be initialized unless options.HasFlag(ExpressionOptions.DontParseDates)
            enabledParsers.Add(dateTime);
        enabledParsers.Add(stringValue);
        enabledParsers.Add(functionOrResultRef);
        enabledParsers.Add(groupExpression);
        enabledParsers.Add(identifierExpression);
        enabledParsers.Add(list);

        var primary = OneOf(enabledParsers.ToArray());

        // factorial => primary ("!")* ;
        // A factorial includes any primary
        var factorial = primary.And(ZeroOrMany(exclamationMark.AndSkip(Not(equal))))
            .Then(static x =>
            {
                if (x.Item2.Count == 0)
                {
                    // there is just a primary discovered
                    return x.Item1;
                }
                return new BinaryExpression(BinaryExpressionType.Factorial, x.Item1, new ValueExpression(x.Item2.Count));
            }
        );

        Parser<LogicalExpression> factorialOrPercent;

        if (extOptions != null && extOptions.Flags.HasFlag(AdvExpressionOptions.CalculatePercent))
        {
            Parser<LogicalExpression>? numberPercent = factorial.And(ZeroOrOne(percentChar, '\0'))
                .Then<LogicalExpression>(static x =>
                {
                    if (x.Item2 == '\0')
                    {
                        // there is just a primary discovered
                        return x.Item1;
                    }
                    return new PercentExpression(x.Item1);
                });
            factorialOrPercent = numberPercent;
        }
        else
            factorialOrPercent = factorial;

        // Either a factorial, primary, or exponential
        // exponential => factorial ( "**" factorial )* ;
        var exponential = factorialOrPercent.And(ZeroOrMany(exponent.And(factorial)))
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

        // ( "-" | "!" | "not" | "~" | root2 | root3 | root4 ) factorial | exponential | primary;
        List<(Parser<string>, Func<LogicalExpression, LogicalExpression>)> unaryOps =
        [
            (not, static value => new UnaryExpression(UnaryExpressionType.Not, value)),
            (minus, static value => new UnaryExpression(UnaryExpressionType.Negate, value)),
            (bitwiseNot, static value => new UnaryExpression(UnaryExpressionType.BitwiseNot, value)),
        ];
        if (root2 != null)
            unaryOps.Add((root2, static value => new UnaryExpression(UnaryExpressionType.SqRoot, value)));
#if NET8_0_OR_GREATER
        if (root3 != null)
            unaryOps.Add((root3, static value => new UnaryExpression(UnaryExpressionType.CbRoot, value)));
#endif
        if (root4 != null)
            unaryOps.Add((root4, static value => new UnaryExpression(UnaryExpressionType.FourthRoot, value)));
        var unary = exponential.Unary(unaryOps.ToArray());

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
                    lessOrEqual.Then(BinaryExpressionType.LessOrEqual),
                    less.Then(BinaryExpressionType.Less),
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

        var xorTypeParser = xor.Then(BinaryExpressionType.XOr)
            .Or(bitwiseXOr.Then(BinaryExpressionType.BitwiseXOr));

        // "and" has higher precedence than "or"
        var andParser = equality.And(ZeroOrMany(andTypeParser.And(equality)))
            .Then(ParseBinaryExpression);

        var orParser = andParser.And(ZeroOrMany(orTypeParser.And(andParser)))
            .Then(ParseBinaryExpression);

        var xorParser = andParser.And(ZeroOrMany(xorTypeParser.And(andParser)))
            .Then(ParseBinaryExpression);

        // logical => equality ( ( "and" | "or" | "xor" ) equality )* ;
        var logical = OneOf(orParser, xorParser).And(ZeroOrMany(xorTypeParser.And(orParser)))
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
                    lessOrEqual, greater, less, equal,
                    notEqual)),
                static (_, _) => throw new InvalidOperationException("Unknown operator sequence.")));

        List<Parser<LogicalExpression>> statements = [operatorSequence];

        Parser<LogicalExpression>? topLevel = null;

        if (options.HasFlag(ExpressionOptions.UseAssignments))
        {
            var assignmentTypeParser = assignmentOperator.Then(BinaryExpressionType.Assignment);

            var assignment = identifierExpression.And(OneOf(assignmentOperator, plusAssign, minusAssign, multiplyAssign, divAssign, orAssign, xorAssign, andAssign)).And(OneOrMany(operatorSequence))
            .Then<LogicalExpression>(x =>
                {
                    LogicalExpression result = null!;
                    BinaryExpressionType expressionType;
                    switch (x.Item2)
                    {
                        case "+=":
                            expressionType = BinaryExpressionType.PlusAssignment;
                            break;
                        case "-=":
                            expressionType = BinaryExpressionType.MinusAssignment;
                            break;
                        case "\u00D7=":
                        case "\u2219=":
                        case "*=":
                            expressionType = BinaryExpressionType.MultiplyAssignment;
                            break;
                        case "/=":
                            expressionType = BinaryExpressionType.DivAssignment;
                            break;
                        case "&=":
                            expressionType = BinaryExpressionType.AndAssignment;
                            break;
                        case "|=":
                            expressionType = BinaryExpressionType.OrAssignment;
                            break;
                        case "^=":
                            expressionType = BinaryExpressionType.XOrAssignment;
                            break;
                        default:
                            expressionType = BinaryExpressionType.Assignment;
                            break;
                    }

                    result = (BinaryExpression)(new BinaryExpression(expressionType, x.Item1, x.Item3[0])).SetOptions(options, cultureInfo, extOptions);
                    if (x.Item3.Count > 1)
                    {
                        for (int i = 1; i < x.Item3.Count - 1; i++)
                        {
                            result = (BinaryExpression)(new BinaryExpression(expressionType, result, x.Item3[i])).SetOptions(options, cultureInfo, extOptions);
                        }
                    }
                    return result;
                }
            );

            statements.Insert(0, assignment);
        }

        var statementsArray = statements.ToArray();

        topLevel = OneOf(statementsArray);
        var expressionOrAssignment = OneOf(statementsArray);

        if (options.HasFlag(ExpressionOptions.UseStatementSequences))
        {
            var statementSequence = expressionOrAssignment.And(ZeroOrMany(Terms.Pattern((c) => c == ';').SkipAnd(expressionOrAssignment)))
                .Then(x =>
                {
                    LogicalExpression result = null!;

                    switch (x.Item2.Count)
                    {
                        case 0:
                            result = x.Item1;
                            break;
                        case 1:
                            result = new BinaryExpression(BinaryExpressionType.StatementSequence, x.Item1, x.Item2[0]);
                            break;
                        default:
                        {
                            result = new BinaryExpression(BinaryExpressionType.StatementSequence, x.Item1, x.Item2[0]);
                            for (int i = 1; i < x.Item2.Count - 1; i++)
                            {
                                result = new BinaryExpression(BinaryExpressionType.StatementSequence, result,
                                    x.Item2[i]);
                            }
                            break;
                        }
                    }
                    return result;
                });
            topLevel = statementSequence;
        }

        expression.Parser = OneOf(statementsArray);

        var expressionParser = topLevel.AndSkip(ZeroOrMany(Literals.WhiteSpace(true))).Eof()
                .ElseError(InvalidTokenMessage);

        AppContext.TryGetSwitch("NCalc.EnableParlotParserCompilation", out var enableParserCompilation);

        return enableParserCompilation ? expressionParser.Compile() : expressionParser;
    }

    private static Parser<LogicalExpression> GetOrCreateExpressionParser(CultureInfo cultureInfo, LogicalExpressionParserContext context)
    {
        if (context.Options == ExpressionOptions.None && Parsers.TryGetValue(cultureInfo, out var parser))
        {
            return parser;
        }

        var newParser = CreateExpressionParser(cultureInfo, context.Options, context.AdvancedOptions);
        if (context.Options == ExpressionOptions.None)
        {
            Parsers.TryAdd(cultureInfo, newParser);
        }

        return newParser;
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
        if (context.AdvancedOptions is not null)
            parserToUse = CreateExpressionParser(context.CultureInfo, context.Options, context.AdvancedOptions);
        else
            parserToUse = GetOrCreateExpressionParser(context.CultureInfo, context);

        if (parserToUse.TryParse(context, out var result, out var error))
            return result;

        string message;
        TextPosition position;
        if (error != null)
        {
            position = error.Position;
            message = $"{error.Message} at position {position}";
        }
        else
        {
            position = context.Scanner.Cursor.Position;
            message = $"Error parsing the expression at position {position}";
        }

        throw new NCalcParserException(message, position);
    }
}