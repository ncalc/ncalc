# Handling Errors

When the expression has a syntax error, the evaluation will throw an **EvaluationException**.

```c#
 try
 {
     new Expression("(3 + 2").Evaluate();
 }
 catch(EvaluationException e)
 {
     Console.WriteLine("Error catched: " + e.Message);
 }
```

Though, you can also detect syntax errors before the evaluation by using the **HasErrors()** method.

```c#
 var expression = new Expression("a + b * (");
 if(e.HasErrors())
 {
     Console.WriteLine(e.Error);
 }