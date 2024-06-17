# Overflow Protection

To handle binary arithmetic operation overflow you should use <xref:NCalc.ExpressionOptions.OverflowProtection>.

```c#
try
{
   var expression = new Expression($"{int.MaxValue} + 1", ExpressionOptions.OverflowProtection);
}
catch (OverflowException ex) { }
```