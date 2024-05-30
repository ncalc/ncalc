# 4.2
* [Improve Parlot error handling](https://github.com/ncalc/ncalc/pull/181) by [Andrey Bykiev](https://github.com/Bykiev)
* [Fix OverflowException with double values](https://github.com/ncalc/ncalc/pull/188) by [Andrey Bykiev](https://github.com/Bykiev), [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Fix double value precision loss](https://github.com/ncalc/ncalc/pull/188) by [Andrey Bykiev](https://github.com/Bykiev), [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Add new ExpressionOptions.AllowBooleanCalculation and ExpressionOptions.OrdinalStringComparer options. ExpressionOptions.CaseInsensitiveComparer is now obsolete, please use ExpressionOptions.CaseInsensitiveStringComparer instead](https://github.com/ncalc/ncalc/pull/188) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Add support for compilation of expressions to CLR lambdas](https://github.com/ncalc/ncalc/pull/188) by [Gustavo Mauricio de Barros](https://github.com/gumbarros). Credits to [Sebastian Klose](https://github.com/sklose) and [NCalc2 project](https://github.com/sklose/NCalc2) contributors

## Breaking Changes
* [Do not convert external function name to lower case when ExpressionOptions.IgnoreCase option is used](https://github.com/ncalc/ncalc/pull/179) by [Andrey Bykiev](https://github.com/Bykiev)
* [Add support for using null with operators](https://github.com/ncalc/ncalc/pull/184) by [Andrey Bykiev](https://github.com/Bykiev)
* [Exceptions need to be handled as NCalcEvaluationException instead of ArgumentException and added TypeHelper](https://github.com/ncalc/ncalc/pull/182) by [Gustavo Barros](https://github.com/gumbarros)
* [Use IOptions instead of IOptionsSnapshot at LogicalExpressionMemoryCache](https://github.com/ncalc/ncalc/pull/187) by [Gustavo Barros](https://github.com/gumbarros)
* [Changed the logic of ExpressionOptions.DecimalAsDefault. When this option is specified, all function parameters expected to be decimal](https://github.com/ncalc/ncalc/pull/188) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)

# 4.1
* [Remove excessive check for casing](https://github.com/ncalc/ncalc/pull/149) by [Andrey Bykiev](https://github.com/Bykiev)
* [Add support for comparison with null parameters](https://github.com/ncalc/ncalc/pull/156) by [Andrey Bykiev](https://github.com/Bykiev)
* [Fix support for TimeSpan and DateTime with hours, minutes and seconds](https://github.com/ncalc/ncalc/issues/158) by [Gustavo Barros](https://github.com/gumbarros)
* [Add Dependency Injection support with IMemoryCache plugin](https://github.com/ncalc/ncalc/issues/154) by [Gustavo Barros](https://github.com/gumbarros)
* [Add support for using semicolon as argument separator](https://github.com/ncalc/ncalc/pull/162) by [Andrey Bykiev](https://github.com/Bykiev)
* [Fix invalid token handling](https://github.com/ncalc/ncalc/pull/166) by [Andrey Bykiev](https://github.com/Bykiev)
* [Add support fo curly braces as alternative to square brackets](https://github.com/ncalc/ncalc/pull/169) by [Andrey Bykiev](https://github.com/Bykiev)
* [Re-added ANTLR as a plugin](https://github.com/ncalc/ncalc/pull/176) by [Gustavo Barros](https://github.com/gumbarros)

# 4.0
* [Parlot is used instead of Antlr for parsing](https://github.com/ncalc/ncalc/issues/137) by [Andrey Bykiev](https://github.com/Bykiev), [Gustavo Mauricio de Barros](https://github.com/gumbarros) and [Sébastien Ros](https://github.com/sebastienros)
* [`GetParametersNames` no longer adds same parameter more than one time to the result](https://github.com/ncalc/ncalc/issues/141) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [xUnit is now used for unit tests](https://github.com/ncalc/ncalc/issues/138) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [New DocFX website with articles and public API](https://github.com/ncalc/ncalc/issues/143) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)

## Breaking Changes
* .NET Framework 4.6.1 no longer supported, please update to .NET Framework 4.6.2 or higher
* Renamed `EvaluateOptions` enum to `ExpressionOptions`
* Renamed `EvaluateOptionsExtensions` class to `ExpressionOptionsExtensions`
* Renamed `Expression.OriginalExpression` property  to `Expression.ExpressionString`
* Renamed `Expression.ParsedExpression` property to `Expression.LogicalExpression`
* Renamed `Numbers` static class to `MathHelper`
* Removed `Expression.Compile` static method, please use `LogicalExpressionFactory.Create`
* Removed unused `BinaryExpressionType.Unknown` enum value
* `Expression.Error` property now stores an `Exception` object instead of a `string`
* `Expression.GetParametersNames` method now returns a `List<String>` instead of a `string[]`

# 3.13.1
* [CompareUsingMostPreciseType is now public again](https://github.com/ncalc/ncalc/commit/c3eb2778c7e83ef191b8f647cdd98f802f6af3bf) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Fixed `BitwiseXOr` behavior](https://github.com/ncalc/ncalc/pull/134) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)

# 3.13

* [Performance: Use pattern matching instead of TypeCode](https://github.com/ncalc/ncalc/pull/126) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Fix boolean comparison](https://github.com/ncalc/ncalc/pull/123) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Test for inner exception type instead of exception message](https://github.com/ncalc/ncalc/pull/127) by [axunonb](https://github.com/axunonb)
* [Update project to create unsigned and signed versions of NCalcSync](https://github.com/ncalc/ncalc/pull/129) by [axunonb](https://github.com/axunonb)

# 3.12

* [Allow using decimal as default floating point type](https://github.com/ncalc/ncalc/pull/118) by [Luca Schimweg](https://github.com/lucaschimweg)
* [Use correct CLR types at GetMostPreciseType](https://github.com/ncalc/ncalc/pull/116/) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Performance improvements: Optimize built-in functions name checking and use ConcurrentDictionary<string, WeakReference<LogicalExpression>> at caching](https://github.com/ncalc/ncalc/pull/114) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Options were not propagating to EvaluationVisitor](https://github.com/ncalc/ncalc/pull/111) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)

# 3.11

* [Evaluate function and parameters only once](https://github.com/ncalc/ncalc/pull/108) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Performance improvements: CA1860 and CA1834](https://github.com/ncalc/ncalc/pull/105) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Add GetParametersNames method to Expression.cs](https://github.com/ncalc/ncalc/pull/104) by [Gustavo Mauricio de Barros](https://github.com/gumbarros),
  [Added Expression.GetParameters() method](https://github.com/ncalc/ncalc/pull/104) by [Rodion Mostovoi](https://github.com/rodion-m)

# 3.10

* [Make EvaluationVisitor sub-class friendly](https://github.com/ncalc/ncalc/pull/92) by [Nick](https://github.com/thetreatment)
* [Add support for ifs(cond, val, default)](https://github.com/ncalc/ncalc/pull/91) by [Justin Baugh](https://github.com/baughj)
* [Added EvaluationVisitor at ctor](https://github.com/ncalc/ncalc/pull/90) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Updated ANTLR runtime to v4.13.1](https://github.com/ncalc/ncalc/issues/81)

# 3.9

* [.NET 8 + C#12 + ReSharper refactors](https://github.com/ncalc/ncalc/pull/88) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)
* [Added EvaluateOptions.CaseInsensitiveComparer](https://github.com/ncalc/ncalc/pull/86) by [Gustavo Mauricio de Barros](https://github.com/gumbarros)

# 3.8

* [CompareUsingMostPreciseType chooses first option, not best](https://github.com/ncalc/ncalc/issues/76) by [@ThomasHambach](https://github.com/ThomasHambach)
* [EvaluationVisitor.Result should be protected, not private](https://github.com/ncalc/ncalc/issues/79) by [Oleksandr Kovaliv](https://github.com/Kizuto3)

# 3.7

* [Add parameterless constructor to ValueExpression](https://github.com/ncalc/ncalc/pull/61) by [@ThomasHambach](https://github.com/ThomasHambach)
* [Add Min and Max to unit tests](https://github.com/ncalc/ncalc/pull/63) by by [@ThomasHambach](https://github.com/ThomasHambach)
* [Update from Antlr3.Runtime to Antlr4.Runtime.Standard 4.12.0](https://github.com/ncalc/ncalc-async/pull/18) by [@markcanary](https://github.com/markcanary)

# 3.6

* [Add arithmetic support for multiple operators and types](https://github.com/ncalc/ncalc/pull/59) by [Justin Baugh](https://github.com/baughj)
* [Using OrdinalIgnoreCase instead of ToLower() checks](https://github.com/ncalc/ncalc/pull/55) by [Andrey Bykiev](https://github.com/Bykiev)

# 3.5

* [Added Atan2 built-in function](https://github.com/ncalc/ncalc/pull/53) by [YuJiaHao](https://github.com/YuJiaHao)

# 3.4

* [Improved custom CultureInfo support](https://github.com/ncalc/ncalc/pull/52) by [rholek](https://github.com/rholek)

# 3.2

* [Disable tracing for Release builds](https://github.com/ncalc/ncalc/issues/51) by [Shifty15](https://github.com/Shifty15)

# 3.1

* [CultureInfo support](https://github.com/ncalc/ncalc/pull/46) by [Shifty15](https://github.com/Shifty15)

# 3.0

Several syntax changes to the grammar:

* [Exponentiation operator `**`](https://github.com/ncalc/ncalc/issues/36)
* [Case insensitive operators and key words (e.g. `AND` or `True`)](https://github.com/ncalc/ncalc/issues/37)
* [Support numbers with trailing dot (e.g. `47.`)](https://github.com/ncalc/ncalc/issues/21)
* [Support for positive sign (e.g. `+5`)](https://github.com/ncalc/ncalc/issues/11)

While these changes in themselves wouldn't introduce compatibility issues with previously valid statements, code that relies on statements with these constructs being invalid would be affected. The grammar also had to be regenerated with a new version of ANTLR with some fixes to it since it was clear that the generated source code had been modified manually. Manual review indicates that the regenerated grammar is identical, but because of both these reasons this is released as a new major version.

* [Bugfix: invalid tokens were skipped silently without any errors](https://github.com/ncalc/ncalc/issues/22). Expressions like `"4711"` would ignore the `"` (since that is not the string character in the NCalc syntax) and parse it as the number `4711`, but now an `EvaluationException` is thrown as for other syntax issues. This may affect existing expressions, but since they were always incorrect and now give an exception rather than silently getting a new value it does not merit a new major release.
* [Major bugfix: long integers are now treated as integers](https://github.com/ncalc/ncalc/issues/18). Previous versions converted them to single-precision floats, which caused data loss on large numbers. Since this affects the results of existing expressions, it requires a new major release.
* [New builtin function `Ln()`](https://github.com/ncalc/ncalc/pull/14)

# 2.0

Initial public release of the .NET Core version.
