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
* in, not in

```
 3 < 2 
 42 == 42 
 'Insert' in ('Insert', 'Update') 
 "Sergio" in "Sergio is at Argentina" 
 "Mozard" not in ("Chopin", "Beethoven", GetComposer())
 945 != 202
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

## Exponential

* **

```
  2 ** 2
```


## Primary

* (, )
* values

```
  2 * ( 3 + 2 )
```