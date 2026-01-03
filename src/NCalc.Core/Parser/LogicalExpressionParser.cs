using NCalc.Domain;
using NCalc.Exceptions;
using Parlot;
using Parlot.Fluent;
using Parlot.SourceGenerator;

namespace NCalc.Parser;

/// <summary>
/// Class responsible for parsing strings into <see cref="LogicalExpression"/> objects.
/// </summary>
public static partial class LogicalExpressionParser
{
    private const double MinDecDouble = (double)decimal.MinValue;
    private const double MaxDecDouble = (double)decimal.MaxValue;

    private const string InvalidTokenMessage = "Invalid token in expression";

    private static long ParseInt64Base(ReadOnlySpan<char> s, int @base)
    {
        if ((uint)(@base - 2) > 34u)
            throw new ArgumentOutOfRangeException(nameof(@base));

        long value = 0;

        foreach (var ch in s)
        {
            int digit = ch switch
            {
                >= '0' and <= '9' => ch - '0',
                >= 'a' and <= 'z' => ch - 'a' + 10,
                >= 'A' and <= 'Z' => ch - 'A' + 10,
                _ => throw new FormatException("Invalid digit.")
            };

            if ((uint)digit >= (uint)@base)
                throw new FormatException("Invalid digit.");

            checked
            {
                value = value * @base + digit;
            }
        }

        return value;
    }

    [GenerateParser]
    [IncludeUsings("NCalc.Parser", "NCalc.Domain", "NCalc.Exceptions")]
    [IncludeGenerators("PolySharp")]
    public static Parser<LogicalExpression> CreateExpressionParser()
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
            .SkipAnd(Terms.AnyOf(Character.HexDigits))
            .Then(static x => ParseInt64Base(x.Span, 16));

        var octalNumber = Terms.Text("0o")
            .SkipAnd(Terms.AnyOf(Character.OctalDigits))
            .Then(static x => ParseInt64Base(x.Span, 8));

        var binaryNumber = Terms.Text("0b")
            .SkipAnd(Terms.AnyOf(Character.BinaryDigits))
            .Then(static x => ParseInt64Base(x.Span, 2));

        var hexOctBinNumber = OneOf(hexNumber, octalNumber, binaryNumber)
            .Then<LogicalExpression>(static d =>
            {
                if (d is > int.MaxValue or < int.MinValue)
                    return new ValueExpression(d);

                return new ValueExpression((int)d);
            });

        var nonScientificParser = Not(OneOf(Terms.Text("."), Terms.Text("E", true)));

        var decimalNumber = Terms.Number<decimal>(NumberOptions.Float)
            .Then<LogicalExpression>(static d => new ValueExpression(d));

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

        var decimalOrDouble = OneOf(decimalNumber, doubleNumber);

        var decimalOrDoubleNumber = Select<LogicalExpressionParserContext, LogicalExpression>(static ctx =>
        {
            if (ctx.Options.HasFlag(ExpressionOptions.DecimalAsDefault))
                return 0; // decimalOrDouble

            return 1; // doubleNumber
        }, [decimalOrDouble, doubleNumber]);

        var argumentSeparator_Semicolon = Terms.Char(';');
        var argumentSeparator_Colon = Terms.Char(':');
        var argumentSeparator_Comma = Terms.Char(',');

        var argumentSeparatorDefinition = Select((ctx) =>
        {
            var argumentSeparator = ((LogicalExpressionParserContext)ctx).ParserOptions.ArgumentSeparator;
            return argumentSeparator switch
            {
                ArgumentSeparator.Semicolon => 0,// argumentSeparator_Semicolon
                ArgumentSeparator.Colon => 1,// argumentSeparator_Colon
                ArgumentSeparator.Comma => 2,// argumentSeparator_Comma
                _ => throw new FormatException($"Invalid Argument Separator {argumentSeparator}.")
            };
        }, [argumentSeparator_Semicolon, argumentSeparator_Colon, argumentSeparator_Comma]);

        var divided = Terms.Text("/");
        var times = Terms.Text("*");
        var modulo = Terms.Text("%");
        var minus = Terms.Text("-");
        var plus = Terms.Text("+");

        var equal = OneOf(Terms.Text("=="), Terms.Text("="));
        var notEqual = OneOf(Terms.Text("<>"), Terms.Text("!="));
        var @in = Terms.Text("in", true);
        var notIn = Capture(Terms.Text("not", true).And(Terms.Text("in", true)));

        var like = Terms.Text("like", true);
        var notLike = Capture(Terms.Text("not", true).And(Terms.Text("like", true)));

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
        var groupExpression = Between(openParen, expression, closeParen);

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
            .Then<LogicalExpression>(static x => new Domain.Identifier(x.ToString()));

        // list => "(" (expression (argumentSeparator expression)*)? ")"
        var populatedList =
            Between(openParen, Separated(argumentSeparatorDefinition, expression),
                    closeParen.ElseError("Parenthesis not closed."))
                .Then<LogicalExpression>(static values => new LogicalExpressionList(values));

        var emptyList = openParen.AndSkip(closeParen).Then<LogicalExpression>(static _ => new LogicalExpressionList());

        var list = OneOf(emptyList, populatedList);

        var function = identifier
            .And(list)
            .Then<LogicalExpression>(static x =>
            new Function(new Domain.Identifier(x.Item1.ToString()), (LogicalExpressionList)x.Item2));

        var booleanTrue = Terms.Text("true", true)
            .Then<LogicalExpression>(static _ => new ValueExpression(true));
        var booleanFalse = Terms.Text("false", true)
            .Then<LogicalExpression>(static _ => new ValueExpression(false));

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
                .Then<LogicalExpression>(static value => new ValueExpression(value.ToString()));

        var stringValue = OneOf(singleQuotesStringValue, doubleQuotesStringValue);

        var charIsNumber = Literals.Pattern(static c => char.IsNumber(c));

        var dateSeparator_Slash = Literals.Text("/");
        var dateSeparator_Dot = Literals.Text(".");
        var dateSeparator_Dash = Literals.Text("-");

        // Authorized date separators: '-', '.', '/' which are the most commonly used internationally.
        var dateSeparatorDefinition = Select((ctx) =>
        {
            var dateSeparator = ((LogicalExpressionParserContext)ctx).ParserOptions.CultureInfo.DateTimeFormat.DateSeparator;
            return dateSeparator switch
            {
                "/" => 0,// dateSeparator_Slash
                "." => 1,// dateSeparator_Dot
                "-" => 2,// dateSeparator_Dash
                _ => throw new FormatException($"Invalid DateTime format. Unknown date separator {dateSeparator}."),
            };
        }, [dateSeparator_Slash, dateSeparator_Dot, dateSeparator_Dash]);

        var dateDefinition = Capture(charIsNumber
            .And(dateSeparatorDefinition)
            .And(charIsNumber)
            .And(dateSeparatorDefinition)
            .And(charIsNumber));

        // date => number/number/number
        var date = dateDefinition.Then<LogicalExpression>(static (ctx, date) =>
        {
#if NET6_0_OR_GREATER
            if (DateTime.TryParse(date.Span, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return new ValueExpression(result);
            }
#else
            if (DateTime.TryParse(date.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return new ValueExpression(result);
            }
#endif

            throw new FormatException("Invalid DateTime format.");
        });

        var decimalSeparator_Dot = Literals.Text(".");
        var decimalSeparator_Comma = Literals.Text(",");

        var numberDecimalDefinition = Select((ctx) =>
        {
            var decimalSeparator = ((LogicalExpressionParserContext)ctx).ParserOptions.CultureInfo.NumberFormat.NumberDecimalSeparator;
            return decimalSeparator switch
            {
                "." => 0,// decimalSeparator_Dot
                "," => 1,// decimalSeparator_Comma
                _ => throw new FormatException($"Invalid DateTime format. Unknown decimal separator {decimalSeparator}.")
            };
        }, [decimalSeparator_Dot, decimalSeparator_Comma]);

        var timeSeparator_Colon = Literals.Text(":");
        var timeSeparator_Dot = Literals.Text(".");
        var timeSeparator_Comma = Literals.Text(",");

        var timeSeparatorDefinition = Select((ctx) =>
        {
            var timeSeparator = ((LogicalExpressionParserContext)ctx).ParserOptions.CultureInfo.DateTimeFormat.TimeSeparator;
            return timeSeparator switch
            {
                ":" => 0,// timeSeparator_Colon
                "." => 1,// timeSeparator_Dot
                "," => 2,// timeSeparator_Comma
                _ => throw new FormatException($"Invalid DateTime format. Unknown time separator {timeSeparator}.")
            };
        }, [timeSeparator_Colon, timeSeparator_Dot, timeSeparator_Comma]);

        // time => number:number:number{.fractional}
        var timeDefinition = Capture(charIsNumber
            .And(timeSeparatorDefinition)
            .And(charIsNumber)
            .And(timeSeparatorDefinition)
            .And(charIsNumber)
            .And(ZeroOrOne(numberDecimalDefinition.And(charIsNumber.Optional()))));

        var time = timeDefinition.Then<LogicalExpression>(static (ctx, time) =>
        {
            var cultureInfo = ((LogicalExpressionParserContext)ctx).ParserOptions.CultureInfo;
#if NET6_0_OR_GREATER
            if (TimeSpan.TryParse(time.Span, cultureInfo, out var result))
            {
                return new ValueExpression(result);
            }
#else
            if (TimeSpan.TryParse(time.ToString(), cultureInfo, out var result))
            {
                return new ValueExpression(result);
            }
#endif

            throw new FormatException("Invalid TimeSpan format.");
        });

        // dateAndTime => number/number/number number:number:number{.fractional}
        var dateAndTime = Capture(dateDefinition.And(Literals.WhiteSpace()).And(timeDefinition)).Then<LogicalExpression>(
            static (ctx, dateTime) =>
            {
                var cultureInfo = ((LogicalExpressionParserContext)ctx).ParserOptions.CultureInfo;

#if NET6_0_OR_GREATER
                if (DateTime.TryParse(dateTime.Span, cultureInfo, DateTimeStyles.None, out var result))
                {
                    return new ValueExpression(result);
                }
#else
                if (DateTime.TryParse(dateTime.ToString(), cultureInfo, DateTimeStyles.None, out var result))
                {
                    return new ValueExpression(result);
                }
#endif

                throw new FormatException("Invalid DateTime format.");
            });

        // datetime => '#' dateAndTime | date | time  '#';
        var dateTime = Terms
            .Char('#')
            .SkipAnd(OneOf(dateAndTime, date, time))
            .AndSkip(Literals.Char('#'));

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
            .Then<LogicalExpression>(static g =>
            {
#if NET6_0_OR_GREATER
                return new ValueExpression(Guid.Parse(g.Span));
#else
                return new ValueExpression(Guid.Parse(g.ToString()));
#endif
            });

        var guidWithoutHyphens = thirtyTwoHexSequence
            .AndSkip(Not(decimalOrDoubleNumber))
            .Then<LogicalExpression>(static g =>
            {
#if NET6_0_OR_GREATER
                return new ValueExpression(Guid.Parse(g.Span));
#else
                return new ValueExpression(Guid.Parse(g.ToString()));
#endif
            });

        var guid = OneOf(guidWithHyphens, guidWithoutHyphens);

        // Avoid instantiating parsers inside a Select() lambda (generator limitation).
        // Parse as long and convert to int when appropriate.
        var integralNumber = Terms.Number<long>(NumberOptions.Integer)
            .AndSkip(nonScientificParser)
            .Then<LogicalExpression>(static (ctx, value) =>
            {
                var options = ((LogicalExpressionParserContext)ctx).Options;

                if (!options.HasFlag(ExpressionOptions.LongAsDefault) && value is <= int.MaxValue and >= int.MinValue)
                    return new ValueExpression((int)value);

                return new ValueExpression(value);
            });

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
            list);

        // factorial => primary ( "!" )* ;
        var factorial = primary.And(ZeroOrMany(exclamation.AndSkip(Not(equal))))
            .Then(static x =>
            {
                var result = x.Item1;
                var count = x.Item2.Count;

                for (var i = 0; i < count; i++)
                    result = new UnaryExpression(UnaryExpressionType.Factorial, result);

                return result;
            });

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

        // Parlot's source generator can struggle to infer the operator parser's generic type
        // here, so we force it to a simple, known type and specify LeftAssociative's type args.
        var invalidOperatorSequence = OneOrMany(OneOf(
                divided, times, modulo, plus,
                minus, leftShift, rightShift, greaterOrEqual,
                lesserOrEqual, greater, lesser, equal,
                notEqual))
            .Then(static _ => 0);

        var operatorSequence = Parsers.LeftAssociative<LogicalExpression, int>(
            ternary,
            (op: invalidOperatorSequence, factory: ThrowUnknownOperatorSequence));

        expression.Parser = operatorSequence;
        var expressionParser = expression.AndSkip(ZeroOrMany(Literals.WhiteSpace(true))).Eof()
            .ElseError(InvalidTokenMessage);

        return expressionParser;
    }

    private static LogicalExpression ThrowUnknownOperatorSequence(LogicalExpression _, LogicalExpression __)
    {
        throw new InvalidOperationException("Unknown operator sequence.");
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
        var parser = CreateExpressionParser();

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