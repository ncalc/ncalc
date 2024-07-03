# String concatenation

To concatenate values as a string you should use <xref:NCalc.ExpressionOptions.StringConcat>.

```c#
try
{
   var expression = new Expression($"5 + \"1\"", ExpressionOptions.StringConcat);
}
catch (OverflowException ex) { }
```