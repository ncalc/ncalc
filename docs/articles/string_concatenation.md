# String concatenation

If the both left and right values of expression are string the result will be a string concatenation. If not, you can still concatenate values as a string by using <xref:NCalc.ExpressionOptions.StringConcat> option.

```c#
try
{
   var expression = new Expression($"5 +'1'", ExpressionOptions.StringConcat);
}
catch (OverflowException ex) { }
```