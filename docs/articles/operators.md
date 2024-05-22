# Operators

Expressions can be combined using operators. Each operator as a precedence priority. Here is the list of those expression's priority.
1. primary
2. unary
3. power
4. multiplicative
5. additive
6. relational
7. logical

## Logical

These operators can do some logical comparison between other expressions:

* or, ||
* and, &&

```
  true or false and true
```

The **and** operator has more priority than the **or**, thus in the example above, **false and true** is evaluated first.

## Relational

* =, ==, !=, <>
* <, <=, >, >=

```
  1 < 2
```

## Additive

* +, -

```
  1 + 2 - 3
```

## Multiplicative

* *, /, %

```
 1 * 2 % 3
```

## Bitwise

* & (bitwise and), | (bitwise or), ^(bitwise xor), << (left shift), >>(right shift)

```
  2 >> 3
```

## Unary

* !, not, -, ~ (bitwise not)

```
  not true
```

## Primary

* (, )
* values

```
  2 * ( 3 + 2 )
```