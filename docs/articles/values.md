# Values

A value is a terminal token representing a concrete element. This can be:

- An <xref:System.Int32>
- Any floating point number, like <xref:System.Double>
- A <xref:System.DateTime> or <xref:System.TimeSpan>
- A <xref:System.Boolean>
- A <xref:System.String>
- A <xref:System.Char>
- A <xref:NCalc.Domain.Function>
- An <xref:NCalc.Domain.Identifier> (parameter)
- A <xref:NCalc.Domain.LogicalExpressionList>  (List of other expressions)

## Integers

They are represented using numbers. 

```
123456
```

They are evaluated as <xref:System.Int32>. If the value is too big, it will be evaluated as <xref:System.Int64>.

## Floating point numbers

Use the dot to define the decimal part. 

```
123.456
.123
```
They are evaluated as <xref:System.Double>, unless you use <xref:NCalc.ExpressionOptions.DecimalAsDefault>.

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
They are evaluated as <xref:System.Double>, unless you use <xref:NCalc.ExpressionOptions.DecimalAsDefault>.

## DateTime

Must be enclosed between sharps. 

```
#2008/01/31# // for en-US culture
#08/08/2001 09:30:00# 
```
By default, NCalc uses current Culture to evaluate DateTime values. When [Advanced Value Formats and Operations](advanced_value_formats.md) are used, the format of date and time values can be customized to a large extent.

## Time

Includes Hours, minutes, and seconds. 
The value must be enclosed between sharps.
```
#20:42:00#
```

When [Advanced Value Formats and Operations](advanced_value_formats.md) are used, the format of time values can be customized to a large extent.

Additionally, it is possible to define periods in a humane form (e.g. #5 weeks 3 days 28 hours#) as described in the [Advanced Value Formats and Operations](advanced_value_formats.md) topic.

## Booleans
Booleans can be either `true` or `false`.

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
If you use <xref:NCalc.ExpressionOptions.AllowCharValues>, single quoted strings are interpreted as <xref:System.Char>
```
var expression = new Expression("'g'", ExpressionOptions.AllowCharValues);
var result = expression.Evalutate();
Debug.Assert(result); // 'g' -> System.Char
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

A parameter is a name that can be optionally contained inside brackets or double quotes.

```
2 + x, 2 + [x]
```

Please read the [parameters page](parameters.md) for details.

## Lists

Lists are collections of expressions enclosed in parentheses. They are the equivalent of `List<LogicalExpression>` at CLR.
```
('Chers', secretOperation(), 3.14)
```