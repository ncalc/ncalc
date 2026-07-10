# String concatenation

If <xref:NCalc.ExpressionEvaluationOptions.StringConcat> is enabled, the result will be a string concatenation.
Otherwise, it will try to perform an arithmetic addition if possible. When the arithmetic operation fails, if both values are
strings then the result will be a string concatenation.

## Example

With this option enabled, string concatenation will always happen.

```csharp
var configuration = new ExpressionConfiguration
{
    Evaluation = new ExpressionEvaluationOptions
    {
        StringConcat = true
    }
};

var expression = new Expression($"5 + '1'", configuration);
var result = expression.Evaluate();
Debug.Assert(result); // 51
```

## Summary

| Expression   | `StringConcat` Enabled | `StringConcat` Disabled |
|--------------|------------------------|-------------------------|
| `'L' + 'eo'` | `'Leo'`                | `'Leo'`                 |
| `1 + '2'`    | `'12'`                 | `3`                     |
| `'1' + '2'`  | `'12'`                 | `3`                     |
