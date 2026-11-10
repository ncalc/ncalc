#nullable enable
using NCalc.Exceptions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NCalc.Tests.TestData;

public sealed class ExpressionTestCase
{
    public required string Name { get; init; }
    public required string? Input { get; init; }
    public required object? Output { get; init; }
    public Type? Throws { get; init; }
    public ExpressionConfiguration Configuration { get; init; } = new();
    public Dictionary<string, object?> Parameters { get; init; } = [];
    public CultureInfo Culture { get; init; } = CultureInfo.InvariantCulture;
    public string Operation { get; init; } = "evaluate";
    public bool CompileToLambda { get; init; }
    public string ResultType { get; init; } = "object";

    public Expression CreateExpression() => new(Input, Configuration,
        new ExpressionContext { Parameters = CopyParameters(Parameters) }, Culture);

    private static Dictionary<string, object?> CopyParameters(IDictionary<string, object?> parameters) =>
        parameters.ToDictionary(p => p.Key, p => p.Value is Expression nested
            ? new Expression(nested.ExpressionString, nested.Configuration,
                new ExpressionContext { Parameters = CopyParameters(nested.Parameters) }, nested.CultureInfo)
            : p.Value);

    public override string ToString() => $"{Name[(Name.IndexOf('/') + 1)..]}: {Input ?? "null"}";
}

public static class YamlTestData
{
    private static readonly IDeserializer ConfigurationDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithTypeConverter(new StringComparerConverter())
        .Build();

    public static IEnumerable<ExpressionTestCase> GetCases(string suite)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "TestData", $"{suite}.yml");
        return ReadCases(File.ReadAllText(file), suite);
    }

    public static IEnumerable<ExpressionTestCase> ReadCases(string yaml, string suite)
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(yaml));
        if (stream.Documents.Count != 1 ||
            stream.Documents[0].RootNode is not YamlSequenceNode { Children.Count: > 0 } cases)
            throw new InvalidDataException($"{suite}: expected a non-empty sequence of test cases.");

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (YamlMappingNode item in cases)
        {
            CheckKeys(item, "name", "cases", "configuration", "parameters", "culture", "resultType",
                "operation", "throws", "compileToLambda");
            var name = Scalar(item["name"]);
            if (string.IsNullOrWhiteSpace(name) || !names.Add(name))
                throw new InvalidDataException($"{suite}: missing or duplicate group name '{name}'.");
            if (!item.Children.TryGetValue("cases", out var caseList) ||
                caseList is not YamlSequenceNode { Children.Count: > 0 } groupCases)
                throw new InvalidDataException($"{suite}/{name}: expected a non-empty cases sequence.");

            var defaults = new YamlMappingNode();
            foreach (var (key, value) in item.Children)
            {
                if (Scalar(key) is not ("name" or "cases"))
                    defaults.Add(key, value);
            }

            foreach (YamlMappingNode testCase in groupCases)
            {
                CheckKeys(testCase, "input", "output", "configuration", "parameters", "culture", "resultType",
                    "operation", "throws", "compileToLambda");
                var expression = Merge(defaults, testCase);
                if (!testCase.Children.ContainsKey("input") ||
                    expression.Children.ContainsKey("output") == expression.Children.ContainsKey("throws"))
                    throw new InvalidDataException($"{suite}/{name}: input and exactly one of output or throws are required.");
                yield return ReadExpression(expression, $"{suite}/{name}");
            }
        }
    }

    private static YamlMappingNode Merge(YamlMappingNode defaults, YamlMappingNode testCase)
    {
        var merged = new YamlMappingNode();
        foreach (var (key, value) in defaults.Children)
            merged.Add(key, value);
        foreach (var (key, value) in testCase.Children)
        {
            merged.Children[key] = value is YamlMappingNode overrides &&
                                   merged.Children.TryGetValue(key, out var original) &&
                                   original is YamlMappingNode inherited
                ? Merge(inherited, overrides)
                : value;
        }

        return merged;
    }

    private static ExpressionTestCase ReadExpression(YamlMappingNode expression, string name) => new()
    {
        Name = name,
        Input = IsNull(expression["input"]) ? null : Scalar(expression["input"]),
        Output = expression.Children.TryGetValue("output", out var output) ? ReadValue(output) : null,
        Throws = expression.Children.TryGetValue("throws", out var exception) ? ReadExceptionType(Scalar(exception)) : null,
        Configuration = expression.Children.TryGetValue("configuration", out var configuration)
            ? ReadConfiguration(configuration)
            : new(),
        Parameters = expression.Children.TryGetValue("parameters", out var parameters)
            ? ReadParameters((YamlMappingNode)parameters)
            : [],
        Culture = expression.Children.TryGetValue("culture", out var culture)
            ? CultureInfo.GetCultureInfo(Scalar(culture))
            : CultureInfo.InvariantCulture,
        CompileToLambda = expression.Children.TryGetValue("compileToLambda", out var compileToLambda)
            && ReadCompileToLambda(compileToLambda),
        Operation = expression.Children.TryGetValue("operation", out var operation) ? Scalar(operation) : "evaluate",
        ResultType = expression.Children.TryGetValue("resultType", out var resultType) ? Scalar(resultType) : "object"
    };

    private static bool ReadCompileToLambda(YamlNode node) =>
        bool.TryParse(Scalar(node), out var enabled)
            ? enabled : throw new InvalidDataException("compileToLambda must be true or false.");

    private static Type ReadExceptionType(string name) => name switch
    {
        nameof(NCalcException) => typeof(NCalcException),
        nameof(NCalcParserException) => typeof(NCalcParserException),
        nameof(NCalcEvaluationException) => typeof(NCalcEvaluationException),
        nameof(NCalcCastException) => typeof(NCalcCastException),
        nameof(NCalcFunctionNotFoundException) => typeof(NCalcFunctionNotFoundException),
        nameof(NCalcParameterNotDefinedException) => typeof(NCalcParameterNotDefinedException),
        nameof(InvalidOperationException) => typeof(InvalidOperationException),
        nameof(ArgumentOutOfRangeException) => typeof(ArgumentOutOfRangeException),
        nameof(OverflowException) => typeof(OverflowException),
        nameof(FormatException) => typeof(FormatException),
        var type => throw new InvalidDataException($"Unsupported YAML exception type '{type}'.")
    };

    private static Dictionary<string, object?> ReadParameters(YamlMappingNode parameters) =>
        parameters.Children.ToDictionary(p => Scalar(p.Key), p => ReadValue(p.Value));

    private static ExpressionConfiguration ReadConfiguration(YamlNode node)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        new YamlStream(new YamlDocument(node)).Save(writer, false);
        return ConfigurationDeserializer.Deserialize<ExpressionConfiguration>(writer.ToString());
    }

    private static object? ReadValue(YamlNode node)
    {
        if (node is YamlSequenceNode sequence)
            return sequence.Select(ReadValue).ToArray();
        if (node is YamlMappingNode mapping)
        {
            if (mapping.Children.TryGetValue("expression", out var nested))
            {
                CheckKeys(mapping, "expression");
                var nestedExpression = (YamlMappingNode)nested;
                CheckKeys(nestedExpression, "input", "configuration", "parameters", "culture");
                return ReadExpression(nestedExpression, "parameter").CreateExpression();
            }

            CheckKeys(mapping, "type", "value");
            if (mapping["value"] is YamlSequenceNode values)
            {
                var items = values.Select(ReadValue).ToArray();
                return Scalar(mapping["type"]) switch
                {
                    "Int32[]" => items.Cast<int>().ToArray(),
                    "Double[]" => items.Cast<double>().ToArray(),
                    "String[]" => items.Cast<string>().ToArray(),
                    "List<Int32>" => items.Cast<int>().ToList(),
                    var type => throw new InvalidDataException($"Unsupported YAML collection type '{type}'.")
                };
            }

            var value = Scalar(mapping["value"]);
            return Scalar(mapping["type"]) switch
            {
                "Decimal" => decimal.Parse(value, CultureInfo.InvariantCulture),
                "Single" => float.Parse(value, CultureInfo.InvariantCulture),
                "Double" => double.Parse(value, CultureInfo.InvariantCulture),
                "Int64" => long.Parse(value, CultureInfo.InvariantCulture),
                "UInt64" => ulong.Parse(value, CultureInfo.InvariantCulture),
                "UInt32" => uint.Parse(value, CultureInfo.InvariantCulture),
                "Int16" => short.Parse(value, CultureInfo.InvariantCulture),
                "UInt16" => ushort.Parse(value, CultureInfo.InvariantCulture),
                "Byte" => byte.Parse(value, CultureInfo.InvariantCulture),
                "SByte" => sbyte.Parse(value, CultureInfo.InvariantCulture),
                "Char" when value.Length == 1 => value[0],
                "DateTime" => DateTime.Parse(value, CultureInfo.InvariantCulture),
                "TimeSpan" => TimeSpan.Parse(value, CultureInfo.InvariantCulture),
                "Guid" => Guid.Parse(value),
                var type => throw new InvalidDataException($"Unsupported YAML value type '{type}'.")
            };
        }

        var scalar = (YamlScalarNode)node;
        if (IsNull(scalar)) return null;
        var text = scalar.Value!;
        if (scalar.Style is ScalarStyle.SingleQuoted or ScalarStyle.DoubleQuoted or ScalarStyle.Literal
            or ScalarStyle.Folded)
            return text;
        if (bool.TryParse(text, out var boolean)) return boolean;
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer)) return integer;
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)) return number;
        return text;
    }

    private static bool IsNull(YamlNode node) => node is YamlScalarNode
    {
        Style: ScalarStyle.Plain or ScalarStyle.Any, Value: null or "" or "null" or "~"
    };

    private static string Scalar(YamlNode node) => ((YamlScalarNode)node).Value ?? "";

    private static void CheckKeys(YamlMappingNode mapping, params string[] allowed)
    {
        foreach (var key in mapping.Children.Keys.Select(Scalar))
        {
            if (!allowed.Contains(key, StringComparer.Ordinal))
                throw new InvalidDataException($"Unknown YAML test data property '{key}'.");
        }
    }

    private sealed class StringComparerConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(StringComparer);

        public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) =>
            parser.Consume<Scalar>().Value switch
            {
                "ordinal" => StringComparer.Ordinal,
                "ordinalIgnoreCase" => StringComparer.OrdinalIgnoreCase,
                "currentCulture" => StringComparer.CurrentCulture,
                "currentCultureIgnoreCase" => StringComparer.CurrentCultureIgnoreCase,
                "invariantCulture" => StringComparer.InvariantCulture,
                "invariantCultureIgnoreCase" => StringComparer.InvariantCultureIgnoreCase,
                var name => throw new InvalidDataException($"Unknown string comparer '{name}'.")
            };

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer) =>
            throw new NotSupportedException();
    }
}