# Expression test data

Put input/output cases in the `.yml` file for their subject. Each file has a
matching suite declaration in `YamlSuites.cs`, under `NCalc.Tests.Yaml`, so the
Test Explorer groups cases into classes such as `DecimalsTests` and
`OperatorsTests`. All suites inherit the same reader and executor.

For a new file, add its matching declaration:

```csharp
[InheritsTests]
public class NewFeatureTests : YamlEvaluationTests<NewFeatureTests>;
```

The suite coverage test fails if any YAML file has no matching class.

```yaml
- name: ShouldAddNumbers
  cases:
    - input: 1 + 1
      output: 2
    - input: 2 + 3
      output: 5

- name: ShouldAllowNullParameters
  configuration:
    evaluation:
      allowNullParameter: true
  cases:
    - input: value == null
      output: true
      parameters:
        value: null
    - input: null == null
      output: true
```

Write the test name once, without numbered variants. `name` must be unique within
its file, and `cases` must be a non-empty sequence. Each case requires `input` and
exactly one of `output` or `throws`. Input and output may be `null`. Test results
identify each case by its group name and input, within its file-specific class.

Put shared `configuration`, `parameters`, `culture`, `operation`, `compileToLambda`,
`resultType`, and `throws` on the group. Cases inherit those defaults and override the
fields that differ. Mappings merge recursively, including configuration sections
and parameter names; scalars and sequences replace the group
value. Each case receives its own parameters and configuration.

Configuration uses `ExpressionConfiguration` property names in camelCase
(`parsing`, `evaluation`, `evaluation.math`, `cacheEnabled`). Unknown properties
fail discovery instead of silently using defaults.

Each YAML case appears once in the explorer. By default, that test runs both
synchronous and asynchronous evaluation against the same expectation. Each
execution starts with independent parameters, including nested expressions. Set `compileToLambda: true` on a group or case to run it as a
compiled lambda instead. Compilation cases use their declared `output` or
`throws`; parity between the interpreter and compiler is outside this migration.
The existing native AOT exclusion still applies to lambda compilation.

```yaml
- name: ShouldCompileIntegerArithmetic
  compileToLambda: true
  resultType: int
  cases:
    - input: 1 + 2
      output: 3
```

Operations such as name extraction, parsing, and error detection run in a
separate operation test. `roundTrip` serializes and deserializes the AST before
the selected evaluator runs.

Optional group defaults and case overrides:

- `parameters`: static parameter names and values. A nested expression parameter
  uses `expression: { input: ..., parameters: ... }`, with optional configuration
  and culture, and no output.
- `compileToLambda`: defaults to `false` (sync and async evaluation). Set it to
  `true` for lambda compilation instead. A case can override its group default.
- `culture`: a culture name such as `de-DE`; defaults to invariant culture.
- `throws`: the expected exception type name, used instead of `output`. Set it
  on the group when every case expects the same exception, or on individual cases
  when a group mixes success and failure. An absent exception or a wrong type fails
  the test; subclasses are accepted, matching `Assert.Throws` semantics.
- `operation`: defaults to `evaluate`. Use `parameterCount`, `functionCount`,
  or `hasErrors` for the corresponding expression API output. `functionNames`
  compares the complete set of extracted names in ordinal order. `serialize`
  calls `ToExpressionString()`; `serializeWithParameters` enables parameter
  substitution. `escapeLike` tests the escaping helper directly. Exception cases
  can use `parse` for the parser context overload or `createLogicalExpression`
  for the logical expression factory. `roundTrip` serializes and deserializes
  the AST before evaluation.
- `resultType`: the generic evaluation/compilation result type, defaulting to
  `object`. Supported types are `object`, `bool`, `int`, `float`, `double`,
  `decimal`, and `string`.

Plain integers are `Int32`; floating point literals such as `3.0` are `Double`.
Quote numeric/boolean/null strings. Specify other CLR types explicitly to retain
precision and numeric-type coverage:

```yaml
- name: ShouldMultiplyDecimals
  cases:
    - input: x * 2
      output: { type: Decimal, value: '6.2' }
      parameters:
        x: { type: Decimal, value: '3.1' }

- name: ShouldIterateParameters
  configuration:
    evaluation:
      iterateParameters: true
  cases:
    - input: x * x
      output: [0, 1, 4]
      parameters:
        x: { type: 'Int32[]', value: [0, 1, 2] }
```

Typed scalars support `Decimal`, `Single`, `Double` (including `NaN` and
`Infinity`), `Int64`, `UInt64`, `UInt32`, `Int16`, `UInt16`, `Byte`, `SByte`, `Char`,
`DateTime`, `TimeSpan`, and `Guid`. Typed collections support `Int32[]`, `Double[]`,
`String[]`, and `List<Int32>`. Output sequences are compared in order, element by
element. String comparers use `ordinal`, `ordinalIgnoreCase`, `currentCulture`,
`currentCultureIgnoreCase`, `invariantCulture`, or `invariantCultureIgnoreCase`.

Exception cases use the same configuration and parameters:

```yaml
- name: ShouldRejectLargeFactorials
  throws: ArgumentOutOfRangeException
  cases:
    - input: 171!
    - input: 99999999999999!

- name: ShouldHandleFunctionCase
  cases:
    - input: Abs(-1)
      output: 1.0
    - input: aBs(-1)
      throws: NCalcFunctionNotFoundException
```

Supported exception names are `NCalcException`, `NCalcParserException`,
`NCalcEvaluationException`, `NCalcCastException`, `NCalcFunctionNotFoundException`,
`NCalcParameterNotDefinedException`, `InvalidOperationException`,
`ArgumentOutOfRangeException`, `OverflowException`, and `FormatException`.
Unknown names fail discovery.

Keep tests that need callbacks, custom C# contexts, state changes, assertions on
exception messages/properties/inner exceptions, AST shape checks, concurrency,
caching, or dependency injection in
C#. They should exercise that specific contract instead of introducing another
input/output data format.
