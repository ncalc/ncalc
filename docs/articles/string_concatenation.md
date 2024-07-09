# String concatenation

If the <xref:NCalc.ExpressionOptions.StringConcat> option specified the result will be a string concatenation. Otherwise it will try to perform an arithmetic addition if possible or if both values are strings then the result will be a string concatenation.

```c#
try
{
   var expression = new Expression($"5 +'1'", ExpressionOptions.StringConcat);
}
catch (OverflowException ex) { }
```