# Values

A value is a terminal token representing a concrete element. This can be:

- An <xref:System.Int32>
- Any floating point number, like <xref:System.Double>
- A <xref:System.DateTime> or <xref:System.TimeSpan>
- A <xref:System.Boolean>
- A <xref:System.String>
- A <xref:System.Char>
- A <xref:NCalc.Function>
- An <xref:NCalc.Identifier> (parameter)
- A <xref:NCalc.LogicalExpressionList>  (List of other expressions)

## Integers

They are represented using numbers. 

```
123456
```

They are evaluated as <xref:System.Int32>. If the value is too big, it will be evaluated as <xref:System.Int64>.

## Arbitrary-precision integers

Select `IntegerNumberType.BigInteger` explicitly to parse whole-number literals
without narrowing them to `Int64` or floating point. Configure math coercion as
well when integer parameters or numeric strings should use the same mode:

```csharp
using System.Numerics;
using NCalc;
using NCalc.Helpers;

var configuration = new ExpressionConfiguration
{
    Parsing = new LogicalExpressionParserOptions
    {
        IntegerNumberType = IntegerNumberType.BigInteger
    },
    Evaluation = new ExpressionEvaluationOptions
    {
        Math = new MathOptions
        {
            IntegerNumberType = IntegerNumberType.BigInteger
        }
    }
};
var expression = new Expression(
    "99999999999999999999999999*888888888888888888888888888888888888888",
    configuration);
var result = (BigInteger)expression.Evaluate()!;
```

Decimal whole-number literals, and `0x`, `0o` and `0b` literals, are arbitrary
precision in this mode. Signs use the existing unary operators. A bare
32-digit decimal sequence is an integer in this mode; hexadecimal GUIDs and
hyphenated GUIDs retain their normal syntax. Decimal fractions and scientific
notation still use `FloatingPointNumberType`.

`+`, `-`, `*`, `/`, `%`, unary negation and comparisons preserve integral values.
Fixed-width integer operands promote exactly when combined with a `BigInteger`;
setting the math option also promotes fixed-width integer parameters for `Abs`
and comparisons as well as arithmetic. Strict type matching is checked before
comparison promotion. Integer `/` truncates toward zero and `%` follows
`BigInteger` remainder semantics.
Division by zero still throws. Supplied `BigInteger` parameters are supported
without changing how other parameter types are evaluated. `Abs`, `Sign`, `Min`
and `Max` support integral `BigInteger` operands.

There is no implicit conversion between `BigInteger` and `float`, `double` or
`decimal` during arithmetic or comparison. Convert parameters to the intended
real type explicitly if a real-valued calculation is needed; use real literals
(such as `1.0`) or default integer parsing for the rest of that calculation. Trigonometric,
logarithmic, exponential and rounding functions do not provide arbitrary
precision; they reject `BigInteger` operands. `Pow` and bitwise/shift operations
do not accept `BigInteger` operands. Lambda compilation does not support this
configuration. Use `Evaluate` or `EvaluateAsync`
for supported operations. The existing factorial limit and result type are
unchanged.
Default lambda compilation still permits existing non-arithmetic pass-through
of `BigInteger` parameters, context properties and context method results.

Text serialization emits invariant integer digits; parse that text with the
same integer option. JSON AST serialization records `ValueType.BigInteger` and
an exact integer token, retaining the type even for small values.
Legacy AST values retain the caller's registered JSON object conversions,
including null callbacks when the converter enables `HandleNull`;
`BigInteger` values use their exact numeric token instead.
Opt-in parsed ASTs bypass the shared text-only cache, even when `CacheEnabled` is true, so
parser options and custom cultures cannot reuse an incompatible tree. Default
`Int32`/`Int64` selection, overflow wrapping/protection, real arithmetic and
Boolean/string options are unchanged; this mode does not automatically promote
an overflowing default calculation.

## Floating point numbers

Use the dot to define the decimal part. 

```
123.456
.123
```
They are evaluated as <xref:System.Double> by default. Use
<xref:NCalc.Parser.LogicalExpressionParserOptions.FloatingPointNumberType> to change the parsed floating point number type.

```csharp
var configuration = new ExpressionConfiguration
{
    Parsing = new LogicalExpressionParserOptions
    {
        FloatingPointNumberType = FloatingPointNumberType.Decimal
    },
    Evaluation = new ExpressionEvaluationOptions
    {
        Math = new MathOptions
        {
            FloatingPointNumberType = FloatingPointNumberType.Decimal
        }
    }
};
```

## Scientific notation

You can use the e to define power of ten (10^).
```
1.22e1
1e2
1e+2
1e+2
1e-2
.1e-2
1e10
```
They are evaluated as <xref:System.Double> by default. Use
<xref:NCalc.Parser.LogicalExpressionParserOptions.FloatingPointNumberType> to change the parsed floating point number type.

## DateTime

Must be enclosed between sharps. 

```
#2008/01/31# // for en-US culture
#08/08/2001 09:30:00# 
```
NCalc uses the current Culture to evaluate them.

## TimeSpan

Must be enclosed between sharps.
```
#20:42:00#
```

## Booleans
Booleans can be either `true` or `false`, matched case-insensitively as keywords.
Names such as `trueValue` and `falsePositive` are identifiers, not boolean prefixes.

```
true
```
## Strings

Any character between single or double quotes are evaluated as <xref:System.String>. 

```
'hello'
```

```
greeting("Chers")
```
You can escape special characters using \\, \', \n, \r, \t.

## Chars
If you set <xref:NCalc.Parser.LogicalExpressionParserOptions.AllowCharValues>, single quoted strings are interpreted as
<xref:System.Char>.

```csharp
var configuration = new ExpressionConfiguration
{
    Parsing = new LogicalExpressionParserOptions
    {
        AllowCharValues = true
    }
};

var expression = new Expression("'g'", configuration);
var result = expression.Evaluate();
Debug.Assert(result is 'g');
```

## Guid
NCalc also supports <xref:System.Guid>, they can be parsed with or without hyphens.
```csharp
b1548bd5-2556-4d2a-9f47-bb8d421026dd
getUser(78b1941f4e7941c9bef656fad7326538)
```

## Function

A function is made of a name followed by braces, containing optionally any value as arguments.

```
Abs(1)
```

```
doSomething(1, 'dummy')
```

Please read the [functions page](functions.md) for details.

## Parameters

A parameter as a name, and can be optionally contained inside brackets or double quotes.

```
2 + x, 2 + [x]
```

Please read the [parameters page](parameters.md) for details.

## Lists

Lists are collections of expressions enclosed in parentheses. They are the equivalent of `List<LogicalExpression>` at CLR.
```
('Chers', secretOperation(), 3.14)
```
