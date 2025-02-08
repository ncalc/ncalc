﻿# Case Sensitivity

## Functions and Parameters
By default, the evaluation process is case-sensitive.
This means every parameter and function evaluation will match with case sensitivity. 
This behavior can be overriden for both using a `IDictionary` implementation with a custom `StringComparer`.

### Case-insensitive function
```c#
var expression = new Expression("secretOperation()")
{
    Functions = new Dictionary<string, ExpressionFunction>(StringComparer.InvariantCultureIgnoreCase)
    {
        { "SecretOperation", (arguments) => 42 }
    }
};
```
### Case-insensitive parameter

```c#
var expression = new Expression("name == 'Beatriz'")
{
    Parameters = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
    {
        { "Name", "Beatriz" }
    }
};
```

## Case-insensitive built-in function
```c#
var expression = new Expression("aBs(-1)", ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
```

## String Comparison
You can also change the comparison of string values using <xref:NCalc.ExpressionOptions.CaseInsensitiveStringComparer>.

```c#
var expression = new Expression("{PageState} == 'list'");
expression.Parameters["PageState"] = "List";
Debug.Assert(false, expression.Evaluate());

expression = new Expression("{PageState} == 'list'", ExpressionOptions.CaseInsensitiveStringComparer);
expression.Parameters["PageState"] = "List";
Debug.Assert(true, expression.Evaluate());
```