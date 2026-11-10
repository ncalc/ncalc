namespace NCalc.Tests;

public class YamlTestDataTests
{
    [Test]
    public async Task ShouldPreserveValueTypesAndConfiguration()
    {
        const string yaml = """
            - name: TypedValues
              configuration:
                parsing:
                  integerNumberType: Int64
                evaluation:
                  allowNullParameter: true
                  stringComparer: ordinalIgnoreCase
              cases:
                - input: x
                  output: { type: Decimal, value: '0.1234567890123456789012345678' }
                  parameters:
                    integer: 3
                    floatingPoint: 3.0
                    quoted: '3.0'
                    text: 'null'
                    nothing: null
                    character: { type: Char, value: ' ' }
                    large: { type: UInt64, value: '18446744073709551615' }
            """;

        var cases = YamlTestData.ReadCases(yaml, "schema").ToArray();
        await Assert.That(cases.Length).IsEqualTo(1);
        var testCase = cases[0];
        await Assert.That(testCase.Output).IsEqualTo(0.1234567890123456789012345678m);
        await Assert.That(testCase.CompileToLambda).IsFalse();
        await Assert.That(testCase.Parameters["integer"]).IsTypeOf<int>();
        await Assert.That(testCase.Parameters["floatingPoint"]).IsTypeOf<double>();
        await Assert.That(testCase.Parameters["quoted"]).IsEqualTo("3.0");
        await Assert.That(testCase.Parameters["text"]).IsEqualTo("null");
        await Assert.That(testCase.Parameters["nothing"]).IsNull();
        await Assert.That(testCase.Parameters["character"]).IsEqualTo(' ');
        await Assert.That(testCase.Parameters["large"]).IsEqualTo(ulong.MaxValue);
        await Assert.That(testCase.Configuration.Parsing.IntegerNumberType).IsEqualTo(IntegerNumberType.Int64);
        await Assert.That(testCase.Configuration.Evaluation.AllowNullParameter).IsTrue();
        await Assert.That(testCase.Configuration.Evaluation.StringComparer).IsEqualTo(StringComparer.OrdinalIgnoreCase);
    }

    [Test]
    [Arguments("[]")]
    [Arguments("- name: MissingCases")]
    [Arguments("- name: EmptyCases\n  cases: []")]
    [Arguments("- name: MissingOutput\n  cases: [{ input: '1' }]")]
    [Arguments("- name: Typo\n  configration: {}\n  cases: [{ input: '1', output: 1 }]")]
    [Arguments("- name: Typo\n  cases: [{ input: '1', output: 1, configration: {} }]")]
    [Arguments("- name: Both\n  cases: [{ input: '1', output: 1, throws: OverflowException }]")]
    [Arguments("- name: InheritedBoth\n  throws: OverflowException\n  cases: [{ input: '1', output: 1 }]")]
    [Arguments("- name: UnknownException\n  cases: [{ input: '1', throws: TypoException }]")]
    [Arguments("- name: NullException\n  cases: [{ input: '1', throws: null }]")]
    [Arguments("- name: MissingInput\n  throws: OverflowException\n  cases: [{}]")]
    [Arguments("- name: RemovedModes\n  modes: [sync]\n  cases: [{ input: '1', output: 1 }]")]
    [Arguments("- name: InvalidCompilationFlag\n  compileToLambda: sometimes\n  cases: [{ input: '1', output: 1 }]")]
    public void ShouldRejectInvalidSchema(string yaml)
    {
        Assert.Throws<InvalidDataException>(() => YamlTestData.ReadCases(yaml, "schema").ToArray());
    }

    [Test]
    public void ShouldRejectDuplicateGroupNames()
    {
        const string yaml = """
            - name: Duplicate
              cases: [{ input: '1', output: 1 }]
            - name: Duplicate
              cases: [{ input: '2', output: 2 }]
            """;
        Assert.Throws<InvalidDataException>(() => YamlTestData.ReadCases(yaml, "schema").ToArray());
    }

    [Test]
    public async Task ShouldInheritGroupDefaultsWithoutChangingOtherCases()
    {
        const string yaml = """
            - name: SharedConfiguration
              compileToLambda: true
              configuration:
                parsing:
                  integerNumberType: Int64
                evaluation:
                  allowNullParameter: true
                  math:
                    floatingPointNumberType: Decimal
              parameters: { x: 3, y: 5 }
              cases:
                - input: x
                  output: 3
                - input: x
                  output: 9
                  compileToLambda: false
                  configuration:
                    evaluation:
                      allowNullParameter: false
                  parameters: { x: 9 }
                - input: y
                  output: 5
            """;

        var cases = YamlTestData.ReadCases(yaml, "schema").ToArray();
        await Assert.That(cases.Length).IsEqualTo(3);
        await Assert.That(cases.Select(c => c.Name).Distinct().Count()).IsEqualTo(1);
        await Assert.That(cases[0].CompileToLambda).IsTrue();
        await Assert.That(cases[1].CompileToLambda).IsFalse();
        await Assert.That(cases[2].CompileToLambda).IsTrue();
        await Assert.That(cases[1].Configuration.Parsing.IntegerNumberType).IsEqualTo(IntegerNumberType.Int64);
        await Assert.That(cases[1].Configuration.Evaluation.Math.FloatingPointNumberType).IsEqualTo(FloatingPointNumberType.Decimal);
        await Assert.That(cases[1].Configuration.Evaluation.AllowNullParameter).IsFalse();
        await Assert.That(cases[1].Parameters["x"]).IsEqualTo(9);
        await Assert.That(cases[1].Parameters["y"]).IsEqualTo(5);
        await Assert.That(cases[2].Configuration.Evaluation.AllowNullParameter).IsTrue();
        await Assert.That(cases[2].Parameters["x"]).IsEqualTo(3);
    }

    [Test]
    public async Task ShouldReadExceptionExpectationsFromGroupsAndCases()
    {
        const string yaml = """
            - name: Exceptions
              throws: OverflowException
              cases:
                - input: a + b
                - input: '1 +'
                  throws: NCalcParserException
            """;

        var cases = YamlTestData.ReadCases(yaml, "schema").ToArray();
        await Assert.That(cases.Length).IsEqualTo(2);
        await Assert.That(cases[0].Throws).IsEqualTo(typeof(OverflowException));
        await Assert.That(cases[1].Throws).IsEqualTo(typeof(NCalc.Exceptions.NCalcParserException));
        await Assert.That(cases[0].Output).IsNull();
    }

    [Test]
    public async Task ShouldHaveSuiteForEveryYamlFile()
    {
        var files = Directory.GetFiles(Path.Join(AppContext.BaseDirectory, "TestData"), "*.yml")
            .Select(Path.GetFileNameWithoutExtension).Order(StringComparer.Ordinal).ToArray();
        var suites = typeof(YamlTestDataTests).Assembly.GetTypes()
            .Where(type => type.BaseType is { IsGenericType: true } parent &&
                parent.GetGenericTypeDefinition() == typeof(YamlEvaluationTests<>))
            .Select(type => type.Name).Order(StringComparer.Ordinal).ToArray();

        await Assert.That(suites.Length).IsEqualTo(files.Length);
        for (var i = 0; i < files.Length; i++)
            await Assert.That(suites[i]).IsEqualTo(files[i]);
    }

    [Test]
    public async Task ShouldCreateIndependentExpressionsForEachEvaluation()
    {
        const string yaml = """
            - name: NestedParameters
              cases:
                - input: surface * h
                  output: 6
                  parameters:
                    h: 3
                    surface:
                      expression:
                        input: l * L
                        parameters: { l: 1, L: 2 }
            """;
        var testCase = YamlTestData.ReadCases(yaml, "schema").Single();
        var first = testCase.CreateExpression();
        first.Parameters["h"] = 9;
        ((Expression)first.Parameters["surface"]).Parameters["l"] = 7;

        var second = testCase.CreateExpression();
        await Assert.That(second.Evaluate(CancellationToken.None)).IsEqualTo(6);
        await Assert.That(testCase.Parameters["h"]).IsEqualTo(3);
        await Assert.That(((Expression)testCase.Parameters["surface"]).Parameters["l"]).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldReadConfigurationFlagsAndDefaults()
    {
        const string yaml = """
            - name: Configuration
              cases:
                - input: '1'
                  output: 1
                  configuration:
                    cacheEnabled: false
                    parsing:
                      allowCharValues: true
                      disallowSingleEquals: true
                      argumentSeparator: Comma, Semicolon
                      floatingPointNumberType: Decimal
                      integerNumberType: Int64
                    evaluation:
                      ignoreCaseAtBuiltInFunctions: true
                      iterateParameters: true
                      allowNullParameter: true
                      stringConcat: true
                      noStringTypeCoercion: true
                      allowNullOrEmptyExpressions: true
                      arithmeticNullOrEmptyStringAsZero: true
                      strictTypeMatching: true
                      concurrentAsyncEvaluation: true
                      stringComparer: ordinal
                      math:
                        floatingPointNumberType: Decimal
                        integerNumberType: Int64
                        allowBooleanCalculation: true
                        overflowProtection: true
                        midpointRounding: AwayFromZero
                - input: '1'
                  output: 1
                  configuration: {}
            """;
        var cases = YamlTestData.ReadCases(yaml, "schema").ToArray();
        var configuration = cases[0].Configuration;
        await Assert.That(configuration.CacheEnabled).IsFalse();
        await Assert.That(configuration.Parsing.AllowCharValues).IsTrue();
        await Assert.That(configuration.Parsing.DisallowSingleEquals).IsTrue();
        await Assert.That(configuration.Parsing.ArgumentSeparator).IsEqualTo(ArgumentSeparator.Comma | ArgumentSeparator.Semicolon);
        await Assert.That(configuration.Parsing.FloatingPointNumberType).IsEqualTo(FloatingPointNumberType.Decimal);
        await Assert.That(configuration.Parsing.IntegerNumberType).IsEqualTo(IntegerNumberType.Int64);
        var evaluation = configuration.Evaluation;
        await Assert.That(evaluation.IgnoreCaseAtBuiltInFunctions).IsTrue();
        await Assert.That(evaluation.IterateParameters).IsTrue();
        await Assert.That(evaluation.AllowNullParameter).IsTrue();
        await Assert.That(evaluation.StringConcat).IsTrue();
        await Assert.That(evaluation.NoStringTypeCoercion).IsTrue();
        await Assert.That(evaluation.AllowNullOrEmptyExpressions).IsTrue();
        await Assert.That(evaluation.ArithmeticNullOrEmptyStringAsZero).IsTrue();
        await Assert.That(evaluation.StrictTypeMatching).IsTrue();
        await Assert.That(evaluation.ConcurrentAsyncEvaluation).IsTrue();
        await Assert.That(evaluation.StringComparer).IsEqualTo(StringComparer.Ordinal);
        await Assert.That(evaluation.Math.FloatingPointNumberType).IsEqualTo(FloatingPointNumberType.Decimal);
        await Assert.That(evaluation.Math.IntegerNumberType).IsEqualTo(IntegerNumberType.Int64);
        await Assert.That(evaluation.Math.AllowBooleanCalculation).IsTrue();
        await Assert.That(evaluation.Math.OverflowProtection).IsTrue();
        await Assert.That(evaluation.Math.MidpointRounding).IsEqualTo(MidpointRounding.AwayFromZero);
        var defaults = cases[1].Configuration;
        await Assert.That(defaults.CacheEnabled).IsTrue();
        await Assert.That(defaults.Parsing.ArgumentSeparator).IsEqualTo(ArgumentSeparator.Comma);
        await Assert.That(defaults.Evaluation.AllowNullParameter).IsFalse();
        await Assert.That(defaults.Evaluation.Math.MidpointRounding).IsEqualTo(MidpointRounding.ToEven);
    }

    [Test]
    [Arguments("{ typo: true }")]
    [Arguments("{ parsing: { typo: true } }")]
    [Arguments("{ evaluation: { typo: true } }")]
    [Arguments("{ evaluation: { math: { typo: true } } }")]
    public void ShouldRejectUnknownConfigurationProperties(string configuration)
    {
        var yaml = $"- name: Typo\n  configuration: {configuration}\n  cases: [{{ input: '1', output: 1 }}]";
        Assert.Throws<InvalidDataException>(() => YamlTestData.ReadCases(yaml, "schema").ToArray());
    }
}
