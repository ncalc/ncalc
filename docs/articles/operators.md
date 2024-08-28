# Operators

Expressions can be combined using operators, each of which has a precedence priority. Below is the list of expression priorities in descending order:

1. Primary
2. Unary
3. Power
4. Multiplicative
5. Additive
6. Relational
7. Logical

These operators follow the precedence rules to determine the order in which operations are performed in an expression.

## Logical

Logical operators perform logical comparisons between expressions.

* `or`, `||`
* `and`, `&&`

Examples:
```
true or false and true
(1 == 1) || false
```
The `and` operator has higher priority than the `or` operator, thus in the example above, `false and true` is evaluated first.

## Relational

Relational operators compare two values and return a boolean result.

### Equality and Inequality Operators

Equality and inequality operators are used to compare two values, returning a boolean result indicating whether the comparison is true or false.

* `=`, `==` : Checks if two values are equal.
* `!=`, `<>` : Checks if two values are not equal.

Examples:
```csharp
42 == 42         // true, because 42 is equal to 42
"hello" == "world" // false, because "hello" is not equal to "world"
10 != 5          // true, because 10 is not equal to 5
"apple" != "apple" // false, because "apple" is equal to "apple"
```

### Comparison Operators

Comparison operators are used to compare two values to determine their relative order, returning a boolean result.

- `<` : Less than
-`<=` : Less than or equal to
-`>` : Greater than
-`>=` : Greater than or equal to

Examples:
```csharp
3 < 5          // true, because 3 is less than 5
10 <= 10       // true, because 10 is less than or equal to 10
7 > 3          // true, because 7 is greater than 3
8 >= 12        // false, because 8 is not greater than or equal to 12
``` 

### IN and NOT IN

The `IN` and `NOT IN` operators are used to check whether a value exists or does not exist within a specified collection or string.

- `IN` returns `true` if the left operand is found in the right operand (which can be a collection or a string).
- `NOT IN` returns `true` if the left operand is not found in the right operand.

The right operand must be either a string or a collection (`IEnumerable`).

Examples:
```csharp
'Insert' IN ('Insert', 'Update')            // True
42 NOT IN (1, 2, 3)                         // True
'Sergio' IN 'Sergio is at Argentina'        // True
'Mozart' NOT IN ('Chopin', 'Beethoven')     // True
945 IN (202, 303, 945)                      // True
```

### LIKE and NOT LIKE

The `LIKE` and `NOT LIKE` operators are used to compare a string against a pattern. These operators return a boolean result based on whether the string matches or does not match the pattern.

- `LIKE` checks if the string matches the specified pattern.
- `NOT LIKE` checks if the string does not match the specified pattern.

The pattern can include:
- `%` to match any sequence of characters.
- `_` to match any single character.

```csharp
'HelloWorld' LIKE 'Hello%'     // True
'Test123' NOT LIKE 'Test__'    // False
'2024-08-28' LIKE '2024-08-__' // True
'abc' LIKE 'a%'                // True
```

## Additive

Additive operators perform addition and subtraction.

* `+`, `-`

Example:
```
1 + 2 - 3
```

## Multiplicative

Multiplicative operators perform multiplication, division, and modulus operations.

* `*`, `/`, `%`

Example:
```
1 * 2 % 3
```

## Bitwise

Bitwise operators perform bitwise operations on integers.

* `&` (bitwise and), `|` (bitwise or), `^` (bitwise xor), `<<` (left shift), `>>` (right shift)

Example:
```
2 >> 3
```

## Unary

Unary operators operate on a single operand.

* `!`, `not`, `-`, `~` (bitwise not)

Example:
```
not true
```

## Exponential

Exponential operators perform exponentiation.

* `**`

Example:
```
2 ** 2
```

## Primary

Primary operators include grouping of expressions, lists and direct values. Check [Values](values.md) for more info.

* `(`, `)`
* values

Examples:
```
2 * (3 + 2)
("foo","bar", 5)
drop_database()
```
