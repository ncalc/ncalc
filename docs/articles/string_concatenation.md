# String concatenation

If the <xref:NCalc.ExpressionOptions.StringConcat> option specified the result will be a string concatenation. 
Otherwise, it will try to perform an arithmetic addition if possible. When the arithmetic operation fails, if both values are
strings then the result will be a string concatenation.

## Example

With this option enabled, string concatenation will always happen.

```csharp
var expression = new Expression($"5 + '1'", ExpressionOptions.StringConcat);
var result = expression.Evaluate();
Debug.Assert(result); // 51
```

## Summary

| Expression   | `StringConcat` Enabled | `StringConcat` Disabled |
|--------------|------------------------|-------------------------|
| `'L' + 'eo'` | `'Leo'`                | `'Leo'`                 |
| `1 + '2'`    | `'12'`                 | `3`                     |
| `'1' + '2'`  | `'12'`                 | `3`                     |
