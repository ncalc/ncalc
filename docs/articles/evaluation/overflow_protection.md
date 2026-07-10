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
