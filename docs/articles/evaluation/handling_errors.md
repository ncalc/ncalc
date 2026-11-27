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

Though, you can also detect syntax errors before the evaluation by using the <xref:NCalc.Expression.HasErrors(System.Threading.CancellationToken)> method.

```c#
var expression = new Expression("a + b * (");
if(expression.HasErrors())
{
    Console.WriteLine(expression.Error);
}
```

## Deeply Nested Expressions

NCalc evaluates the parsed tree recursively, so an expression with a very long chain of operators, such as
`1+1+1+...` with tens of thousands of terms, needs more stack than a thread can provide. Instead of crashing the
process with a stack overflow, NCalc stops the walk early.
<xref:NCalc.Expression.Evaluate(System.Threading.CancellationToken)>,
<xref:NCalc.Expression.EvaluateAsync(System.Threading.CancellationToken)> and `ToLambda` then throw a
<xref:NCalc.Exceptions.NCalcEvaluationException> whose `InnerException` is an `InsufficientExecutionStackException`.

```c#
var chain = string.Join("+", Enumerable.Repeat("1", 100_000));
try
{
    new Expression(chain).Evaluate();
}
catch (NCalcEvaluationException ex) when (ex.InnerException is InsufficientExecutionStackException)
{
    Console.WriteLine("The expression is nested too deeply.");
}
```

The depth at which this happens depends on the stack size of the calling thread, so it differs between the main
thread, thread-pool threads and operating systems. <xref:NCalc.Expression.GetParameterNames(System.Threading.CancellationToken)>,
<xref:NCalc.Expression.GetFunctionNames(System.Threading.CancellationToken)> and `ToExpressionString` walk the same
tree and throw the `InsufficientExecutionStackException` directly.

If you evaluate expressions supplied by users, treat this exception as invalid input rather than as a server fault,
and consider limiting the length of the expression text before parsing it.
