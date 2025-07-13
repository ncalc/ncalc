# Functions

## Built-in Functions

The framework includes a set of already implemented functions.

| Name		         | Description	                                                                                                                                                                                                     | Usage	               | Result |
|----------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------|--------|
| Abs		          | Returns the absolute value of a specified number.	                                                                                                                                                               | Abs(-1)	             | 1d     |
| Acos		         | Returns the angle whose cosine is the specified number.	                                                                                                                                                         | Acos(1)	             | 0d     |
| Asin		         | Returns the angle whose sine is the specified number.	                                                                                                                                                           | Asin(0)	             | 0d     |
| Atan		         | Returns the angle whose tangent is the specified number.	                                                                                                                                                        | Atan(0)	             | 0d     |
| Ceiling	       | Returns the smallest integer greater than or equal to the specified number.	                                                                                                                                     | Ceiling(1.5)	        | 2d     |
| Cos		          | Returns the cosine of the specified angle.	                                                                                                                                                                      | Cos(0)	              | 1d     |
| Exp		          | Returns e raised to the specified power.	                                                                                                                                                                        | Exp(0)	              | 1d     |
| Floor		        | Returns the largest integer less than or equal to the specified number.	                                                                                                                                         | Floor(1.5)	          | 1d     |
| IEEERemainder	 | Returns the remainder resulting from the division of a specified number by another specified number.	                                                                                                            | IEEERemainder(3, 2)	 | -1d    |
| Ln		        | Returns the natural logarithm of a specified number.	                                                                                                                                                            | Ln(1)	            | 0d     |
| Log		          | Returns the logarithm of a specified number.	                                                                                                                                                                    | Log(1, 10)	          | 0d     |
| Log10		        | Returns the base 10 logarithm of a specified number.	                                                                                                                                                            | Log10(1)	            | 0d     |
| Max		          | Returns the larger of two specified numbers.	                                                                                                                                                                    | Max(1, 2)	           | 2      |
| Min		          | Returns the smaller of two numbers.	                                                                                                                                                                             | Min(1, 2)	           | 1      |
| Pow		          | Returns a specified number raised to the specified power.	                                                                                                                                                       | Pow(3, 2)	           | 9d     |
| Round		        | Rounds a value to the nearest integer or specified number of decimal places. The mid number behaviour can be changed by using ExpressionOptions.RoundAwayFromZero during construction of the Expression object.	 | Round(3.222, 2)	     | 3.22d  |
| Sign		         | Returns a value indicating the sign of a number.	                                                                                                                                                                | Sign(-10)	           | -1     |
| Sin		          | Returns the sine of the specified angle.	                                                                                                                                                                        | Sin(0)	              | 0d     |
| Sqrt		         | Returns the square root of a specified number.	                                                                                                                                                                  | Sqrt(4)	             | 2d     |
| Tan		          | Returns the tangent of the specified angle.	                                                                                                                                                                     | Tan(0)	              | 0d     |
| Truncate	      | Calculates the integral part of a number.	                                                                                                                                                                       | Truncate(1.7)	       | 1      |

It also includes other general purpose ones.

| Name		 | Description	                                                                                      | Usage	                                            | Result                                                                         |
|--------|---------------------------------------------------------------------------------------------------|---------------------------------------------------|--------------------------------------------------------------------------------|
| in	    | Returns whether an element is in a set of values.	                                                | in(1 + 1, 1, 2, 3)	                               | true                                                                           |
| if	    | Returns a value based on a condition.	                                                            | if(3 % 2 = 1, 'value is true', 'value is false')	 | 'value is true'                                                                |
| ifs    | Returns a value based on evaluating a number of conditions, returning a default if none are true. | ifs(foo > 50, "bar", foo > 75, "baz", "quux")     | if foo is between 50 and 75 "bar", foo greater than 75 "baz", otherwise "quux" |  

You can use comma (,) or semicolon (;) as argument separator.

If <xref:NCalc.ExpressionOptions.DecimalAsDefault> is used all functions will cast the arguments to <xref:System.Decimal>.

## Custom Functions
Custom functions are created using the <xref:NCalc.ExpressionFunction> delegate. The parameters are <xref:NCalc.Expression> instances that can be lazy evaluated.
```csharp
expression.Functions["SecretOperation"] = (args) => {
    return (int)args[0].Evaluate() + (int)args[1].Evaluate();
};

```

## Using Event Handlers
You can also use event handlers to handle functions.
```csharp
expression.EvaluateFunction += delegate(string name, FunctionArgs args)
{
    if (name == "SecretOperation")
        args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
    
    return ValueTask.CompletedTask;
};
```

## Case Sensitivity
See [case_sensitivity](case_sensitivity.md) for more info.
