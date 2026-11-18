# Overflow Protection

To handle binary arithmetic operation overflow, enable <xref:NCalc.Helpers.MathOptions.OverflowProtection>.

```c#
try
{
    var configuration = new ExpressionConfiguration
    {
        Evaluation = new ExpressionEvaluationOptions
        {
            Math = new MathOptions
            {
                OverflowProtection = true
            }
        }
    };

    var expression = new Expression($"{int.MaxValue} + 1", configuration);
}
catch (OverflowException ex) { }
```

The factorial operator is not affected by this option. A factorial that leaves the range of its type, such as `13!` on an `int` or `21!` on a `long`, always throws an `OverflowException`, because a wrapped factorial is never a meaningful value.
