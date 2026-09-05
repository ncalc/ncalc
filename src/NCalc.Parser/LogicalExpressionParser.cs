using System.Runtime.CompilerServices;
using NCalc.Exceptions;
using Parlot;
using Parlot.Fluent;
using Parlot.SourceGenerator;

namespace NCalc;

/// <summary>
/// Class responsible for parsing strings into <see cref="LogicalExpression"/> objects.
/// </summary>
public static partial class LogicalExpressionParser
{
    // Preserve custom culture settings without retaining short-lived culture instances.
    private static readonly ConditionalWeakTable<CultureInfo, ConcurrentDictionary<LogicalExpressionParserCacheKey, Parser<LogicalExpression>>> ParserCache = new();

    private static readonly LogicalExpression True = new ValueExpression(true);
    private static readonly LogicalExpression False = new ValueExpression(false);

    private const double MinDecDouble = (double)decimal.MinValue;
    private const double MaxDecDouble = (double)decimal.MaxValue;

    private const string InvalidTokenMessage = "Invalid token in expression";

    public static Parser<LogicalExpression> GetOrCreateExpressionParser(
        LogicalExpressionParserOptions options,
        CultureInfo culture)
    {
        var key = new LogicalExpressionParserCacheKey(options);
        var cache = ParserCache.GetValue(culture, static _ => new());

        return cache.GetOrAdd(key, _ => CreateExpressionParser(options, culture));
    }

    /// <summary>
    /// Creates a source-generated parser with the specified grammar options and culture.
    /// </summary>
    private static Parser<LogicalExpression> CreateExpressionParser(LogicalExpressionParserOptions options, CultureInfo cultureInfo)
    {
        // Keep the intercepted call in this assembly, including for external callers.
        return CreateParserGrammar(options, cultureInfo).Named("Expression");
    }

    private static Parser<string> CreateStringParser(char quote)
    {
        var openingDelimiter = Terms.Char(quote);
        var closingDelimiter = Literals.Char(quote);
        var escapedUnicode = Literals.Text("\\u")
            .SkipAnd(Literals.Pattern(Character.IsHexDigit, 4, 4))
            .Then(static value => char.ConvertFromUtf32(Convert.ToInt32(value.ToString(), 16)));
        var escapedCharacter = Literals.Char('\\')
            .And(Literals.Pattern(_ => true, 1, 1))
            .Then(static value =>
            {
                var escaped = value.Item2.ToString();
                return escaped switch
                {
                    "\\" => "\\",
                    "'" => "'",
                    "\"" => "\"",
                    "n" => "\n",
                    "r" => "\r",
                    "t" => "\t",
                    "b" => "\b",
                    "f" => "\f",
                    _ => "\\" + escaped
                };
            });
        var text = (quote == '\''
                ? Literals.Pattern(static c => c != '\'' && c != '\\' && c != '\r' && c != '\n')
                : Literals.Pattern(static c => c != '"' && c != '\\' && c != '\r' && c != '\n'))
            .Then(static value => value.ToString());

        return Between(openingDelimiter, ZeroOrMany(OneOf(escapedUnicode, escapedCharacter, text)), closingDelimiter)
            .Then(static values => string.Concat(values));
    }

    [GenerateParser]
    [IncludeUsings("NCalc", "NCalc.Exceptions")]
    [IncludeGenerators("PolySharp")]
    private static Parser<LogicalExpression> CreateParserGrammar(LogicalExpressionParserOptions options, CultureInfo cultureInfo)
    {
        /*
         * Grammar:
         * expression     => ternary ( ( "-" | "+" ) ternary )* ;
         * ternary        => coalescing ( "?" coalescing ":" coalescing)?
         * coalescing     => logical ( "??" coalescing)? ;
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
        expression.Named("ExpressionBody");

        var hexNumber = Terms.Text("0x")
            .SkipAnd(Terms.AnyOf(Character.HexDigits))
            .Then(static x => Convert.ToInt64(x.ToString(), 16))
            .Named("HexadecimalNumber");

        var octalNumber = Terms.Text("0o")
            .SkipAnd(Terms.AnyOf(Character.OctalDigits))
            .Then(static x => Convert.ToInt64(x.ToString(), 8))
            .Named("OctalNumber");

        var binaryNumber = Terms.Text("0b")
            .SkipAnd(Terms.AnyOf(Character.BinaryDigits))
            .Then(static x => Convert.ToInt64(x.ToString(), 2))
            .Named("BinaryNumber");

        var hexOctBinNumber = OneOf(hexNumber, octalNumber, binaryNumber)
            .Then(ParseBasedInteger);

        var nonScientificParser = Not(OneOf(Terms.Text("."), Terms.Text("E", true)));

        var intNumber = Terms.Number<int>(NumberOptions.Integer)
            .AndSkip(nonScientificParser)
            .Then<LogicalExpression>(CreateValueExpression)
            .Named("Int32");

        var longNumber = Terms.Number<long>(NumberOptions.Integer)
            .AndSkip(nonScientificParser)
            .Then<LogicalExpression>(CreateValueExpression)
            .Named("Int64");

        var decimalNumber = Terms.Number<decimal>(NumberOptions.Float)
            .Then<LogicalExpression>(CreateValueExpression).Named("Decimal");

        var doubleNumber = Terms.Number<double>(NumberOptions.Float)
            .Then<LogicalExpression>(CreateValueExpression).Named("Double");

        var decimalFallbackNumber = Terms.Number<double>(NumberOptions.Float)
            .Then(ParseDecimalFallback);

        var decimalOrDouble = OneOf(decimalNumber, decimalFallbackNumber);
        var decimalOrDoubleNumber = If<LogicalExpressionParseContext, LogicalExpression>(
            static context => context.Options.FloatingPointNumberType == FloatingPointNumberType.Decimal,
            decimalOrDouble, doubleNumber).Named("FloatingPointNumber");

        var argumentSeparatorTerm = Terms.AnyOf(",;:", 1, 1)
            .When((_, separator) => separator.Span[0] switch
            {
                ',' => options.ArgumentSeparator == ArgumentSeparator.Default || options.ArgumentSeparator.HasFlag(ArgumentSeparator.Comma),
                ';' => options.ArgumentSeparator.HasFlag(ArgumentSeparator.Semicolon),
                ':' => options.ArgumentSeparator.HasFlag(ArgumentSeparator.Colon),
                _ => false
            }).Named("ArgumentSeparator");

        var divided = Terms.Text("/");
        var times = Terms.Text("*");
        var modulo = Terms.Text("%");
        var minus = Terms.Text("-");
        var plus = Terms.Text("+");

        var equals = Terms.Text("==");
        var singleEquals = Terms.Text("=");

        var equal = If(() => options.DisallowSingleEquals, equals, OneOf(equals, singleEquals))
            .Named("EqualOperator");

        var equalityGuard = OneOf(equals, singleEquals);
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
        var coalesce = Terms.Text("??");
        var colon = Terms.Char(':');
        var exclamation = Terms.Char('!');

        var identifier = Terms.Identifier();

        var not = OneOf(
            Terms.Text("NOT", true).AndSkip(OneOf(Literals.WhiteSpace(), Not(AnyCharBefore(openParen)))),
            Terms.Text("!"));
        var and = OneOf(Terms.Text("AND", true), Terms.Text("&&"));
        var or = OneOf(Terms.Text("OR", true), Terms.Text("||"));

        var bitwiseAnd = Terms.Text("&");
        var bitwiseOr = Terms.Text("|");
        var bitwiseXOr = Terms.Text("^");
        var bitwiseNot = Terms.Text("~");

        // "(" expression ")"
        var groupExpression = Between(openParen, expression, closeParen).Named("Group");

        var braceIdentifier = openBrace
            .SkipAnd(AnyCharBefore(closeBrace, failOnEof: true, consumeDelimiter: true).ElseError("Brace not closed."));

        var curlyBraceIdentifier =
            openCurlyBrace.SkipAnd(AnyCharBefore(closeCurlyBrace, failOnEof: true, consumeDelimiter: true)
                .ElseError("Brace not closed."));

        // ("[" | "{") identifier ("]" | "}")
        var identifierExpression = OneOf(
                braceIdentifier,
                curlyBraceIdentifier,
                identifier)
            .Then<LogicalExpression>(static x => new Identifier(x.ToString())).Named("Identifier");

        // list => "(" (expression (argumentSeparator expression)*)? ")"
        var populatedList =
            Between(openParen, Separated(argumentSeparatorTerm, expression),
                    closeParen.ElseError("Parenthesis not closed."))
                .Then<LogicalExpression>(static values => new LogicalExpressionList(values));

        var emptyList = openParen.AndSkip(closeParen).Then<LogicalExpression>(static _ => new LogicalExpressionList());

        var list = OneOf(emptyList, populatedList).Named("List");

        var function = identifier
            .And(list)
            .Then<LogicalExpression>(static x =>
                new Function(new Identifier(x.Item1.ToString()), (LogicalExpressionList)x.Item2)).Named("Function");

        var booleanTrue = Terms.Text("true", true)
            .Then<LogicalExpression>(static _ => True).Named("True");
        var booleanFalse = Terms.Text("false", true)
            .Then<LogicalExpression>(static _ => False).Named("False");

        var singleQuotesStringValue = CreateStringParser('\'')
            .Then<LogicalExpression>(ParseSingleQuotedString)
            .Named("SingleQuotedString");

        var doubleQuotesStringValue = CreateStringParser('"')
            .Then<LogicalExpression>(CreateValueExpression).Named("DoubleQuotedString");

        var stringValue = OneOf(singleQuotesStringValue, doubleQuotesStringValue).Named("String");

        var charIsNumber = Literals.Pattern(static c => char.IsNumber(c));

        // Match arbitrary culture separators at parse time without specializing the generated graph.
        var dateSeparator = Always(true)
            .When((context, _) => context.Scanner.ReadText(cultureInfo.DateTimeFormat.DateSeparator.AsSpan()))
            .Named("DateSeparator");
        var timeSeparator = Always(true)
            .When((context, _) => context.Scanner.ReadText(cultureInfo.DateTimeFormat.TimeSeparator.AsSpan()))
            .Named("TimeSeparator");
        var decimalSeparator = Always(true)
            .When((context, _) => context.Scanner.ReadText(cultureInfo.NumberFormat.NumberDecimalSeparator.AsSpan()))
            .Named("DecimalSeparator");

        var dateDefinition = charIsNumber
            .AndSkip(dateSeparator)
            .And(charIsNumber)
            .AndSkip(dateSeparator)
            .And(charIsNumber);

        // date => number/number/number
        var date = dateDefinition.Then(value => ParseDate(value, cultureInfo)).Named("Date");

        // time => number:number:number{.fractional}
        var timeDefinition = charIsNumber
            .AndSkip(timeSeparator)
            .And(charIsNumber)
            .AndSkip(timeSeparator)
            .And(charIsNumber)
            .AndSkip(ZeroOrOne(decimalSeparator))
            .And(ZeroOrOne(charIsNumber));

        var time = timeDefinition.Then(value => ParseTime(value, cultureInfo)).Named("Time");

        // dateAndTime => number/number/number number:number:number{.fractional}
        var dateAndTime = dateDefinition.AndSkip(Literals.WhiteSpace()).And(timeDefinition)
            .Then(value => ParseDateAndTime(value, cultureInfo)).Named("DateAndTime");

        // datetime => '#' dateAndTime | date | time  '#';
        var dateTime = Terms
            .Char('#')
            .SkipAnd(OneOf(dateAndTime, date, time))
            .AndSkip(Literals.Char('#')).Named("DateTimeLiteral");

        var eightHexSequence = Terms.AnyOf(Character.HexDigits, 8, 8);

        var fourHexSequence = Terms.AnyOf(Character.HexDigits, 4, 4);

        var twelveHexSequence = Terms.AnyOf(Character.HexDigits, 12, 12);

        var thirtyTwoHexSequence = Terms.AnyOf(Character.HexDigits, 32, 32);

        var guidWithHyphens = Capture(eightHexSequence
                .And(minus)
                .And(fourHexSequence)
                .And(minus)
                .And(fourHexSequence)
                .And(minus)
                .And(fourHexSequence)
                .And(minus)
                .And(twelveHexSequence))
            .Then(ParseGuid);

        var guidWithoutHyphens = thirtyTwoHexSequence
            .AndSkip(Not(decimalOrDoubleNumber))
            .Then(ParseGuid);

        var guid = OneOf(guidWithHyphens, guidWithoutHyphens).Named("Guid");

        var intOrLong = OneOf(intNumber, longNumber);
        var integralNumber = If<LogicalExpressionParseContext, LogicalExpression>(
            static context => context.Options.IntegerNumberType == IntegerNumberType.Int64,
            longNumber, intOrLong).Named("Integer");

        // primary => GUID | NUMBER | identifier | DateTime | string | function | boolean | groupExpression | list ;
        var primary = OneOf(
            guid,
            hexOctBinNumber,
            integralNumber,
            decimalOrDoubleNumber,
            booleanTrue,
            booleanFalse,
            dateTime,
            stringValue,
            function,
            groupExpression,
            identifierExpression,
            list).Named("Primary");

        // factorial => primary ( "!" )* ;
        var factorial = primary.And(ZeroOrMany(exclamation.AndSkip(Not(equalityGuard))))
            .Then(static x =>
            {
                var result = x.Item1;
                var count = x.Item2.Count;

                for (var i = 0; i < count; i++)
                    result = new UnaryExpression(UnaryExpressionType.Factorial, result);

                return result;
            }).Named("Factorial");

        // exponential => unary ( "**" unary )* ;
        var exponential = factorial.And(ZeroOrMany(exponent.And(primary)))
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
            }).Named("Exponentiation");

        // ( "-" | "not" ) unary | primary;
        var unary = exponential.Unary(
            (not, static value => new UnaryExpression(UnaryExpressionType.Not, value)),
            (minus, static value => new UnaryExpression(UnaryExpressionType.Negate, value)),
            (bitwiseNot, static value => new UnaryExpression(UnaryExpressionType.BitwiseNot, value))
        ).Named("Unary");

        // multiplicative => unary ( ( "/" | "*" | "%" ) unary )* ;
        var multiplicative = unary.LeftAssociative(
            (divided, static (a, b) => new BinaryExpression(BinaryExpressionType.Div, a, b)),
            (times, static (a, b) => new BinaryExpression(BinaryExpressionType.Times, a, b)),
            (modulo, static (a, b) => new BinaryExpression(BinaryExpressionType.Modulo, a, b))
        ).Named("Multiplicative");

        // additive => multiplicative ( ( "-" | "+" ) multiplicative )* ;
        var additive = multiplicative.LeftAssociative(
            (plus, static (a, b) => new BinaryExpression(BinaryExpressionType.Plus, a, b)),
            (minus, static (a, b) => new BinaryExpression(BinaryExpressionType.Minus, a, b))
        ).Named("Additive");

        // shift => additive ( ( "<<" | ">>" ) additive )* ;
        var shift = additive.LeftAssociative(
            (leftShift, static (a, b) => new BinaryExpression(BinaryExpressionType.LeftShift, a, b)),
            (rightShift, static (a, b) => new BinaryExpression(BinaryExpressionType.RightShift, a, b))
        ).Named("Shift");

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
                ).Named("RelationalOperator")
                .And(shift)))
            .Then(ParseBinaryExpression).Named("Relational");

        var equality = relational.And(ZeroOrMany(OneOf(
                    equal.Then(BinaryExpressionType.Equal),
                    notEqual.Then(BinaryExpressionType.NotEqual)).Named("EqualityOperator")
                .And(relational)))
            .Then(ParseBinaryExpression).Named("Equality");

        var bitwiseAndExpression = equality.And(
            ZeroOrMany(bitwiseAnd.Then(BinaryExpressionType.BitwiseAnd).And(equality)))
            .Then(ParseBinaryExpression).Named("BitwiseAnd");

        var bitwiseXOrExpression = bitwiseAndExpression.And(
            ZeroOrMany(bitwiseXOr.Then(BinaryExpressionType.BitwiseXOr).And(bitwiseAndExpression)))
            .Then(ParseBinaryExpression).Named("BitwiseXor");

        var bitwiseOrExpression = bitwiseXOrExpression.And(
            ZeroOrMany(bitwiseOr.Then(BinaryExpressionType.BitwiseOr).And(bitwiseXOrExpression)))
            .Then(ParseBinaryExpression).Named("BitwiseOr");

        var andParser = bitwiseOrExpression.And(
            ZeroOrMany(and.Then(BinaryExpressionType.And).And(bitwiseOrExpression)))
            .Then(ParseBinaryExpression).Named("LogicalAnd");

        // logical => equality ( ("&" | "^" | "|" | "and" | "or") equality )* ;
        var logical = andParser.And(
            ZeroOrMany(or.Then(BinaryExpressionType.Or).And(andParser)))
            .Then(ParseBinaryExpression).Named("LogicalOr");

        // coalescing => logical ( "??" coalescing)? ;
        // RightAssociative stores delegates as parser results, which code generation cannot emit.
        var coalescing = logical.And(ZeroOrMany(coalesce.SkipAnd(logical)))
            .Then(ParseCoalescingExpression).Named("Coalescing");

        // ternary => coalescing ("?" coalescing ":" coalescing) ?
        var ternary = coalescing.And(ZeroOrOne(questionMark.SkipAnd(coalescing).AndSkip(colon).And(coalescing)))
            .Then(static x => x.Item2.Item1 == null
                ? x.Item1
                : new TernaryExpression(x.Item1, x.Item2.Item1, x.Item2.Item2))
            .Or(coalescing).Named("Ternary");

        // Parlot's source generator can struggle to infer the operator parser's generic type
        // here, so we force it to a simple, known type and specify LeftAssociative's type args.
        var invalidOperatorSequence = OneOrMany(OneOf(
                divided, times, modulo, plus,
                minus, leftShift, rightShift, greaterOrEqual,
                lesserOrEqual, greater, lesser, equal,
                notEqual))
            .Then(static _ => 0).Named("InvalidOperatorSequence");

        var operatorSequence = Parsers.LeftAssociative<LogicalExpression, int>(
            ternary,
            (op: invalidOperatorSequence, factory: ThrowUnknownOperatorSequence));

        expression.Parser = operatorSequence;
        var expressionParser = expression.AndSkip(ZeroOrMany(Literals.WhiteSpace(true))).Eof()
            .ElseError(InvalidTokenMessage).Named("Expression");

        return expressionParser;
    }

    private static LogicalExpression ParseSingleQuotedString(ParseContext context, string value)
    {
        return ((LogicalExpressionParseContext)context).Options.AllowCharValues && value.Length == 1
            ? new ValueExpression(value[0])
            : new ValueExpression(value);
    }

    // Keep concrete AST construction outside inspected callbacks: the compiler host may not load
    // the target framework's System.Text.Json dependency on ValueExpression.
    private static LogicalExpression CreateValueExpression(int value) => new ValueExpression(value);
    private static LogicalExpression CreateValueExpression(long value) => new ValueExpression(value);
    private static LogicalExpression CreateValueExpression(decimal value) => new ValueExpression(value);
    private static LogicalExpression CreateValueExpression(double value) => new ValueExpression(value);
    private static LogicalExpression CreateValueExpression(string value) => new ValueExpression(value);

    private static LogicalExpression ParseBasedInteger(long value)
    {
        return value is > int.MaxValue or < int.MinValue
            ? new ValueExpression(value)
            : new ValueExpression((int)value);
    }

    private static LogicalExpression ParseDecimalFallback(double value)
    {
        return value switch
        {
            > MaxDecDouble => new ValueExpression(double.PositiveInfinity),
            < MinDecDouble => new ValueExpression(double.NegativeInfinity),
            _ => new ValueExpression((decimal)value)
        };
    }

    private static LogicalExpression ParseGuid(TextSpan value)
    {
#if NET6_0_OR_GREATER
        return new ValueExpression(Guid.Parse(value.Span));
#else
        return new ValueExpression(Guid.Parse(value.ToString()));
#endif
    }

    private static LogicalExpression ParseDate((TextSpan First, TextSpan Second, TextSpan Third) date, CultureInfo culture)
    {
        var separator = culture.DateTimeFormat.DateSeparator;
        if (DateTime.TryParse($"{date.First}{separator}{date.Second}{separator}{date.Third}",
                culture, DateTimeStyles.None, out var result))
        {
            return new ValueExpression(result);
        }

        throw new FormatException("Invalid DateTime format.");
    }

    private static LogicalExpression ParseTime((TextSpan Hour, TextSpan Minute, TextSpan Second, TextSpan Fraction) time, CultureInfo culture)
    {
        var separator = culture.DateTimeFormat.TimeSeparator;
        var value = $"{time.Hour}{separator}{time.Minute}{separator}{time.Second}";
        if (time.Fraction.Length == 0 && TimeSpan.TryParse(value, culture, out var result))
            return new ValueExpression(result);

        if (TimeSpan.TryParse($"{value}{culture.NumberFormat.NumberDecimalSeparator}{time.Fraction}", culture, out result))
            return new ValueExpression(result);

        throw new FormatException("Invalid TimeSpan format.");
    }

    private static LogicalExpression ParseDateAndTime(
        (TextSpan First, TextSpan Second, TextSpan Third, (TextSpan Hour, TextSpan Minute, TextSpan Second, TextSpan Fraction) Time) dateTime,
        CultureInfo culture)
    {
        var dateSeparator = culture.DateTimeFormat.DateSeparator;
        var timeSeparator = culture.DateTimeFormat.TimeSeparator;
        var time = dateTime.Time;
        var value = $"{dateTime.First}{dateSeparator}{dateTime.Second}{dateSeparator}{dateTime.Third} {time.Hour}{timeSeparator}{time.Minute}{timeSeparator}{time.Second}";
        if (time.Fraction.Length == 0 && DateTime.TryParse(value, culture, DateTimeStyles.None, out var result))
            return new ValueExpression(result);

        if (DateTime.TryParse($"{value}{culture.NumberFormat.NumberDecimalSeparator}{time.Fraction}", culture, DateTimeStyles.None, out result))
            return new ValueExpression(result);

        throw new FormatException("Invalid DateTime format.");
    }

    private static LogicalExpression ThrowUnknownOperatorSequence(LogicalExpression _, LogicalExpression __)
    {
        throw new InvalidOperationException("Unknown operator sequence.");
    }

    private static LogicalExpression ParseCoalescingExpression((LogicalExpression Left, IReadOnlyList<LogicalExpression> Right) expression)
    {
        if (expression.Right.Count == 0)
            return expression.Left;

        var result = expression.Right[expression.Right.Count - 1];
        for (var i = expression.Right.Count - 2; i >= 0; i--)
            result = new BinaryExpression(BinaryExpressionType.Coalesce, expression.Right[i], result);

        return new BinaryExpression(BinaryExpressionType.Coalesce, expression.Left, result);
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

    public static LogicalExpression Parse(LogicalExpressionParseContext context, CultureInfo? culture = null)
    {
        var parser = GetOrCreateExpressionParser(context.Options, culture ?? CultureInfo.CurrentCulture);

        if (parser.TryParse(context, out var result, out var error))
            return result;

        string message;
        if (error != null)
            message = $"{error.Message} at position {error.Position}";
        else
            message = $"Error parsing the expression at position {context.Scanner.Cursor.Position}";

        throw new NCalcParserException(message);
    }
}
