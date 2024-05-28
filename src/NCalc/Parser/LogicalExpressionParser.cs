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

        var exponentNumberPart = Literals.Text("e", true).SkipAnd(Literals.Integer(NumberOptions.AllowSign)).Then(x => x);
        
        
        // [integral_value]['.'decimal_value}]['e'exponent_value]
        var number =
            SkipWhiteSpace(OneOf(
                Literals.Char('.')
                    .SkipAnd(Terms.Integer().Then<long?>(x => x))
                    .And(exponentNumberPart.ThenElse<long?>(x => x, null))
                    .AndSkip(Not(Literals.Identifier()).ElseError("Invalid token in expression"))
                    .Then(x => (0L, x.Item1, x.Item2)),
                Literals.Integer(NumberOptions.AllowSign)
                    .And(Literals.Char('.')
                    .SkipAnd(ZeroOrOne(Terms.Integer()))
                    .ThenElse<long?>(x => x, null))
                    .And(exponentNumberPart.ThenElse<long?>(x => x, null))
                    .AndSkip(Not(Literals.Identifier()).ElseError("Invalid token in expression"))
                    .Then(x => (x.Item1, x.Item2, x.Item3))
                ))
            .Then<LogicalExpression>((ctx, x) =>
            {
                long integralValue = x.Item1;
                long? decimalPart = x.Item2;
                long? exponentPart = x.Item3;

                decimal result = integralValue;

                // decimal part?
                if (decimalPart != null && decimalPart.Value != 0)
                {
                    var digits = Math.Floor(Math.Log10(decimalPart.Value) + 1);
                    result += decimalPart.Value / (decimal)Math.Pow(10, digits);
                }

                // exponent part?
                if (exponentPart != null)
                {
                    result *= (decimal)Math.Pow(10, exponentPart.Value);
                }

                if (ctx is LogicalExpressionParserContext { UseDecimalsAsDefault: true })
                {
                    return new ValueExpression(result);
                }

                if (decimalPart != null || (exponentPart != null))
                {
                    return new ValueExpression((double)result);
                }

                return new ValueExpression((long)result);
            });

        var comma = Terms.Char(',');
        var divided = Terms.Char('/');
        var times = Terms.Char('*');
        var modulo = Terms.Char('%');
        var minus = Terms.Text("-");
        var plus = Terms.Text("+");
        var bitwiseNot = Terms.Text("~");
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

        var negate = Terms.Text("!");
        var not = Terms.Text("not", true);

        // "(" expression ")"
        var groupExpression = Between(openParen, expression, closeParen.ElseError("Parenthesis not closed."));

        // ("[" | "{") identifier ("]" | "}")
        var identifierExpression = openBrace.Or(openCurlyBrace)
            .SkipAnd(AnyCharBefore(closeBrace.Or(closeCurlyBrace), consumeDelimiter: true))
            .Or(Terms.Identifier())
            .Then<LogicalExpression>(x => new Identifier(x.ToString()));

        var arguments = Separated(comma.Or(semicolon), expression);

        var functionWithArguments = Terms
            .Identifier()
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
            .AndSkip(Terms.Char(':'))
            .And(charIsNumber)
            .AndSkip(Terms.Char(':'))
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
        var dateAndTime = dateDefinition.AndSkip(Literals.WhiteSpace()).And(timeDefinition).Then<LogicalExpression>(dateTime =>
        {
            if (DateTime.TryParse($"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}:{dateTime.Item4.Item3}", out var result))
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

        
        var decimalNumber = Terms.Decimal().Then<LogicalExpression>(d=> new ValueExpression(d));
        var doubleNumber = Terms.Double().Then<LogicalExpression>(d=> new ValueExpression(d));
        
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
                        result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item2[i - 1].Item2, x.Item2[i].Item2);
                    }

                    result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item1, result);
                }

                return result;
            });

        // The Recursive helper allows to create parsers that depend on themselves.
        // ( "-" | "not" ) unary | primary;
        var unary = Recursive<LogicalExpression>(u =>
                OneOf(
                    not.Then(UnaryExpressionType.Not),
                    negate.Then(UnaryExpressionType.Not),
                    minus.Then(UnaryExpressionType.Negate),
                    bitwiseNot.Then(UnaryExpressionType.BitwiseNot))
                .And(u).Then<LogicalExpression>(static x => new UnaryExpression(x.Item1, x.Item2))
                .Or(exponential)
                );

        // multiplicative => unary ( ( "/" | "*" | "%" ) unary )* ;
        var multiplicative = unary.And(ZeroOrMany(
            divided.Then(BinaryExpressionType.Div)
            .Or(times.Then(BinaryExpressionType.Times))
            .Or(modulo.Then(BinaryExpressionType.Modulo))
            .And(unary)))
            .Then(static x =>
            {
                var result = x.Item1;

                foreach (var op in x.Item2)
                {
                    result = new BinaryExpression(op.Item1, result, op.Item2);
                }

                return result;
            }).Or(unary);

        // additive => multiplicative ( ( "-" | "+" ) multiplicative )* ;
        var additive = multiplicative.And(ZeroOrMany(
            plus.Then(BinaryExpressionType.Plus)
            .Or(minus.Then(BinaryExpressionType.Minus))
            .And(multiplicative)))
            .Then(static x =>
            {
                var result = x.Item1;
                foreach (var op in x.Item2)
                {
                    result = new BinaryExpression(op.Item1, result, op.Item2);
                }

                return result;
            }).Or(multiplicative);

        // shift => additive ( ( "<<" | ">>" ) additive )* ;
        var shift = additive.And(ZeroOrMany(
                Terms.Text("<<").Then(BinaryExpressionType.LeftShift)
                .Or(Terms.Text(">>").Then(BinaryExpressionType.RightShift))
                .And(additive)))
            .Then(static x =>
            {
                var result = x.Item1;
                foreach (var op in x.Item2)
                {
                    result = new BinaryExpression(op.Item1, result, op.Item2);
                }

                return result;
            }).Or(additive);

        // relational => shift ( ( ">=" | "<=" | "<" | ">" ) shift )* ;
        var relational = shift.And(ZeroOrMany(OneOf(
                    Terms.Text(">=").Then(BinaryExpressionType.GreaterOrEqual),
                    Terms.Text("<=").Then(BinaryExpressionType.LesserOrEqual),
                    Terms.Text("<").Then(BinaryExpressionType.Lesser),
                    Terms.Text(">").Then(BinaryExpressionType.Greater))
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
                    Terms.Text("<>").Then(BinaryExpressionType.NotEqual),
                    Terms.Text("==").Then(BinaryExpressionType.Equal),
                    Terms.Text("!=").Then(BinaryExpressionType.NotEqual),
                    Terms.Text("=").Then(BinaryExpressionType.Equal))
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

        var and = Terms
            .Text("AND", true).Then(BinaryExpressionType.And)
            .Or(Terms.Text("&&").Then(BinaryExpressionType.And))
            .Or(Terms.Text("&").Then(BinaryExpressionType.BitwiseAnd));

        var or = Terms
            .Text("OR", true).Then(BinaryExpressionType.Or)
            .Or(Terms.Text("||").Then(BinaryExpressionType.Or))
            .Or(Terms.Text("|").Then(BinaryExpressionType.BitwiseOr));

        var xor = Terms.Text("^").Then(BinaryExpressionType.BitwiseXOr);

        // logical => equality ( ( "and" | "or" ) equality )* ;
        var logical = equality.And(ZeroOrMany(OneOf(and, or, xor).And(equality)))
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
                : new TernaryExpression(x.Item1, x.Item2.Item1, x.Item2.Item2));

        expression.Parser = ternary;
        Parser = expression.Compile();
    }

    public static LogicalExpression Parse(LogicalExpressionParserContext context)
    {
        if (!Parser.TryParse(context, out LogicalExpression result, out ParseError error))
        {
            string message;
            if (error != null)
                message = $"{error.Message} at position {error.Position}";
            else
                message = $"Error parsing the expression at position {context.Scanner.Cursor.Position}";

            throw new NCalcParserException(message);
        }

        return result;
    }
}
