#nullable disable

// ReSharper disable AssignNullToNotNullAttribute

using System;
using System.Globalization;
using System.Linq;
using NCalc.Domain;
using NCalc.Exceptions;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;
using Identifier = NCalc.Domain.Identifier;

namespace NCalc.Parser;

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
         * ternary        => equality ( "?" equality ":" equality)?
         * equality       => relational ( ( "=" | "!=" | ... ) relational )* ;
         * relational     => multiplicative ( ( ">=" | ">" | ... ) multiplicative )* ;
         * multiplicative => unary ( ( "/" | "*" ) unary )* ;
         * exponential    => unary ( "**" unary )* ;

         * unary          => ( "-" | "not" ) unary
         *                 | primary ;
         *
         * primary        => NUMBER
         *                  | STRING
         *                  | "true"
         *                  | "false"
         *                  | "[" anything "]"
         *                  | function
         *                  | "(" expression ")" ;
         *
         * function       => Identifier "(" arguments ")"
         * arguments      => expression ( "," expression )*
         */
        // The Deferred helper creates a parser that can be referenced by others before it is defined
        var expression = Deferred<LogicalExpression>();

        var intParser = Terms.Integer(NumberOptions.AllowSign).Then<LogicalExpression>(static d => new ValueExpression(d));

        var intExponentParser = Terms.Integer(NumberOptions.AllowSign)
            .And(Terms.Text("e", true))
            .And(Terms.Integer(NumberOptions.AllowSign))
            .Then<LogicalExpression>((context, n) =>
            {
                var useDecimalAsDefault = ((LogicalExpressionParserContext)context).UseDecimalsAsDefault;

                if (useDecimalAsDefault)
                {
                    var decimalValue = Convert.ToDecimal(n.Item1 + n.Item2 + n.Item3);
                    return new ValueExpression(decimalValue);
                }

                var doubleValue = Convert.ToDouble(n.Item1 + n.Item2 + n.Item3);
                return new ValueExpression(doubleValue);
            });

        var decimalPointParser = ZeroOrOne(Terms.Integer(NumberOptions.AllowSign))
            .AndSkip(Terms.Char('.'))
            .And(ZeroOrOne(Terms.Integer()))
            .AndSkip(ZeroOrOne(Terms.Text("e", true)))
            .And(ZeroOrOne(Terms.Integer(NumberOptions.AllowSign)))
            .Then<LogicalExpression>((context, x) =>
            {
                var useDecimalAsDefault = ((LogicalExpressionParserContext)context).UseDecimalsAsDefault;

                if (useDecimalAsDefault)
                {
                    decimal decimalValue;
                    if (x.Item3 != 0)
                        decimalValue = Convert.ToDecimal(x.Item1 + "." + x.Item2 + "e" + x.Item3,
                            CultureInfo.InvariantCulture);
                    else
                        decimalValue = Convert.ToDecimal(x.Item1 + "." + x.Item2, CultureInfo.InvariantCulture);

                    return new ValueExpression(decimalValue);
                }

                double doubleValue;
                if (x.Item3 != 0)
                    doubleValue = Convert.ToDouble(x.Item1 + "." + x.Item2 + "e" + x.Item3,
                        CultureInfo.InvariantCulture);
                else
                    doubleValue = Convert.ToDouble(x.Item1 + "." + x.Item2, CultureInfo.InvariantCulture);

                return new ValueExpression(doubleValue);
            });

        var number = OneOf(
            decimalPointParser,
            intExponentParser,
            intParser
        );

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
        var questionMark = Terms.Char('?');
        var colon = Terms.Char(':');
        var negate = Terms.Text("!");
        var not = Terms.Text("not", caseInsensitive: true);

        // "(" expression ")"
        var groupExpression = Between(openParen, expression, closeParen);

        // "[" identifier "]"
        var identifierExpression = openBrace
            .SkipAnd(AnyCharBefore(closeBrace, consumeDelimiter: true))
            .Or(Terms.Identifier())
            .Then<LogicalExpression>(x => new Identifier(x.ToString()));

        var arguments = Separated(comma, expression);

        var functionWithArguments = Terms
            .Identifier()
            .And(openParen.SkipAnd(arguments).AndSkip(closeParen))
            .Then<LogicalExpression>(x => new Function(new Identifier(x.Item1.ToString()), x.Item2.ToArray()));

        var functionWithoutArguments = Terms
            .Identifier()
            .And(openParen.AndSkip(closeParen))
            .Then<LogicalExpression>(x => new Function(new Identifier(x.Item1.ToString()), []));

        var function = OneOf(functionWithArguments, functionWithoutArguments);

        var booleanTrue = Terms.Text("true", caseInsensitive: true).Then<LogicalExpression>(_ => True);
        var booleanFalse = Terms.Text("false", caseInsensitive: true).Then<LogicalExpression>(_ => False);
        var stringValue = Terms.String(quotes: StringLiteralQuotes.SingleOrDouble)
            .Then<LogicalExpression>(x => new ValueExpression(x.ToString()));

        var charIsNumber = Terms.Pattern(char.IsNumber);

        var dateTimeParser = Terms
            .Char('#')
            .SkipAnd(charIsNumber)
            .AndSkip(divided)
            .And(charIsNumber)
            .AndSkip(divided)
            .And(charIsNumber)
            .AndSkip(Terms.Char('#'))
            .Then<LogicalExpression>(date =>
            {
                if (DateTime.TryParse($"{date.Item1}/{date.Item2}/{date.Item3}", out var result))
                {
                    return new ValueExpression(result);
                }

                throw new NCalcParserException("Invalid date format.");
            });

        // primary => NUMBER | "[" identifier "]" | function | boolean | "(" expression ")";
        var primary = number
            .Or(booleanTrue)
            .Or(booleanFalse)
            .Or(dateTimeParser)
            .Or(stringValue)
            .Or(function)
            .Or(groupExpression)
            .Or(identifierExpression);

        // exponential => primary ( "**" primary )* ;
        var exponential = primary.And(ZeroOrMany(exponent.And(primary)))
            .Then(static x =>
            {
                LogicalExpression result = null;

                if (x.Item2.Count == 0)
                    result = x.Item1;
                else if (x.Item2.Count == 1)
                    result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item1, x.Item2[0].Item2);
                else
                {
                    for (int i = x.Item2.Count - 1; i > 0; i--)
                        result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item2[i - 1].Item2, x.Item2[i].Item2);

                    result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item1, result);
                }

                return result;
            }).Or(primary);

        // The Recursive helper allows to create parsers that depend on themselves.
        // ( "-" | "not" ) unary | primary;
        var unary = Recursive<LogicalExpression>(u =>
            minus.Or(not).Or(negate).Or(plus).Or(bitwiseNot).And(u)
                .Then<LogicalExpression>(static x =>
                {
                    return x.Item1.ToUpperInvariant() switch
                    {
                        "!" => new UnaryExpression(UnaryExpressionType.Negate, x.Item2),
                        "-" => new UnaryExpression(UnaryExpressionType.Negate, x.Item2),
                        "+" => new UnaryExpression(UnaryExpressionType.Positive, x.Item2),
                        "NOT" => new UnaryExpression(UnaryExpressionType.Not, x.Item2),
                        "~" => new UnaryExpression(UnaryExpressionType.BitwiseNot, x.Item2),
                        _ => throw new NotSupportedException()
                    };
                })
                .Or(exponential));

        // multiplicative => exponential ( ( "/" | "*" | "%" ) exponential )* ;
        var multiplicative = unary.And(ZeroOrMany(divided.Or(times).Or(modulo).And(unary)))
            .Then(static x =>
            {
                var result = x.Item1;
                foreach (var op in x.Item2)
                {
                    result = op.Item1 switch
                    {
                        '/' => new BinaryExpression(BinaryExpressionType.Div, result, op.Item2),
                        '*' => new BinaryExpression(BinaryExpressionType.Times, result, op.Item2),
                        '%' => new BinaryExpression(BinaryExpressionType.Modulo, result, op.Item2),
                        _ => null
                    };
                }

                return result;
            }).Or(unary);

        // expression => ternary ( ( "-" | "+" ) ternary )* ;
        var additive = multiplicative.And(ZeroOrMany(plus.Or(minus).And(multiplicative)))
            .Then(static x =>
            {
                var result = x.Item1;
                foreach (var op in x.Item2)
                {
                    result = op.Item1 switch
                    {
                        "+" => new BinaryExpression(BinaryExpressionType.Plus, result, op.Item2),
                        "-" => new BinaryExpression(BinaryExpressionType.Minus, result, op.Item2),
                        _ => null
                    };
                }

                return result;
            })
            .Or(multiplicative);
            

        var relational = additive.And(ZeroOrMany(OneOf(
                    Terms.Text(">="),
                    Terms.Text("<="),
                    Terms.Text("<"),
                    Terms.Text(">"))
                .And(additive)))
            .Then(static x =>
            {
                // unary
                var result = x.Item1;
                // (("/" | "*") unary ) *
                foreach (var op in x.Item2)
                {
                    result = op.Item1 switch
                    {
                        "<" => new BinaryExpression(BinaryExpressionType.Lesser, result, op.Item2),
                        ">" => new BinaryExpression(BinaryExpressionType.Greater, result, op.Item2),
                        "<=" => new BinaryExpression(BinaryExpressionType.LesserOrEqual, result, op.Item2),
                        ">=" => new BinaryExpression(BinaryExpressionType.GreaterOrEqual, result, op.Item2),
                        _ => null
                    };
                }

                return result;
            });

        var equality = relational.And(ZeroOrMany(OneOf(
                    Terms.Text("<>"),
                    Terms.Text("=="),
                    Terms.Text("!="),
                    Terms.Text("="))
                .And(relational)))
            .Then(static x =>
            {
                // unary
                var result = x.Item1;
                // (("/" | "*") unary ) *
                foreach (var op in x.Item2)
                {
                    result = op.Item1 switch
                    {
                        "<>" => new BinaryExpression(BinaryExpressionType.NotEqual, result, op.Item2),
                        "==" => new BinaryExpression(BinaryExpressionType.Equal, result, op.Item2),
                        "!=" => new BinaryExpression(BinaryExpressionType.NotEqual, result, op.Item2),
                        "=" => new BinaryExpression(BinaryExpressionType.Equal, result, op.Item2),
                        _ => null
                    };
                }

                return result;
            });


        var and = Terms
            .Text("and", caseInsensitive: true)
            .Or(Terms.Text("&&"))
            .Or(Terms.Text("&"));

        var or = Terms
            .Text("or", caseInsensitive: true)
            .Or(Terms.Text("||"))
            .Or(Terms.Text("|"));

        var xor = Terms.Text("^");

        var logical = equality.And(
                ZeroOrMany(OneOf(and, or, xor)
                    .And(equality)))
            .Then(static x =>
            {
                var result = x.Item1;
                foreach (var op in x.Item2)
                {
                    result = op.Item1.ToUpperInvariant() switch
                    {
                        "AND" => new BinaryExpression(BinaryExpressionType.And, result, op.Item2),
                        "&&" => new BinaryExpression(BinaryExpressionType.And, result, op.Item2),
                        "&" => new BinaryExpression(BinaryExpressionType.BitwiseAnd, result, op.Item2),
                        "OR" => new BinaryExpression(BinaryExpressionType.Or, result, op.Item2),
                        "||" => new BinaryExpression(BinaryExpressionType.Or, result, op.Item2),
                        "|" => new BinaryExpression(BinaryExpressionType.BitwiseOr, result, op.Item2),
                        "^" => new BinaryExpression(BinaryExpressionType.BitwiseXOr, result, op.Item2),
                        _ => null
                    };
                }

                return result;
            });

        var ternary = logical.And(ZeroOrOne(questionMark.SkipAnd(logical).AndSkip(colon).And(logical)))
            .Then(x => x.Item2.Item1 == null
                ? x.Item1
                : new TernaryExpression(x.Item1, x.Item2.Item1, x.Item2.Item2));

        expression.Parser = ternary;
        Parser = expression;
    }

    public static LogicalExpression Parse(LogicalExpressionParserContext context) => Parser.Parse(context);
}