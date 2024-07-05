# String concatenation

If the left or right values of expression are strings and <xref:NCalc.ExpressionOptions.StringConcat> option specified the result will be a string concatenation. Otherwise the arithmetic addition will be performed event with <xref:NCalc.ExpressionOptions.StringConcat> option specified.

```c#
try
{
   var expression = new Expression($"5 +'1'", ExpressionOptions.StringConcat);
}
catch (OverflowException ex) { }
```