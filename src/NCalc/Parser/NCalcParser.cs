using System;
using NCalc.Domain;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;
using Identifier = NCalc.Domain.Identifier;

namespace NCalc.Parser;

public class NCalcParser
{
    private static readonly Parser<LogicalExpression> LogicalExpressionParser;
    private static readonly ValueExpression True = new(true);
    private static readonly ValueExpression False = new(false);

    static NCalcParser()
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

        var number = OneOf(
            Terms.Decimal().Then<LogicalExpression>(static d => new ValueExpression(d)),
            Terms.Double().Then<LogicalExpression>(static d => new ValueExpression(d)),
            Terms.Integer().Then<LogicalExpression>(static d => new ValueExpression(d))
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
        var identifierExpression = openBrace.SkipAnd(AnyCharBefore(closeBrace, consumeDelimiter: true)).Then<LogicalExpression>(x => new Domain.Identifier(x.ToString()));

        var arguments = Separated(comma, expression);

        var function = Terms.Identifier().And(openParen.SkipAnd(arguments).AndSkip(closeParen))
            .Then<LogicalExpression>(x => new Function(new Identifier(x.Item1.ToString()), x.Item2.ToArray()));
        
        var booleanTrue = Terms.Text("true", caseInsensitive: true).Then<LogicalExpression>(x => True);
        var booleanFalse = Terms.Text("false", caseInsensitive: true).Then<LogicalExpression>(x => False);
        var stringValue = Terms.String(quotes: StringLiteralQuotes.Single).Then<LogicalExpression>(x => new ValueExpression(x.ToString()));

        // primary => NUMBER | "[" identifier "]" | function | boolean | "(" expression ")";
        var primary = number.Or(identifierExpression).Or(function).Or(booleanTrue).Or(booleanFalse).Or(stringValue).Or(groupExpression);

        // The Recursive helper allows to create parsers that depend on themselves.
        // ( "-" | "not" ) unary | primary;
        var unary = Recursive<LogicalExpression>((u) =>
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

        LogicalExpressionParser = expression;
    }

    public static LogicalExpression Parse(string expression)
    {
        return LogicalExpressionParser.Parse(expression);
    }
}