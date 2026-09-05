# Architecture

The entire process of evaluating an expression can be demonstrated at this flowchart:

```mermaid
flowchart TB

A["1+1"] -->|Parsing| B("new BinaryExpression(new ValueExpression(1), new ValueExpression(1), BinaryExpressionType.Plus)")
B --> |Evaluation|2
```

## Parsing

Parsing is the process of analyzing the input expression and converting it into a structured format that can be easily
evaluated. We use [Parlot](https://github.com/sebastienros/parlot) to handle parsing, but you can use any parser you
want if you implement the interface <xref:NCalc.Factories.ILogicalExpressionFactory>.
For our example, "1+1", the parsing step converts the string into an abstract syntax tree (AST).
This tree is made up of different types of expressions, such as binary expressions, value expressions our even
functions.
Our AST is represented by <xref:NCalc.Domain.LogicalExpression> class.

### Source-generated parser

NCalc builds its Parlot parser at compile time. The generated parser is used automatically, without
runtime parser compilation or an AppContext switch. Parlot `2.0.0-preview-743` is restored from the
[Parlot preview feed](https://f.feedz.io/sebastienros/parlot/nuget/index.json), configured in `nuget.config`.

To reuse a parser with a specific configuration, pass expression and parser options to
`LogicalExpressionParser.CreateExpressionParser`:

```csharp
using System.Globalization;
using NCalc;
using NCalc.Parser;
using Parlot.Fluent;

var parser = LogicalExpressionParser.CreateExpressionParser(
    ExpressionOptions.DecimalAsDefault,
    LogicalExpressionParserOptions.Create(
        CultureInfo.InvariantCulture, ArgumentSeparator.Semicolon));

var logicalExpression = parser.Parse("Max(1.5; 2.5)");
var expression = new Expression(logicalExpression, ExpressionOptions.DecimalAsDefault);
var result = expression.Evaluate(); // 2.5m
```

Each parser binds its own options and can parse multiple inputs with ordinary Parlot parse contexts.
Omitting parser options captures `CultureInfo.CurrentCulture` when the parser is created, not at build
time or the first use of NCalc. Create a new parser to use different options, and do not mutate its
`CultureInfo` while it is shared. Cancellation remains a per-parse setting. The higher-level
`Expression` and `LogicalExpressionFactory` APIs continue to pass their options to the generated parser.

## Evaluation

Evaluation refers to the process of determining the value of an expression. We use the visitor pattern at evaluation.
This pattern allows you to add new operations to existing object structures without modifying those structures.
With the method <xref:NCalc.Domain.LogicalExpression.Accept``1(NCalc.Visitors.ILogicalExpressionVisitor{``0})> is possible to accept any kind of visitor that
implements <xref:NCalc.Visitors.ILogicalExpressionVisitor`1>. Example implementations
include <xref:NCalc.Visitors.EvaluationVisitor> that returns a <xref:System.Object>
and <xref:NCalc.Visitors.SerializationVisitor> that converts the AST into a <xref:System.String>.

If you are creating your custom implementation, beware it should be stateless to be easier to debug and read. This is
enforced by the [PureAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.contracts.pureattribute0) and generic return at
the <xref:NCalc.Domain.LogicalExpression.Accept``1(NCalc.Visitors.ILogicalExpressionVisitor{``0})> method.

## <xref:NCalc.Expression> Class

This is the main class of NCalc. It abstracts the process of parsing and evaluating the string.
The method <xref:NCalc.Expression.Evaluate> returns the actual value of its <xref:System.String> representation.

Example:

```c#
var expression = new Expression("2 * 3");
var result = expression.Evaluate();
  
Console.WriteLine(result);
```

This example above first creates an instance of <xref:NCalc.Expression> using a valued constructor. This constructor
takes a <xref:System.String> as parameter.
Then the method <xref:NCalc.Expression.Evaluate> is called to parse the <xref:System.String> and returns the actual
value represented by the <xref:System.String>.

To create expressions you can combine several [Operators](operators.md) and [Values](values.md).

## Learn More
For additional information on the technique we used to create this library please read [this
article](https://www.codeproject.com/articles/State-of-the-Art-Expression-Evaluation).
