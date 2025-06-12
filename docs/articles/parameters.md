# Parameters

## Static Parameters

Static parameters are values which can be defined before the evaluation of an expression.
These parameters can be accessed using the <xref:NCalc.ExpressionBase`1.Parameters> property of the <xref:NCalc.Expression>
instance.

```c#
var expression = new Expression("2 * [x] ** 2 + 5 * [y]");
expression.Parameters["x"] = 5;
expression.Parameters["y"] = 1;

Console.WriteLine(expression.Evaluate());
```

Parameters can be useful when a value is unknown at compile time, or when performance is important and the parsing can
be saved for further calculations.

## Expression Parameters

Expressions can be split into several ones by defining expression parameters. Those parameters are not simple values but
<xref:NCalc.Expression> instances themselves.

```c#
Expression volume = new Expression("[surface] * h");
Expression surface = new Expression("[l] * [L]");
volume.Parameters["surface"] = surface;
surface.Parameters["l"] = 1;
surface.Parameters["L"] = 2;
```

## Dynamic Parameters

Sometimes parameters can be even more complex to evaluate and need a dedicated method to be evaluated. This can be done
using the <xref:NCalc.ExpressionParameter> delegate.

```c#
var expression = new Expression("Round(Pow([Pi], 2) + Pow([Pi], 2) + [X], 2)");

expression.Parameters["Pi2"] = new Expression("Pi * [Pi]");
expression.Parameters["X"] = 10;

expression.DynamicParameters["Pi"] = _ => {
    Console.WriteLine("I'm evaluating π!");
    return 3.14;
};
```

## Square Brackets Parameters

Parameters in between square brackets can contain special characters like spaces, dots, and also start with digits.

```c#
var expression = new Expression("[My First Parameter] + [My Second Parameter]");
```

## Curly Braces Parameters

You can also use a curly braces as alternative to square brackets.

```c#
var expression = new Expression("{PageState} ==  'List'");
```

## Multi-Valued Parameters

When parameters are `IEnumerable` and the <xref:NCalc.ExpressionOptions.IterateParameters> is
used, the result is a `List<object?>` made of the evaluation of each value in the parameter.

```c#
var expression = new Expression("(a * b) ** c", ExpressionOptions.IterateParameters);
expression.Parameters["a"] = new int[] { 1, 2, 3, 4, 5 };
expression.Parameters["b"] = new int[] { 6, 7, 8, 9, 0 };
expression.Parameters["c"] = 3;

foreach (var result in (IList)expression.Evaluate())
{
    Console.WriteLine(result);
}

//  216
//  2744
//  13824
//  46656
//  0
```

## Using Event Handlers
You can also use event handlers to handle parameters.
```csharp
expression.EvaluateParameter += delegate(string name, ParameterArgs args)
{
    if (name == "Pi")
        args.Result = 3.14;
};
```

## Compare with Null Parameters

When parameter is null and <xref:NCalc.ExpressionOptions.AllowNullParameter> is used, comparison of values to null is
allowed.

```c#
var expression = new Expression("'a string' == null", ExpressionOptions.AllowNullParameter);
(bool)expression.Evaluate();

//  False
```

## Getting all Parameters from an Expression

```c#
var expression = new Expression ("if(x=0,x,y)"); 
expression.Parameters["x"] = 1;
expression.Parameters["y"] = "pan";
var parameters = expression.GetParametersNames(); 
//  x
//  y
```

## Case Sensitivity
See [case_sensitivity](case_sensitivity.md) for more info.

## Assigning and Updating Parameters 

Parameters can be assigned in expressions. 
Support for assignments must be enabled by including the <xref:NCalc.ExpressionOptions.UseAssignments> flag into <xref:NCalc.ExpressionOptions> of an <xref:NCalc.Expression>.

When a parameter is assigned, first the <xref:NCalc.Expression.OnUpdateParameter> event is fired. An event handler may tell the evaluation engine to update the static parameter table or bypass this step by setting the UpdateParameterArgs.<xref:NCalc.Handlers.UpdateParameterArgs.UpdateParameterLists> or AsyncUpdateParameterArgs.<xref:NCalc.Handlers.AsyncUpdateParameterArgs.UpdateParameterLists> property to `true` or `false` respectively.

An assignment is an expression, so it can be used wherever a value is accepted. E.g., the following operations are equivalent:

```
if (true, a := 2, a := 4); a + Max(2; 4)
a := if (true; 2; 4); a + Max(2; 4)
```

Assignment can be combined with an operator (such as "+=" for addition with assignment); please, see the [Operators](operators.md) topic for the list of supported operators with assignment.