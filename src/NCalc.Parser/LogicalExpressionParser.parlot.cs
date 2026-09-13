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
            .SkipAnd(AnyCharBefore(closeBrace, failOnEof: true, consumeDelimiter: true)
                .Else(static context => throw new NCalcParserException($"Brace not closed. at position {context.Scanner.Cursor.Position}")));

        var curlyBraceIdentifier =
            openCurlyBrace.SkipAnd(AnyCharBefore(closeCurlyBrace, failOnEof: true, consumeDelimiter: true)
                .Else(static context => throw new NCalcParserException($"Brace not closed. at position {context.Scanner.Cursor.Position}")));

        // ("[" | "{") identifier ("]" | "}")
        var identifierExpression = OneOf(
                braceIdentifier,
                curlyBraceIdentifier,
                identifier)
            .Then<LogicalExpression>(static x => new Identifier(x.ToString())).Named("Identifier");

        // list => "(" (expression (argumentSeparator expression)*)? ")"
        var populatedList =
            Between(openParen, Separated(argumentSeparatorTerm, expression),
                    closeParen.Else(static context => throw new NCalcParserException($"Parenthesis not closed. at position {context.Scanner.Cursor.Position}")))
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
            .Then(value => ParseSingleQuotedString(value, options.AllowCharValues))
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
        var date = dateDefinition.Then(value => ParseDate(
            value.Item1.ToString(), value.Item2.ToString(), value.Item3.ToString(), cultureInfo)).Named("Date");

        // time => number:number:number{.fractional}
        var timeDefinition = charIsNumber
            .AndSkip(timeSeparator)
            .And(charIsNumber)
            .AndSkip(timeSeparator)
            .And(charIsNumber)
            .AndSkip(ZeroOrOne(decimalSeparator))
            .And(ZeroOrOne(charIsNumber));

        var time = timeDefinition.Then(value => ParseTime(
            value.Item1.ToString(), value.Item2.ToString(), value.Item3.ToString(), value.Item4.ToString(), cultureInfo)).Named("Time");

        // dateAndTime => number/number/number number:number:number{.fractional}
        var dateAndTime = dateDefinition.AndSkip(Literals.WhiteSpace()).And(timeDefinition)
            .Then(value => ParseDateAndTime(
                value.Item1.ToString(), value.Item2.ToString(), value.Item3.ToString(),
                value.Item4.Item1.ToString(), value.Item4.Item2.ToString(), value.Item4.Item3.ToString(),
                value.Item4.Item4.ToString(), cultureInfo)).Named("DateAndTime");

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
            .Then(static value => ParseGuid(value.Span));

        var guidWithoutHyphens = thirtyTwoHexSequence
            .AndSkip(Not(decimalOrDoubleNumber))
            .Then(static value => ParseGuid(value.Span));

        var guid = OneOf(guidWithHyphens, guidWithoutHyphens).Named("Guid");

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
            .Else(static context => throw new NCalcParserException($"Invalid token in expression at position {context.Scanner.Cursor.Position}"))
            .Named("Expression");

        return expressionParser;
    }
}
