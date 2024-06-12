using ExtendedNumerics;
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
    private static readonly Parser<LogicalExpression> Parser;

    private static readonly ValueExpression True = new(true);
    private static readonly ValueExpression False = new(false);

    private const string InvalidTokenMessage = "Invalid token in expression";

    static LogicalExpressionParser()
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
         *                  | "(" expression ")" ;
         *
         * function       => Identifier "(" arguments ")"
         * arguments      => expression ( ("," | ";") expression )*
         */
        // The Deferred helper creates a parser that can be referenced by others before it is defined
        var expression = Deferred<LogicalExpression>();

        var exponentNumberPart =
            Literals.Text("e", true).SkipAnd(Literals.Integer(NumberOptions.AllowSign)).Then(x => x);

        // [integral_value]['.'decimal_value}]['e'exponent_value]
        var number =
            SkipWhiteSpace(OneOf(
                    Literals.Char('.')
                        .SkipAnd(ZeroOrMany(Terms.Char('0')).ThenElse(x => x.Count, 0))
                        .And(Terms.Integer().Then<long?>(x => x))
                        .And(exponentNumberPart.ThenElse<long?>(x => x, null))
                        .Then(x => (0L, x.Item1, x.Item2, x.Item3)),
                    Literals.Integer(NumberOptions.AllowSign)
                        .And(Literals.Char('.')
                            .SkipAnd(ZeroOrMany(Terms.Char('0')).ThenElse(x => x.Count, 0))
                            .And(ZeroOrOne(Terms.Integer()))
                            .ThenElse<(int, long?)>(x => (x.Item1, x.Item2), (0, null)))
                        .And(exponentNumberPart.ThenElse<long?>(x => x, null))
                        .Then(x => (x.Item1, x.Item2.Item1, x.Item2.Item2, x.Item3))
                ))
                .Then<LogicalExpression>((ctx, x) =>
                {
                    long integralValue = x.Item1;
                    int zeroCount = x.Item2;
                    long? decimalPart = x.Item3;
                    long? exponentPart = x.Item4;

                    double result = integralValue;

                    // decimal part?
                    if (decimalPart != null && decimalPart.Value != 0)
                    {
                        var digits = Math.Floor(Math.Log10(decimalPart.Value) + 1) + zeroCount;
                        result += decimalPart.Value / Math.Pow(10, digits);
                    }

                    // exponent part?
                    if (exponentPart != null)
                    {
                        var left = BigDecimal.Parse(result);
                        var right = BigDecimal.Pow(10, exponentPart.Value);

                        var res = BigDecimal.Multiply(left, right);

                        if (res > double.MaxValue)
                            result = double.PositiveInfinity;
                        else if (res < double.MinValue)
                            result = double.NegativeInfinity;
                        else
                            result = (double)res;
                    }

                    if (ctx is LogicalExpressionParserContext { UseDecimalsAsDefault: true })
                    {
                        return new ValueExpression((decimal)result);
                    }

                    if (decimalPart != null || exponentPart != null)
                    {
                        return new ValueExpression(result);
                    }

                    return new ValueExpression((long)result);
                });

        var comma = Terms.Char(',');
        var divided = Terms.Text("/");
        var times = Terms.Text("*");
        var modulo = Terms.Text("%");
        var minus = Terms.Text("-");
        var plus = Terms.Text("+");

        var equal = OneOf(Terms.Text("=="), Terms.Text("="));
        var notEqual = OneOf(Terms.Text("<>"), Terms.Text("!="));

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
        var bitwiserOr = Terms.Text("|");
        var bitwiseXOr = Terms.Text("^");
        var bitwiseNot = Terms.Text("~");

        // "(" expression ")"
        var groupExpression = Between(openParen, expression, closeParen.ElseError("Parenthesis not closed."));

        var braceIdentifier = openBrace
            .SkipAnd(AnyCharBefore(closeBrace, consumeDelimiter: true));

        var curlyBraceIdentifier =
            openCurlyBrace.SkipAnd(AnyCharBefore(closeCurlyBrace, consumeDelimiter: true));

        // ("[" | "{") identifier ("]" | "}")
        var identifierExpression = OneOf(braceIdentifier, curlyBraceIdentifier)
            .Then<LogicalExpression>(x => new Identifier(x.ToString()));

        var arguments = Separated(comma.Or(semicolon), expression);

        var functionWithArguments = identifier
            .And(openParen.SkipAnd(arguments).AndSkip(closeParen))
            .Then<LogicalExpression>(x => new Function(new Identifier(x.Item1.ToString()), x.Item2.ToArray()));

        var functionWithoutArguments = Terms
            .Identifier()
            .And(openParen.AndSkip(closeParen))
            .Then<LogicalExpression>(x => new Function(new Identifier(x.Item1.ToString()), []));

        var function = OneOf(functionWithArguments, functionWithoutArguments);

        var booleanTrue = Terms.Text("true", true)
            .Then<LogicalExpression>(True);
        var booleanFalse = Terms.Text("false", true)
            .Then<LogicalExpression>(False);

        var stringValue = Terms.String(quotes: StringLiteralQuotes.SingleOrDouble)
            .Then<LogicalExpression>(x => new ValueExpression(x.ToString()));

        var charIsNumber = Literals.Pattern(char.IsNumber);

        var dateDefinition = charIsNumber
            .AndSkip(divided)
            .And(charIsNumber)
            .AndSkip(divided)
            .And(charIsNumber);

        // date => number/number/number
        var date = dateDefinition.Then<LogicalExpression>(date =>
        {
            if (DateTime.TryParse($"{date.Item1}/{date.Item2}/{date.Item3}", out var result))
            {
                return new ValueExpression(result);
            }

            throw new FormatException("Invalid DateTime format.");
        });

        // time => number:number:number
        var timeDefinition = charIsNumber
            .AndSkip(colon)
            .And(charIsNumber)
            .AndSkip(colon)
            .And(charIsNumber);

        var time = timeDefinition.Then<LogicalExpression>(time =>
        {
            if (TimeSpan.TryParse($"{time.Item1}:{time.Item2}:{time.Item3}", out var result))
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
                        $"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}:{dateTime.Item4.Item3}",
                        out var result))
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


        var decimalNumber = Terms.Decimal(NumberOptions.AllowSign).Then<LogicalExpression>(d => new ValueExpression(d));
        var doubleNumber = Terms.Double(NumberOptions.AllowSign).Then<LogicalExpression>(d => new ValueExpression(d));

        // primary => NUMBER | "[" identifier "]" | DateTime | string | function | boolean | "(" expression ")";
        var primary = OneOf(
            number,
            decimalNumber,
            doubleNumber,
            booleanTrue,
            booleanFalse,
            dateTime,
            stringValue,
            function,
            groupExpression,
            identifierExpression);

        // exponential => unary ( "**" unary )* ;
        var exponential = primary.And(ZeroOrMany(exponent.And(primary)))
            .Then(static x =>
            {
                LogicalExpression result = null!;

                if (x.Item2.Count == 0)
                {
                    result = x.Item1;
                }
                else if (x.Item2.Count == 1)
                {
                    result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item1, x.Item2[0].Item2);
                }
                else
                {
                    for (int i = x.Item2.Count - 1; i > 0; i--)
                    {
                        result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item2[i - 1].Item2,
                            x.Item2[i].Item2);
                    }

                    result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item1, result);
                }

                return result;
            });

        // ( "-" | "not" ) unary | primary;
        var unary = exponential.Unary(
            (not, value => new UnaryExpression(UnaryExpressionType.Not, value)),
            (minus, value => new UnaryExpression(UnaryExpressionType.Negate, value)),
            (bitwiseNot, value => new UnaryExpression(UnaryExpressionType.BitwiseNot, value))
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

        // relational => shift ( ( ">=" | "<=" | "<" | ">" ) shift )* ;
        var relational = shift.And(ZeroOrMany(OneOf(
                    greaterOrEqual.Then(BinaryExpressionType.GreaterOrEqual),
                    lesserOrEqual.Then(BinaryExpressionType.LesserOrEqual),
                    lesser.Then(BinaryExpressionType.Lesser),
                    greater.Then(BinaryExpressionType.Greater))
                .And(shift)))
            .Then(static x =>
            {
                var result = x.Item1;
                foreach (var op in x.Item2)
                {
                    result = new BinaryExpression(op.Item1, result, op.Item2);
                }

                return result;
            });

        var equality = relational.And(ZeroOrMany(OneOf(
                    equal.Then(BinaryExpressionType.Equal),
                    notEqual.Then(BinaryExpressionType.NotEqual))
                .And(relational)))
            .Then(static x =>
            {
                var result = x.Item1;
                foreach (var op in x.Item2)
                {
                    result = new BinaryExpression(op.Item1, result, op.Item2);
                }

                return result;
            });

        var andParser = and.Then(BinaryExpressionType.And)
            .Or(bitwiseAnd.Then(BinaryExpressionType.BitwiseAnd));

        var orParser = or.Then(BinaryExpressionType.Or)
            .Or(bitwiserOr.Then(BinaryExpressionType.BitwiseOr));

        var xorParser = bitwiseXOr.Then(BinaryExpressionType.BitwiseXOr);

        // logical => equality ( ( "and" | "or" ) equality )* ;
        var logical = equality.And(ZeroOrMany(OneOf(andParser, orParser, xorParser).And(equality)))
            .Then(static x =>
            {
                var result = x.Item1;
                foreach (var op in x.Item2)
                {
                    result = new BinaryExpression(op.Item1, result, op.Item2);
                }

                return result;
            });

        // ternary => logical("?" logical ":" logical) ?
        var ternary = logical.And(ZeroOrOne(questionMark.SkipAnd(logical).AndSkip(colon).And(logical)))
            .Then(x => x.Item2.Item1 == null
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

#if NET6_0_OR_GREATER
        if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
            Parser = expressionParser.Compile();
        else
            Parser = expressionParser;
#else
        Parser = expressionParser.Compile();
#endif
    }

    public static LogicalExpression Parse(LogicalExpressionParserContext context)
    {
        if (Parser.TryParse(context, out LogicalExpression result, out ParseError error))
            return result;

        string message;
        if (error != null)
            message = $"{error.Message} at position {error.Position}";
        else
            message = $"Error parsing the expression at position {context.Scanner.Cursor.Position}";

        throw new NCalcParserException(message);
    }
}