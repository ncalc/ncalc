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
