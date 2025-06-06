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

    private static Parser<LogicalExpression> CreateExpressionParser(CultureInfo cultureInfo)
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

        var hexNumber = Terms.Text("0x")
            .SkipAnd(Terms.Pattern(c => "0123456789abcdefABCDEF".Contains(c)))
            .Then(x => Convert.ToInt64(x.ToString(), 16));

        var octalNumber = Terms.Text("0o")
            .SkipAnd(Terms.Pattern(c => "01234567".Contains(c)))
            .Then(x => Convert.ToInt64(x.ToString(), 8));

        var binaryNumber = Terms.Text("0b")
            .SkipAnd(Terms.Pattern(c => c == '0' || c == '1'))
            .Then(x => Convert.ToInt64(x.ToString(), 2));

        var hexOctBinNumber = OneOf(hexNumber, octalNumber, binaryNumber)
            .Then<LogicalExpression>(d =>
            {
                if (d is > int.MaxValue or < int.MinValue)
                    return new ValueExpression(d);

                return new ValueExpression((int)d);
            });

        var intNumber = Terms.Number<int>(NumberOptions.Integer)
            .AndSkip(Not(OneOf(Terms.Text("."), Terms.Text("E", true))))
            .Then<LogicalExpression>(d => new ValueExpression(d));

        var longNumber = Terms.Number<long>(NumberOptions.Integer)
            .AndSkip(Not(OneOf(Terms.Text("."), Terms.Text("E", true))))
            .Then<LogicalExpression>(d => new ValueExpression(d));

        var decimalNumber = Terms.Number<decimal>(NumberOptions.Float)
            .Then<LogicalExpression>(static (ctx, val) =>
            {
                bool useDecimal = ((LogicalExpressionParserContext)ctx).Options.HasFlag(ExpressionOptions.DecimalAsDefault);
                if (useDecimal)
                    return new ValueExpression(val);

                return new ValueExpression((double)val);
            });

        var doubleNumber = Terms.Number<double>(NumberOptions.Float)
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

        var comma = Terms.Char(',');
        var divided = Terms.Text("/");
        var times = Terms.Text("*");
        var modulo = Terms.Text("%");
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

        var dateSeparator = cultureInfo.DateTimeFormat.DateSeparator;
        var timeSeparator = cultureInfo.DateTimeFormat.TimeSeparator;

        var dateDefinition = charIsNumber
            .AndSkip(Literals.Text(dateSeparator))
            .And(charIsNumber)
            .AndSkip(Literals.Text(dateSeparator))
            .And(charIsNumber);

        // date => number/number/number
        var date = dateDefinition.Then<LogicalExpression>(date =>
        {
            if (DateTime.TryParse($"{date.Item1}{dateSeparator}{date.Item2}{dateSeparator}{date.Item3}",
                    cultureInfo, DateTimeStyles.None, out var result))
            {
                return new ValueExpression(result);
            }

            throw new FormatException("Invalid DateTime format.");
        });

        // time => number:number:number
        var timeDefinition = charIsNumber
            .AndSkip(Literals.Text(timeSeparator))
            .And(charIsNumber)
            .AndSkip(Literals.Text(timeSeparator))
            .And(charIsNumber);

        var time = timeDefinition.Then<LogicalExpression>(time =>
        {
            if (TimeSpan.TryParse($"{time.Item1}{timeSeparator}{time.Item2}{timeSeparator}{time.Item3}", cultureInfo, out var result))
            {
                return new ValueExpression(result);
            }

            throw new FormatException("Invalid TimeSpan format.");
        });

        // dateAndTime => number/number/number number:number:number
        var dateAndTime = dateDefinition.AndSkip(Literals.WhiteSpace()).And(timeDefinition).Then<LogicalExpression>(
            dateTime =>
            {
                if (DateTime.TryParse(
                        $"{dateTime.Item1}{dateSeparator}{dateTime.Item2}{dateSeparator}{dateTime.Item3} {dateTime.Item4.Item1}{timeSeparator}{dateTime.Item4.Item2}{timeSeparator}{dateTime.Item4.Item3}",
                        cultureInfo, DateTimeStyles.None, out var result))
                {
                    return new ValueExpression(result);
                }

                throw new FormatException("Invalid DateTime format.");
            });

        // datetime => '#' dateAndTime | date | time  '#';
        var dateTime = Terms
            .Char('#')
            .SkipAnd(OneOf(dateAndTime, date, time))
            .AndSkip(Literals.Char('#'));

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

        return enableParserCompilation ? expressionParser.Compile() : expressionParser;
    }

    private static Parser<LogicalExpression> GetOrCreateExpressionParser(CultureInfo cultureInfo)
    {
        if (Parsers.TryGetValue(cultureInfo, out var parser))
            return parser;

        var newParser = CreateExpressionParser(cultureInfo);
        Parsers.TryAdd(cultureInfo, newParser);

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
        var parser = GetOrCreateExpressionParser(context.CultureInfo);

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