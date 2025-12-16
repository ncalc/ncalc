using Parlot;
using Parlot.Fluent;
using Parlot.SourceGenerator;

namespace NCalc.Parser
{
    public static partial class NCalcGrammar
    {
        [GenerateParser]
        public static Parser<long> HecOctBinNumberParser()
        {
            var hexNumber = Terms.Text("0x")
                .SkipAnd(Terms.AnyOf("0123456789abcdefABCDEF"))
                .Then(x => Convert.ToInt64(x.ToString(), 16));

            var octalNumber = Terms.Text("0o")
                .SkipAnd(Terms.AnyOf("01234567"))
                .Then(x => Convert.ToInt64(x.ToString(), 8));

            var binaryNumber = Terms.Text("0b")
                .SkipAnd(Terms.AnyOf("01"))
                .Then(x => Convert.ToInt64(x.ToString(), 2));

            return OneOf(hexNumber, octalNumber, binaryNumber);
        }

        public static Parser<string> NonScientificParser() =>
            Not(OneOf(Terms.Text("."), Terms.Text("E", true)));

        [GenerateParser]
        public static Parser<int> IntegerParser() =>
            Terms.Number<int>(NumberOptions.Integer)
                .AndSkip(NonScientificParser());

        [GenerateParser]
        public static Parser<long> LongParser() =>
            Terms.Number<long>(NumberOptions.Integer)
                .AndSkip(NonScientificParser());

        [GenerateParser]
        public static Parser<decimal> DecimalParser() =>
            Terms.Number<decimal>(NumberOptions.Float);

        [GenerateParser]
        public static Parser<double> DoubleParser() =>
            Terms.Number<double>(NumberOptions.Float);

        [GenerateParser]
        public static Parser<string> DividedParser() => Terms.Text("/");

        [GenerateParser]
        public static Parser<string> TimesParser() => Terms.Text("*");

        [GenerateParser]
        public static Parser<string> ModuloParser() => Terms.Text("%");

        [GenerateParser]
        public static Parser<string> MinusParser() => Terms.Text("-");

        [GenerateParser]
        public static Parser<string> PlusParser() => Terms.Text("+");

        [GenerateParser]
        public static Parser<string> EqualParser() =>
            OneOf(Terms.Text("=="), Terms.Text("="));

        [GenerateParser]
        public static Parser<string> NotEqualParser() =>
            OneOf(Terms.Text("<>"), Terms.Text("!="));

        [GenerateParser]
        public static Parser<string> InPrser() => Terms.Text("in", true);

        [GenerateParser]
        public static Parser<string> NotInParser() => Terms.Text("not in", true);

        [GenerateParser]
        public static Parser<string> LikeParser() => Terms.Text("like", true);

        [GenerateParser]
        public static Parser<string> NotLikeParser() => Terms.Text("not like", true);

        [GenerateParser]
        public static Parser<string> GreaterParser() => Terms.Text(">");

        [GenerateParser]
        public static Parser<string> GreaterOrEqualParser() => Terms.Text(">=");

        [GenerateParser]
        public static Parser<string> LesserParser() => Terms.Text("<");

        [GenerateParser]
        public static Parser<string> LesserOrEqualParser() => Terms.Text("<=");

        [GenerateParser]
        public static Parser<string> LeftShiftParser() => Terms.Text("<<");

        [GenerateParser]
        public static Parser<string> RightShiftParser() => Terms.Text(">>");

        [GenerateParser]
        public static Parser<string> ExponentParser() => Terms.Text("**");

        [GenerateParser]
        public static Parser<char> OpenParenParser() => Terms.Char('(');

        [GenerateParser]
        public static Parser<char> CloseParenParser() => Terms.Char(')');

        [GenerateParser]
        public static Parser<char> OpenBraceParser() => Terms.Char('[');

        [GenerateParser]
        public static Parser<char> CloseBraceParser() => Terms.Char(']');

        [GenerateParser]
        public static Parser<char> OpenCurlyBraceParser() => Terms.Char('{');

        [GenerateParser]
        public static Parser<char> CloseCurlyBraceParser() => Terms.Char('}');

        [GenerateParser]
        public static Parser<char> QuestionMarkParser() => Terms.Char('?');

        [GenerateParser]
        public static Parser<char> ColonParser() => Terms.Char(':');

        [GenerateParser]
        public static Parser<char> SharpTermsParser() => Terms.Char('#');

        [GenerateParser]
        public static Parser<char> SharpLiteralsParser() => Literals.Char('#');

        [GenerateParser]
        public static Parser<TextSpan> IdentifierParser() => Terms.Identifier();

        [GenerateParser]
        public static Parser<string> NotParser()
        {
            var openParen = OpenParenParser();

            return OneOf(
                Terms.Text("NOT", true)
                .AndSkip(OneOf(Literals.WhiteSpace(), Not(AnyCharBefore(openParen)))),
                Terms.Text("!"));
        }

        [GenerateParser]
        public static Parser<string> AndParser() =>
            OneOf(Terms.Text("AND", true), Terms.Text("&&"));

        [GenerateParser]
        public static Parser<string> OrParser() =>
            OneOf(Terms.Text("OR", true), Terms.Text("||"));

        [GenerateParser]
        public static Parser<string> BitwiseAndParser() => Terms.Text("&");

        [GenerateParser]
        public static Parser<string> BitwiseOrParser() => Terms.Text("|");

        [GenerateParser]
        public static Parser<string> BitwiseXOrParser() => Terms.Text("^");

        [GenerateParser]
        public static Parser<string> BitwiseNotParser() => Terms.Text("~");

        [GenerateParser]
        public static Parser<TextSpan> ThirtyTwoHexSequenceParser() =>
            Terms.Pattern(Character.IsHexDigit, 32, 32);

        [GenerateParser]
        public static Parser<(TextSpan, TextSpan, TextSpan, TextSpan, TextSpan)> GuidWithHyphens()
        {
            var minus = MinusParser();

            var eightHexSequence = Terms
                .Pattern(Character.IsHexDigit, 8, 8);

            var fourHexSequence = Terms
                .Pattern(Character.IsHexDigit, 4, 4);

            var twelveHexSequence = Terms
                .Pattern(Character.IsHexDigit, 12, 12);

            return eightHexSequence
                    .AndSkip(minus)
                    .And(fourHexSequence)
                    .AndSkip(minus)
                    .And(fourHexSequence)
                    .AndSkip(minus)
                    .And(fourHexSequence)
                    .AndSkip(minus)
                    .And(twelveHexSequence);
        }

        [GenerateParser]
        public static Parser<string> TrueParser() => Terms.Text("true", true);

        [GenerateParser]
        public static Parser<string> FalseParser() => Terms.Text("false", true);

        [GenerateParser]
        public static Parser<TextSpan> SingleQuoteParser() =>
            Terms.String(quotes: StringLiteralQuotes.Single);

        [GenerateParser]
        public static Parser<TextSpan> DoubleQuoteParser() =>
            Terms.String(quotes: StringLiteralQuotes.Double);

        [GenerateParser]
        public static Parser<TextSpan> CharIsNumberParser() =>
            Literals.Pattern(char.IsNumber);

        [GenerateParser]
        public static Parser<TextSpan> IdentifierExpressionParser()
        {
            var openBrace = OpenBraceParser();
            var closeBrace = CloseBraceParser();

            var braceIdentifier = openBrace
                .SkipAnd(AnyCharBefore(closeBrace, failOnEof: true, consumeDelimiter: true)
                .ElseError("Brace not closed."));

            var identifier = IdentifierParser();

            var openCurlyBrace = OpenCurlyBraceParser();
            var closeCurlyBrace = CloseCurlyBraceParser();

            var curlyBraceIdentifier =
                openCurlyBrace.SkipAnd(AnyCharBefore(closeCurlyBrace, failOnEof: true, consumeDelimiter: true)
                .ElseError("Brace not closed."));

            return OneOf(
                braceIdentifier,
                curlyBraceIdentifier,
                identifier);
        }

        [GenerateParser]
        public static Parser<IReadOnlyList<char>> FactorialParser()
        {
            var exclamation = Terms.Char('!');
            var equalParser = EqualParser();

            return ZeroOrMany(exclamation.AndSkip(Not(equalParser)));
        }

        [GenerateParser]
        public static Parser<char> EmptyListParser()
        {
            var openParen = OpenParenParser();
            var closeParen = CloseParenParser();

            return openParen.AndSkip(closeParen);
        }

        [GenerateParser]
        public static Parser<IReadOnlyList<string>> OperatorParser()
        {
            var divided = DividedParser();
            var times = TimesParser();
            var modulo = ModuloParser();
            var minus = MinusParser();
            var plus = PlusParser();

            var equal = EqualParser();
            var notEqual = NotEqualParser();

            var greater = GreaterParser();
            var greaterOrEqual = GreaterOrEqualParser();
            var lesser = LesserParser();
            var lesserOrEqual = LesserOrEqualParser();

            var leftShift = LeftShiftParser();
            var rightShift = RightShiftParser();

            return OneOrMany(OneOf(
                    divided, times, modulo, plus,
                    minus, leftShift, rightShift, greaterOrEqual,
                    lesserOrEqual, greater, lesser, equal,
                    notEqual));
        }

        [GenerateParser]
        public static Parser<IReadOnlyList<TextSpan>> ZeroOrManyWhiteSpaceParser() =>
            ZeroOrMany(Literals.WhiteSpace(true));
    }
}