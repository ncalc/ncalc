using NCalc.Exceptions;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;
#if !PARLOT_FLUENT
using Parlot.SourceGenerator;
#endif

#if PARLOT_FLUENT
namespace NCalc.Benchmarks;

internal static partial class FluentExpressionParser
#else
namespace NCalc;

public static partial class LogicalExpressionParser
#endif
{
    private const StringEscapes NCalcStringEscapes =
        StringEscapes.Quotes |
        StringEscapes.NewLineAndTab |
        StringEscapes.BackspaceAndFormFeed |
        StringEscapes.KeepUnknown |
        StringEscapes.SingleLine;

#if !PARLOT_FLUENT
    [GenerateParser(nameof(TryParseCore))]
    [IncludeUsings("NCalc", "NCalc.Exceptions")]
    [IncludeGenerators("PolySharp")]
#endif
    private static Parser<LogicalExpression> CreateParserGrammar(LogicalExpressionParserOptions options, CultureInfo cultureInfo)
    {
        /*
         * Grammar:
         * expression     => ternary ( ( "-" | "+" ) ternary )* ;
         * ternary        => coalescing ( "?" coalescing ":" ternary )?
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
            .SkipAnd(Terms.Hexadecimal<ulong>())
            .Then(static value => unchecked((long)value))
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
        var decimalOrDoubleNumber = If(
            () => options.FloatingPointNumberType == FloatingPointNumberType.Decimal,
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
        var @in = Terms.Keyword("in", true);
        var notIn = Terms.Keyword("not in", true);

        var like = Terms.Keyword("like", true);
        var notLike = Terms.Keyword("not like", true);

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

        var whiteSpaceOrOpenParen = OneOf(Literals.WhiteSpace(), Capture(Not(Not(openParen))));

        var not = OneOf(
            Terms.Keyword("NOT", true).AndSkip(whiteSpaceOrOpenParen),
            Terms.Text("!"));

        var and = OneOf(Terms.Keyword("AND", true), Terms.Text("&&"));
        var or = OneOf(Terms.Keyword("OR", true), Terms.Text("||"));

        var bitwiseAnd = Terms.Text("&");
        var bitwiseOr = Terms.Text("|");
        var bitwiseXOr = Terms.Text("^");
        var bitwiseNot = Terms.Text("~");

        // "(" expression ")"
        var groupExpression = Between(openParen, expression, closeParen).Named("Group");

        var braceIdentifier = openBrace
            .SkipAnd(AnyCharBefore(closeBrace, failOnEof: true, consumeDelimiter: true)
                .Else(static context => throw new NCalcParserException($"Brace not closed. at position {context.Scanner.Cursor.Position}")));

        var curlyBraceIdentifier =
            openCurlyBrace.SkipAnd(AnyCharBefore(closeCurlyBrace, failOnEof: true, consumeDelimiter: true)
                .Else(static context => throw new NCalcParserException($"Brace not closed. at position {context.Scanner.Cursor.Position}")));

        // ("[" | "{") anything ("]" | "}"). The names come from a pool, so the trees of parsed expressions share them.
        var bracedIdentifier = OneOf(braceIdentifier, curlyBraceIdentifier)
            .ToPooledString()
            .Then<LogicalExpression>(static x => new Identifier(x)).Named("Identifier");

        // list => "(" (expression (argumentSeparator expression)*)? ")"
        var populatedList =
            Between(openParen, Separated(argumentSeparatorTerm, expression),
                    closeParen.Else(static context => throw new NCalcParserException($"Parenthesis not closed. at position {context.Scanner.Cursor.Position}")))
                .Then<LogicalExpression>(static values => new LogicalExpressionList(values));

        var emptyList = openParen.AndSkip(closeParen).Then<LogicalExpression>(static _ => new LogicalExpressionList());

        var list = OneOf(emptyList, populatedList).Named("List");

        // function | identifier: the identifier is read once, then a list makes it a function.
        var identifierOrFunction = identifier
            .ToPooledString()
            .And(list.Optional())
            .Then<LogicalExpression>(static x => x.Item2.TryGetValue(out var arguments)
                ? new Function(new Identifier(x.Item1), (LogicalExpressionList)arguments)
                : new Identifier(x.Item1)).Named("IdentifierOrFunction");

        var booleanTrue = Terms.Keyword("true", true)
            .Then<LogicalExpression>(static _ => True).Named("True");
        var booleanFalse = Terms.Keyword("false", true)
            .Then<LogicalExpression>(static _ => False).Named("False");

        var singleQuotesStringValue = Terms.String(StringLiteralQuotes.Single, NCalcStringEscapes, returnDecoded: false)
            .Then(value => ParseSingleQuotedString(value.Span, options.AllowCharValues))
            .Named("SingleQuotedString");

        var doubleQuotesStringValue = Terms.String(StringLiteralQuotes.Double, NCalcStringEscapes, returnDecoded: false)
            .Then(static value => ParseDoubleQuotedString(value.Span))
            .Named("DoubleQuotedString");

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

        // time => number:number:number{.fractional}
        var timeDefinition = charIsNumber
            .AndSkip(timeSeparator)
            .And(charIsNumber)
            .AndSkip(timeSeparator)
            .And(charIsNumber)
            .AndSkip(decimalSeparator.Optional())
            .And(charIsNumber.Optional().Then(static value => value.HasValue ? value.Value : TextSpan.Empty));

        var dateOrDateTime = dateDefinition
            .And(Literals.WhiteSpace().SkipAnd(timeDefinition).Optional())
            .Then(value => value.Item4.TryGetValue(out var t)
                ? ParseDateAndTime(
                    value.Item1.Span, value.Item2.Span, value.Item3.Span,
                    t.Item1.Span, t.Item2.Span, t.Item3.Span,
                    t.Item4.Span, cultureInfo)
                : ParseDate(
                    value.Item1.Span, value.Item2.Span, value.Item3.Span, cultureInfo))
            .Named("DateOrDateTime");

        var time = timeDefinition.Then(value => ParseTime(
            value.Item1.Span, value.Item2.Span, value.Item3.Span, value.Item4.Span, cultureInfo)).Named("Time");

        // datetime => '#' dateOrDateTime | time  '#';
        var dateTime = Terms
            .Char('#')
            .SkipAnd(OneOf(dateOrDateTime, time))
            .AndSkip(Literals.Char('#')).Named("DateTimeLiteral");

        var eightHexSequence = Literals.AnyOf(Character.HexDigits, 8, 8);
        var fourHexSequence = Literals.AnyOf(Character.HexDigits, 4, 4);
        var twelveHexSequence = Literals.AnyOf(Character.HexDigits, 12, 12);
        var twentyFourHexSequence = Literals.AnyOf(Character.HexDigits, 24, 24);
        var hyphen = Literals.Char('-');

        var hyphenatedTail = hyphen
            .And(fourHexSequence)
            .And(hyphen)
            .And(fourHexSequence)
            .And(hyphen)
            .And(fourHexSequence)
            .And(hyphen)
            .And(twelveHexSequence)
            .Then(static _ => 0);

        var unhyphenatedTail = twentyFourHexSequence
            .AndSkip(Not(decimalOrDoubleNumber))
            .Then(static _ => 0);

        var guid = SkipWhiteSpace(Capture(eightHexSequence.And(OneOf(hyphenatedTail, unhyphenatedTail))))
            .Then(static value => ParseGuid(value.Span))
            .Named("Guid");

        var intOrLong = OneOf(intNumber, longNumber);
        var integralNumber = If(
            () => options.IntegerNumberType == IntegerNumberType.Int64,
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
            identifierOrFunction,
            groupExpression,
            bracedIdentifier,
            list).Named("Primary");

        // The operator levels, from the one which binds the tightest, are parsed by a single precedence parser which looks
        // for the operator following an operand once, whatever the number of levels. An operator that isn't followed by an
        // operand is not read, and the looser levels try their operators at its position: "&&" after "&", "<=" after "<".
        //
        // factorial      => primary ( "!" )* ;
        // exponential    => factorial ( "**" exponential )? ;
        // unary          => ( "-" | "not" | "!" | "~" ) unary | exponential ;
        // multiplicative => unary ( ( "/" | "*" | "%" ) unary )* ;
        // additive       => multiplicative ( ( "+" | "-" ) multiplicative )* ;
        // shift          => additive ( ( "<<" | ">>" ) additive )* ;
        // relational     => shift ( ( ">=" | "<=" | "<" | ">" | "in" | "not in" | "like" | "not like" ) shift )* ;
        // equality       => relational ( ( "==" | "=" | "!=" | "<>" ) relational )* ;
        // bitwiseAnd     => equality ( "&" equality )* ;
        // bitwiseXor     => bitwiseAnd ( "^" bitwiseAnd )* ;
        // bitwiseOr      => bitwiseXor ( "|" bitwiseXor )* ;
        // logicalAnd     => bitwiseOr ( ( "and" | "&&" ) bitwiseOr )* ;
        // logicalOr      => logicalAnd ( ( "or" | "||" ) logicalAnd )* ;
        // coalescing     => logicalOr ( "??" coalescing )? ;
        var coalescing = primary.Operators()
            .Postfix((exclamation.AndSkip(Not(equalityGuard)), static value => new UnaryExpression(UnaryExpressionType.Factorial, value)))
            .RightAssociative((exponent, static (a, b) => new BinaryExpression(BinaryExpressionType.Exponentiation, a, b)))
            .Unary(
                (not, static value => new UnaryExpression(UnaryExpressionType.Not, value)),
                (minus, static value => new UnaryExpression(UnaryExpressionType.Negate, value)),
                (bitwiseNot, static value => new UnaryExpression(UnaryExpressionType.BitwiseNot, value)))
            .LeftAssociative(
                (divided, static (a, b) => new BinaryExpression(BinaryExpressionType.Div, a, b)),
                (times, static (a, b) => new BinaryExpression(BinaryExpressionType.Times, a, b)),
                (modulo, static (a, b) => new BinaryExpression(BinaryExpressionType.Modulo, a, b)))
            .LeftAssociative(
                (plus, static (a, b) => new BinaryExpression(BinaryExpressionType.Plus, a, b)),
                (minus, static (a, b) => new BinaryExpression(BinaryExpressionType.Minus, a, b)))
            .LeftAssociative(
                (leftShift, static (a, b) => new BinaryExpression(BinaryExpressionType.LeftShift, a, b)),
                (rightShift, static (a, b) => new BinaryExpression(BinaryExpressionType.RightShift, a, b)))
            .LeftAssociative(
                (greaterOrEqual, static (a, b) => new BinaryExpression(BinaryExpressionType.GreaterOrEqual, a, b)),
                (lesserOrEqual, static (a, b) => new BinaryExpression(BinaryExpressionType.LesserOrEqual, a, b)),
                (lesser, static (a, b) => new BinaryExpression(BinaryExpressionType.Lesser, a, b)),
                (greater, static (a, b) => new BinaryExpression(BinaryExpressionType.Greater, a, b)),
                (@in, static (a, b) => new BinaryExpression(BinaryExpressionType.In, a, b)),
                (notIn, static (a, b) => new BinaryExpression(BinaryExpressionType.NotIn, a, b)),
                (like, static (a, b) => new BinaryExpression(BinaryExpressionType.Like, a, b)),
                (notLike, static (a, b) => new BinaryExpression(BinaryExpressionType.NotLike, a, b)))
            .LeftAssociative(
                (equal, static (a, b) => new BinaryExpression(BinaryExpressionType.Equal, a, b)),
                (notEqual, static (a, b) => new BinaryExpression(BinaryExpressionType.NotEqual, a, b)))
            .LeftAssociative((bitwiseAnd, static (a, b) => new BinaryExpression(BinaryExpressionType.BitwiseAnd, a, b)))
            .LeftAssociative((bitwiseXOr, static (a, b) => new BinaryExpression(BinaryExpressionType.BitwiseXOr, a, b)))
            .LeftAssociative((bitwiseOr, static (a, b) => new BinaryExpression(BinaryExpressionType.BitwiseOr, a, b)))
            .LeftAssociative((and, static (a, b) => new BinaryExpression(BinaryExpressionType.And, a, b)))
            .LeftAssociative((or, static (a, b) => new BinaryExpression(BinaryExpressionType.Or, a, b)))
            .RightAssociative((coalesce, static (a, b) => new BinaryExpression(BinaryExpressionType.Coalesce, a, b)));

        // ternary => coalescing ( "?" coalescing ":" ternary )? ;
        var ternary = coalescing
            .Ternary(questionMark, coalescing, colon, static (condition, whenTrue, whenFalse) => new TernaryExpression(condition, whenTrue, whenFalse))
            .Named("Ternary");

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

        expression.Parser = ternary;

        // The error is raised by the end-of-input check itself so the reported position is where parsing stopped,
        // not the start of the enclosing sequence, which is reset when the sequence fails.
        var endOfInput = Eof()
            .Else(static context => throw new NCalcParserException($"Invalid token in expression at position {context.Scanner.Cursor.Position}"));
        var expressionParser = operatorSequence.AndSkip(ZeroOrMany(Literals.WhiteSpace(true))).AndSkip(endOfInput)
            .Named("Expression");

        return expressionParser;
    }
}
