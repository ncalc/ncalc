# Case Sensitivity

## Functions and Parameters
By default, the evaluation process is case-sensitive.
This means every parameter and function evaluation will match with case sensitivity. 
This behavior can be overriden for both using a `IDictionary` implementation with a custom `StringComparer`.

### Case-insensitive function
```c#
var expression = new Expression("secretOperation()")
{
    Context =
    {
        Functions = new Dictionary<string, ExpressionFunction>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "SecretOperation", _ => 42 }
        }
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
var configuration = new ExpressionConfiguration
{
    Evaluation = new ExpressionEvaluationOptions
    {
        IgnoreCaseAtBuiltInFunctions = true
    }
};

var expression = new Expression("aBs(-1)", configuration);
```

## String Comparison
You can also change the comparison of string values using <xref:NCalc.ExpressionEvaluationOptions.StringComparer>.

```c#
var expression = new Expression("{PageState} == 'list'");
expression.Parameters["PageState"] = "List";
Debug.Assert(false, expression.Evaluate());

var configuration = new ExpressionConfiguration
{
    Evaluation = new ExpressionEvaluationOptions
    {
        StringComparer = StringComparer.CurrentCultureIgnoreCase
    }
};

expression = new Expression("{PageState} == 'list'", configuration);
expression.Parameters["PageState"] = "List";
Debug.Assert(true, expression.Evaluate());
```
