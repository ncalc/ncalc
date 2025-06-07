# Operators

Expressions can be combined using operators, each with a specific precedence priority. The precedence rules determine the order in which operations are performed in an expression. Below is a list of operator precedence in descending order:

1. **Primary**
2. **Unary**
3. **Percent**
4. **Exponential**
5. **Multiplicative**
6. **Additive**
7. **Relational**
8. **Logical**

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

* `!` : Logical NOT
* `not` : Logical NOT
* `-` : Negation
* `~` : Bitwise NOT

**Examples:**
```csharp
not true
!(1 != 2)
```

## Percent

Percent is a post-operator that alters the way the operand is interpreted. A percent value is marked with a `%` character placed after a numeric value or expression.

Percent calculations must be enabled via the [AdvancedOptions](advanced_value_formats.md) property of the ExpressionBase class. When percent calculations are enabled, the `%` character is used for them, and `mod` is used for modulo division.

## Exponential

Exponential operators perform exponentiation.

* `**` : Exponentiation

**Example:**
```csharp
2 ** 2
```

## Multiplicative

Multiplicative operators perform multiplication, division, and modulus operations.

* `*` : Multiplication
* `/` : Division
* `%` : Modulus (when percent calculation is disabled)
* `mod` : Modulus (when percent calculation is enabled in AdvancedOptions)

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

### IN and NOT IN

The `IN` and `NOT IN` operators check whether a value is present or absent within a specified collection or string.

* `IN` : Returns `true` if the left operand is found in the right operand (which can be a collection or string).
* `NOT IN` : Returns `true` if the left operand is not found in the right operand.

The right operand must be either a string or a collection (`IEnumerable`).

**Examples:**
```csharp
'Insert' IN ('Insert', 'Update')          // True
42 NOT IN (1, 2, 3)                       // True
'Sergio' IN 'Sergio is at Argentina'      // True
'Mozart' NOT IN ('Chopin', 'Beethoven')   // True
945 IN (202, 303, 945)                    // True
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

**Examples:**
```csharp
true or false and true    // Evaluates to true
(1 == 1) || false        // Evaluates to true
```

*Note:* The `and` operator has higher priority than the `or` operator. Hence, in the example above, `false and true` is evaluated first.

## Bitwise

Bitwise operators perform bitwise operations on integers.

* `&` : Bitwise AND
* `|` : Bitwise OR
* `^` : Bitwise XOR
* `<<` : Left shift
* `>>` : Right shift

**Example:**
```csharp
2 >> 3
```

