# Strict Type Matching

With `StrictTypeMatching`, expressions comparing incompatible types (e.g., `string` and `int`) will return `false`.

## Usage Example

```csharp
var configuration = new ExpressionConfiguration
{
    Evaluation = new ExpressionEvaluationOptions
    {
        StrictTypeMatching = true
    }
};

var expression = new Expression("'1' == 1", configuration);
Debug.Assert(false, expression.Evaluate()); // Without StrictTypeMatching, this would be true.
```

It is especially useful in expressions like this that would throw an exception:
```csharp
var expression = new Expression("'' == 1", configuration);
Debug.Assert(false, expression.Evaluate()); // Without StrictTypeMatching, this would throw an exception.
```
