# Strict Type Matching

With `StrictTypeMatching`, expressions comparing incompatible types (e.g., `string` and `int`) will return `false`.

## Usage Example

```csharp
var expression = new Expression("'1' == 1", ExpressionOptions.StrictTypeMatching);
Debug.Assert(false, expression.Evaluate()); //Without ExpressionOptions.StrictTypeMatching, this would be true.
```

It is especially useful in expressions like this that would throw an exception:
```csharp
var expression = new Expression("'' == 1", ExpressionOptions.StrictTypeMatching);
Debug.Assert(false, expression.Evaluate()); //Without ExpressionOptions.StrictTypeMatching, this would throw an exception.
```