# Operators

Expressions can be combined using operators, each with a specific precedence priority. The precedence rules determine the order in which operations are performed in an expression. Below is a list of operator precedence in descending order:

1. **Primary**
2. **Unary**
3. **Factorial**
4. **Percent**
5. **Exponential**
6. **Multiplicative**
7. **Additive**
8. **Relational**
9. **Logical**

## Primary

Primary are the first thing to be evaluated. They are direct values or a list of them.

### Values

Values include literals that are used in expressions.

**Examples:**
```csharp
42
"hello"
true
```

### Lists

Lists are used to group expressions.

**Examples:**
```csharp
2 * (3 + 2)
"foo" in ("foo", "bar", 5) 
secret_operation("my_db", 2) // Function arguments are actually a list!
```

## Unary

Unary operators operate on a single operand.

* `!` : Logical NOT  (unless the <xref:NCalc.ExpressionOptions.SkipLogicalAndBitwiseOpChars> flag is set in  <xref:NCalc.ExpressionOptions>)
* `not` : Logical NOT
* `-` : Negation
* `~` : Bitwise NOT (unless the <xref:NCalc.ExpressionOptions.SkipLogicalAndBitwiseOpChars> flag is set in  <xref:NCalc.ExpressionOptions>)
* `bit_not` : Bitwise NOT

**Examples:**
```csharp
not true
!(1 != 2)
```

When Unicode Characters are enabled for operations using the <xref:NCalc.ExpressionOptions.UseUnicodeCharsForOperations> flag in <xref:NCalc.ExpressionOptions>, the following operations are also supported:

* `¬` (U+00AC) : Logical NOT
* `√` (U+221A) : Square root
* `∛` (U+221B) : Cube root
* `∜` (U+221C) : Fourth root

**Examples:**
```csharp
√4
∜(4*4)
```

## Factorial

Factorial ('!') is a special post-operator that is applicable to a value or expression that evaluates to an integer value. The resulting expression is binary, and the number of exclamation marks determines the type of factorial (standard, double, triple etc.). 

* `!`: standard factorial
* `!!`: double factorial
* `!!!`: triple factorial
* `!....!`: some multifactorial

**Example**:
```
(1+2)!
20!!!
```

## Percent

Percent is a post-operator that alters the way the operand is interpreted. A percent value is marked with a `%` character placed after a numeric value or expression.

Percent calculations must be enabled via the [AdvancedOptions](advanced_value_formats.md) property of the ExpressionBase class. When percent calculations are enabled, the `%` character is used for them, and `mod` is used for modulo division.

**Example**:
```
(1+2)%
5% + 2%
100 * 5%
200 + 10%
200 - 5%
```

## Exponential

Exponential operators perform exponentiation.

* `**` : Exponentiation  (default, when the <xref:NCalc.ExpressionOptions.SkipLogicalAndBitwiseOpChars> flag is not set in  <xref:NCalc.ExpressionOptions>)
* `^` : Exponentiation  (when the <xref:NCalc.ExpressionOptions.SkipLogicalAndBitwiseOpChars> flag is set in  <xref:NCalc.ExpressionOptions>)

**Example:**
```csharp
2 ** 2
```
When Unicode Characters are enabled for operations using the <xref:NCalc.ExpressionOptions.UseUnicodeCharsForOperations> flag in <xref:NCalc.ExpressionOptions>, the following operations are also supported:
* `↑` (U+2291): Exponentiation

## Multiplicative

Multiplicative operators perform multiplication, division, and modulus operations.

* `*` : Multiplication
* `/` : Division
* `%` : Modulus (when percent calculation is disabled)
* `mod` : Modulus (when percent calculation is enabled in <xref:NCalc.AdvancedExpressionOptions>)

When Unicode Characters are enabled for operations using the <xref:NCalc.ExpressionOptions.UseUnicodeCharsForOperations> flag in <xref:NCalc.ExpressionOptions>, the following operations are also supported:
* `×` (U+00D7) : Multiplication
* `∙` (U+2219): Multiplication
* `:` : Division
* `÷` (U+00F7) : Division

When assignments are enabled using the <xref:NCalc.ExpressionOptions.UseAssignments> flag in <xref:NCalc.ExpressionOptions>, the following operations are also supported:
* `*=` : Multiplication with assignment of the result to the left operand
* `/=` : Division with assignment of the result to the left operand

When unicode characters are enabled, they can also be used for multiplication with assignment and division with assignment.

**Example:**
```csharp
1 * 2 % 3
```

## Additive

Additive operators perform addition and subtraction.

* `+` : Addition
* `-` : Subtraction

**Example:**
```csharp
1 + 2 - 3
```

When [Advanced Value Formats and Operations](advanced_value_formats.md) are used and time operations are enabled, it is possible to add a time value (Timespan) to a date or another time value. Also, it is possible to subtract a time value from another time value or a time value from a date or a date from a date.

When assignments are enabled using the <xref:NCalc.ExpressionOptions.UseAssignments> flag in <xref:NCalc.ExpressionOptions>, the following operations are also supported:
* `+=` : Addition with assignment of the result to the left operand
* `-=` : Subtraction with assignment of the result to the left operand

## Relational

Relational operators compare two values and return a boolean result.

### Equality and Inequality Operators

These operators compare two values to check equality or inequality.

* `=`, `==` : Equal to
* `!=`, `<>` : Not equal to

**Examples:**
```csharp
42 == 42            // true
"hello" == "world"  // false
10 != 5             // true
"apple" != "apple"  // false
```

When Unicode Characters are enabled for operations using the <xref:NCalc.ExpressionOptions.UseUnicodeCharsForOperations> flag in <xref:NCalc.ExpressionOptions>, the following operations are also supported:
* `≠` (U+2260) : Not equal to

### Comparison Operators

These operators compare two values to determine their relative order.

* `<`  : Less than
* `<=` : Less than or equal to
* `>`  : Greater than
* `>=` : Greater than or equal to

**Examples:**
```csharp
3 < 5          // true
10 <= 10       // true
7 > 3          // true
8 >= 12        // false
```

When Unicode Characters are enabled for operations using the <xref:NCalc.ExpressionOptions.UseUnicodeCharsForOperations> flag in <xref:NCalc.ExpressionOptions>, the following operations are also supported:
* `≤` (U+2264) : Less than or equal to
* `≥` (U+2265) : Greater than or equal to

### IN and NOT IN

The `IN` and `NOT IN` operators check whether a value is present or absent within a specified collection or string.

* `IN` : Returns `true` if the left operand is found in the right operand (which can be a collection or string).
* `NOT IN` : Returns `true` if the left operand is not found in the right operand.

When Unicode Characters are enabled for operations using the <xref:NCalc.ExpressionOptions.UseUnicodeCharsForOperations> flag in <xref:NCalc.ExpressionOptions>, the following operations are also supported:
* `∈` (U+2208) : Returns `true` if the left operand is found in the right operand (which can be a collection or string). 
* `∉` (U+2209) : Returns `true` if the left operand is not found in the right operand.

The right operand must be either a string or a collection (`IEnumerable`).

**Examples:**
```csharp
'Insert' IN ('Insert', 'Update')          // True
42 NOT IN (1, 2, 3)                       // True
'Sergio' IN 'Sergio is at Argentina'      // True
'Mozart' NOT IN ('Chopin', 'Beethoven')   // True
945 IN (202, 303, 945)                    // True
945 ∈ (202, 303, 945)                     // True
```

### LIKE and NOT LIKE

The `LIKE` and `NOT LIKE` operators compare a string against a pattern.

* `LIKE` : Checks if the string matches the specified pattern.
* `NOT LIKE` : Checks if the string does not match the specified pattern.

Patterns can include:
* `%` to match any sequence of characters.
* `_` to match any single character.

**Examples:**
```csharp
'HelloWorld' LIKE 'Hello%'     // True
'Test123' NOT LIKE 'Test__'    // False
'2024-08-28' LIKE '2024-08-__' // True
'abc' LIKE 'a%'                // True
```

## Logical

Logical operators perform logical comparisons between expressions.

* `or`, `||` : Logical OR
* `and`, `&&` : Logical AND
* `xor` : Logical XOR
**Examples:**
```csharp
true or false and true    // Evaluates to true
(1 == 1) || false        // Evaluates to true
```

*Note:* The `and` operator has higher priority than the `or` or `xor` operator. Hence, in the example above, `false and true` is evaluated first.

When Unicode Characters are enabled for operations using the <xref:NCalc.ExpressionOptions.UseUnicodeCharsForOperations> flag in <xref:NCalc.ExpressionOptions>, the following operators are also supported:
* `∨` (U+2228) : Logical OR
* `∧` (U+2229) : Logical AND
* `⊕` (U+2295) : Logical XOR
* `⊻` (U+22BB) : Logical XOR

## Bitwise

Bitwise operators perform bitwise operations on integers.

* `<<` : Left shift
* `>>` : Right shift

By default, when the <xref:NCalc.ExpressionOptions.SkipLogicalAndBitwiseOpChars> flag is not set in <xref:NCalc.ExpressionOptions>, the following operator symbols are used:

* `|` : Bitwise OR
* `&` : Bitwise AND
* `^` : Bitwise XOR

**Example:**
```csharp
2 >> 3
```

When the <xref:NCalc.ExpressionOptions.SkipLogicalAndBitwiseOpChars> flag is set in <xref:NCalc.ExpressionOptions>, the following operators are used:
* `BIT_OR` : Bitwise OR
* `BIT_AND` : Bitwise AND
* `BIT_XOR` : Bitwise XOR

When assignments are enabled using the <xref:NCalc.ExpressionOptions.UseAssignments> flag in <xref:NCalc.ExpressionOptions>, the following operations are also supported (regardless of the <xref:NCalc.ExpressionOptions.SkipLogicalAndBitwiseOpChars> flag):
* `&=` : Bitwise AND with assignment of the result to the left operand
* `|=` : Bitwise OR with assignment of the result to the left operand
* `^=` : Bitwise XOR with assignment of the result to the left operand 
