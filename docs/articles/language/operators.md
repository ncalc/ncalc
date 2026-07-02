# Operators

Expressions can be combined using operators, each with a specific precedence priority. The precedence rules determine the order in which operations are performed in an expression. Below is a list of operator precedence in descending order:

1. **Primary**
2. **Unary**
3. **Exponential**
4. **Multiplicative**
5. **Additive**
6. **Relational**
7. **Logical**

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

### Prefix Operators
* `!` : Logical NOT
* `not` : Logical NOT
* `-` : Negation
* `~` : Bitwise NOT

### Suffix operators
* `!`  : Factorial

**Examples:**
```csharp
not true // Evaluates to false
!(1 != 2) // Evaluates to false
3!        // Evaluates to 6 (3*2*1)
```

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
* `%` : Modulus

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

Use a backslash (`\`) to escape wildcard characters when they should be matched literally:
* `\%` matches a literal percent sign.
* `\_` matches a literal underscore.
* `\\` matches a literal backslash.

Use `EscapeLike(value)` when passing user input into a `LIKE` pattern and the input should be matched literally.

**Examples:**
```csharp
'HelloWorld' LIKE 'Hello%'     // True
'Test123' NOT LIKE 'Test__'    // False
'2024-08-28' LIKE '2024-08-__' // True
'abc' LIKE 'a%'                // True
'100%' LIKE '%\%'              // True
'1000' LIKE '%\%'              // False
'Hello_world' LIKE '%\_%'      // True
'Hello-world' LIKE '%\_%'      // False
'100%' LIKE EscapeLike('100%') // True
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

## Using Binary Evaluation Handlers

You can intercept binary operators before NCalc applies its built-in evaluation rules by subscribing to
<xref:NCalc.Expression.EvaluateBinary>. This event uses the <xref:NCalc.Handlers.EvaluateBinaryHandler> delegate and
receives a <xref:NCalc.Handlers.BinaryEventArgs> instance.

`BinaryEventArgs` exposes:

* `BinaryExpression` to inspect the operator kind.
* `LeftValue()` and `RightValue()` to lazily evaluate operands only when needed.
* `Result` to provide a custom value and stop the default operator evaluation.

```csharp
var expression = new Expression("((1 + 2 + 3) * 2) * 2");

expression.EvaluateBinary += args =>
{
    if (args.BinaryExpression.Type == BinaryExpressionType.Plus)
    {
        args.Result = (int)args.LeftValue()! + (int)args.RightValue()! + 1;
    }

    if (args.BinaryExpression.Type == BinaryExpressionType.Times)
    {
        args.Result = string.Concat(
            Enumerable.Repeat(args.LeftValue()!.ToString(), (int)args.RightValue()!));
    }
};

var result = expression.Evaluate();
// "8888"
```

If you do not assign `args.Result`, NCalc continues with the built-in implementation for that operator.
