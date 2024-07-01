# Handling Errors

When the expression has a syntax error, the evaluation will throw a <xref:NCalc.Exceptions.NCalcException>.

```c#
try
{
 new Expression("(3 + 2").Evaluate();
}
catch (NCalcParserException ex)
{
    Console.WriteLine("Error parsing the expression: {0}", ex.Message);
}
catch (NCalcEvaluationException ex)
{
    Console.WriteLine("Error evaluating the expression: {0}", ex.Message);
}
```

Though, you can also detect syntax errors before the evaluation by using the <xref:NCalc.Expression.HasErrors> method.

```c#
 var expression = new Expression("a + b * (");
 if(expression.HasErrors())
 {
     Console.WriteLine(expression.Error);
 }
```