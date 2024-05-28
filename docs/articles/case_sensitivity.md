# Case Sensitivity

By default, the evaluation process is case-sensitive.
This means every parameter and function evaluation will match using case. 
This behavior can be overriden using a specific evaluation option.

```c#
 var expression = new Expression("aBs(-1)", ExpressionOptions.IgnoreCase);
 Debug.Assert(1M, e.Evaluate());
```

You can also change the comparison behavior of expressions.

```c#
 var expression = new Expression("{PageState} == 'list'");
 expression.Parameters["PageState"] = "List";
 Debug.Assert(false, e.Evaluate());
 
 expression = new Expression("{PageState} == 'list'");
 expression.Parameters["PageState"] = "List";
 Debug.Assert(true, e.Evaluate());
```