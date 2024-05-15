#nullable disable

using System;
using System.Globalization;
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

var intParser = Terms.Integer().Then<LogicalExpression>(static d => new ValueExpression(d));

var intExponentParser = Terms.Integer(NumberOptions.AllowSign)
    .And(Terms.Text("e", true))
    .And(Terms.Integer(NumberOptions.AllowSign))
    .Then<LogicalExpression>((context, d) =>
    {
        if (!string.IsNullOrWhiteSpace(d.Item2))
        {
            bool useDecimalAsDefault = ((LogicalExpressionParserContext)context).UseDecimalsAsDefault;

            if (useDecimalAsDefault)
            {
                decimal val = Convert.ToDecimal(d.Item1 + d.Item2 + d.Item3);
                return new ValueExpression(val);
            }
            else
            {
                double val = Convert.ToDouble(d.Item1 + d.Item2 + d.Item3);
                return new ValueExpression(val);
            }
        }

        return new ValueExpression(d.Item1);
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
                        decimalValue = Convert.ToDecimal(x.Item1 + "." + x.Item2 + "e" + x.Item3, CultureInfo.InvariantCulture);
                    else
                        decimalValue = Convert.ToDecimal(x.Item1 + "." + x.Item2, CultureInfo.InvariantCulture);

                    return new ValueExpression(decimalValue);
                }

                double doubleValue;
                if (x.Item3 != 0)
                    doubleValue = Convert.ToDouble(x.Item1 + "." + x.Item2 + "e" + x.Item3, CultureInfo.InvariantCulture);
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
        var openParen = Terms.Char('(');
        var closeParen = Terms.Char(')');
        var openBrace = Terms.Char('[');
        var closeBrace = Terms.Char(']');
        var questionMark = Terms.Char('?');
        var colon = Terms.Char(':');

        // "(" expression ")"
        var groupExpression = Between(openParen, expression, closeParen);

        // "[" identifier "]"
        var identifierExpression = openBrace
            .SkipAnd(AnyCharBefore(closeBrace, consumeDelimiter: true))
            .Or(Terms.Identifier())
            .Then<LogicalExpression>(x => new Identifier(x.ToString()));

        var arguments = Separated(comma, expression);

        var function = Terms
            .Identifier()
            .And(openParen.SkipAnd(arguments).AndSkip(closeParen))
            .Then<LogicalExpression>(x => new Function(new Identifier(x.Item1.ToString()), x.Item2.ToArray()));

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
                if (DateTime.TryParse( $"{date.Item1}/{date.Item2}/{date.Item3}", out var result))
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


        // The Recursive helper allows to create parsers that depend on themselves.
        // ( "-" | "not" ) unary | primary;
        var unary = Recursive<LogicalExpression>(u =>
            minus.Or(Terms.Text("not")).And(u)
                .Then<LogicalExpression>(static x =>
                {
                    return x.Item1 switch
                    {
                        "-" => new UnaryExpression(UnaryExpressionType.Negate, x.Item2),
                        "not" => new UnaryExpression(UnaryExpressionType.Not, x.Item2),
                        _ => throw new NotSupportedException()
                    };
                })
                .Or(primary));

        // factor => unary ( ( "/" | "*" | "%" ) unary )* ;
        var multiplicative = unary.And(ZeroOrMany(divided.Or(times).Or(modulo).And(unary)))
            .Then(static x =>
            {
                // unary
                var result = x.Item1;
                // (("/" | "*") unary ) *
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
            });

        var relational = multiplicative.And(ZeroOrMany(OneOf(
                    Terms.Text(">="),
                    Terms.Text("<="),
                    Terms.Text("<"),
                    Terms.Text(">"))
                .And(multiplicative)))
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

        var ternary = equality.And(ZeroOrOne(questionMark.SkipAnd(equality).AndSkip(colon).And(equality)))
            .Then(x => x.Item2.Item1 == null
                ? x.Item1
                : new TernaryExpression(x.Item1, x.Item2.Item1, x.Item2.Item2));

        // expression => ternary ( ( "-" | "+" ) ternary )* ;
        expression.Parser = ternary.And(ZeroOrMany(plus.Or(minus).And(ternary)))
            .Then(static x =>
            {
                // factor
                var result = x.Item1;
                // (("-" | "+") factor ) *
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
            });

        Parser = expression;
    }

    public static LogicalExpression Parse(LogicalExpressionParserContext context)
    {
        return Parser.Parse(context);
    }
}