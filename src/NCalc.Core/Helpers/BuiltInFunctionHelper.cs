using System.Collections.Frozen;
using NCalc.Exceptions;
using NCalc.Handlers;

namespace NCalc.Helpers;

public static class BuiltInFunctionHelper
{
    private static readonly BuiltInFunctionDefinition[] Definitions =
    [
        Unary("Abs", static (value, data) => MathHelper.Abs(value, data.EvaluationOptions.Math, data.CultureInfo)),
        Unary("Acos", static (value, data) => MathHelper.Acos(value, data.CultureInfo)),
        Unary("Asin", static (value, data) => MathHelper.Asin(value, data.CultureInfo)),
        Unary("Atan", static (value, data) => MathHelper.Atan(value, data.CultureInfo)),
        Binary("Atan2", static (left, right, data) => MathHelper.Atan2(left, right, data.CultureInfo)),
        Unary("Ceiling", static (value, data) => MathHelper.Ceiling(value, data.EvaluationOptions.Math, data.CultureInfo)),
        Unary("Cos", static (value, data) => MathHelper.Cos(value, data.CultureInfo)),
        Unary("Exp", static (value, data) => MathHelper.Exp(value, data.CultureInfo)),
        Unary("Floor", static (value, data) => MathHelper.Floor(value, data.EvaluationOptions.Math, data.CultureInfo)),
        Binary("IEEERemainder", static (left, right, data) => MathHelper.IEEERemainder(left, right, data.CultureInfo)),
        Unary("Ln", static (value, data) => MathHelper.Ln(value, data.CultureInfo)),
        Binary("Log", static (left, right, data) => MathHelper.Log(left, right, data.CultureInfo)),
        Unary("Log10", static (value, data) => MathHelper.Log10(value, data.CultureInfo)),
        Binary("Pow", static (left, right, data) => MathHelper.Pow(left, right, data.EvaluationOptions.Math, data.CultureInfo)),
        Binary("Round", static (left, right, data) => MathHelper.Round(left, right, data.EvaluationOptions.Math.MidpointRounding, data.EvaluationOptions.Math, data.CultureInfo)),
        Unary("Sign", static (value, data) => MathHelper.Sign(value, data.EvaluationOptions.Math, data.CultureInfo)),
        Unary("Sin", static (value, data) => MathHelper.Sin(value, data.CultureInfo)),
        Unary("Sqrt", static (value, data) => MathHelper.Sqrt(value, data.CultureInfo)),
        Unary("Tan", static (value, data) => MathHelper.Tan(value, data.CultureInfo)),
        Unary("Truncate", static (value, data) => MathHelper.Truncate(value, data.EvaluationOptions.Math, data.CultureInfo)),
        Binary("Max", static (left, right, data) => MathHelper.Max(left, right, data.EvaluationOptions.Math, data.CultureInfo)),
        Binary("Min", static (left, right, data) => MathHelper.Min(left, right, data.EvaluationOptions.Math, data.CultureInfo)),
        new("if", EvaluateIf, EvaluateIfAsync),
        new("in", EvaluateIn, EvaluateInAsync),
        new("ifs", EvaluateIfs, EvaluateIfsAsync),
        Unary("EscapeLike", static (value, data) =>
            LikeOperatorHelper.EscapeLike(Convert.ToString(value, data.CultureInfo) ?? string.Empty))
    ];

    private static readonly IReadOnlyList<string> BuiltInFunctionNames = Array.AsReadOnly([..Definitions.Select(static definition => definition.Name)]);

    private static readonly FrozenDictionary<string, BuiltInFunctionDefinition> CaseSensitiveDefinitions =
        Definitions.ToDictionary(static definition => definition.Name).ToFrozenDictionary(StringComparer.Ordinal);

    private static readonly FrozenDictionary<string, BuiltInFunctionDefinition> CaseInsensitiveDefinitions =
        Definitions.ToDictionary(static definition => definition.Name).ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<string> GetBuiltInFunctionNames() => BuiltInFunctionNames;

    public static object? Evaluate(
        string functionName,
        FunctionData functionData)
    {
        var definitions = functionData.EvaluationOptions.IgnoreCaseAtBuiltInFunctions
            ? CaseInsensitiveDefinitions
            : CaseSensitiveDefinitions;

        if (definitions.TryGetValue(functionName, out var definition))
            return definition.EvaluateFunc(functionData);

        throw new NCalcFunctionNotFoundException(functionName);
    }

    public static async ValueTask<object?> EvaluateAsync(
        string functionName,
        FunctionData functionData)
    {
        var definitions = functionData.EvaluationOptions.IgnoreCaseAtBuiltInFunctions
            ? CaseInsensitiveDefinitions
            : CaseSensitiveDefinitions;

        if (definitions.TryGetValue(functionName, out var definition))
            return await definition.EvaluateFuncAsync(functionData);

        throw new NCalcFunctionNotFoundException(functionName);
    }

    private static BuiltInFunctionDefinition Unary(string name, UnaryFunction evaluator)
    {
        return new BuiltInFunctionDefinition(
            name,
            functionData =>
            {
                EnsureArgumentCount(functionData, 1, name);
                return evaluator(functionData.Evaluate(0), functionData);
            },
            async functionData =>
            {
                EnsureArgumentCount(functionData, 1, name);
                return evaluator(await functionData.EvaluateAsync(0), functionData);
            });
    }

    private static BuiltInFunctionDefinition Binary(string name, BinaryFunction evaluator)
    {
        return new BuiltInFunctionDefinition(
            name,
            functionData =>
            {
                EnsureArgumentCount(functionData, 2, name);
                return evaluator(functionData.Evaluate(0), functionData.Evaluate(1), functionData);
            },
            async functionData =>
            {
                EnsureArgumentCount(functionData, 2, name);
                return evaluator(
                    await functionData.EvaluateAsync(0),
                    await functionData.EvaluateAsync(1),
                    functionData);
            });
    }

    private static void EnsureArgumentCount(FunctionData functionData, int expectedCount, string functionName)
    {
        if (functionData.Count != expectedCount)
            throw new NCalcEvaluationException($"{functionName}() takes exactly {expectedCount} argument{(expectedCount == 1 ? string.Empty : "s")}");
    }

    private static object? EvaluateIf(FunctionData functionData)
    {
        EnsureArgumentCount(functionData, 3, "if");

        var condition = Convert.ToBoolean(functionData.Evaluate(0), functionData.CultureInfo);
        return condition ? functionData.Evaluate(1) : functionData.Evaluate(2);
    }

    private static async ValueTask<object?> EvaluateIfAsync(FunctionData functionData)
    {
        EnsureArgumentCount(functionData, 3, "if");

        var condition = Convert.ToBoolean(await functionData.EvaluateAsync(0), functionData.CultureInfo);
        return condition ? await functionData.EvaluateAsync(1) : await functionData.EvaluateAsync(2);
    }

    private static object? EvaluateIn(FunctionData functionData)
    {
        if (functionData.Count < 2)
            throw new NCalcEvaluationException("in() takes at least 2 arguments");

        var parameter = functionData.Evaluate(0);
        for (var i = 1; i < functionData.Count; i++)
        {
            if (TypeHelper.CompareUsingMostPreciseType(
                    parameter,
                    functionData.Evaluate(i),
                    functionData.EvaluationOptions.StringComparer,
                    functionData.CultureInfo) == ComparisonResult.Equal)
                return true;
        }

        return false;
    }

    private static async ValueTask<object?> EvaluateInAsync(FunctionData functionData)
    {
        if (functionData.Count < 2)
            throw new NCalcEvaluationException("in() takes at least 2 arguments");

        var parameter = await functionData.EvaluateAsync(0);
        for (var i = 1; i < functionData.Count; i++)
        {
            if (TypeHelper.CompareUsingMostPreciseType(
                    parameter,
                    await functionData.EvaluateAsync(i),
                    functionData.EvaluationOptions.StringComparer,
                    functionData.CultureInfo) == ComparisonResult.Equal)
                return true;
        }

        return false;
    }

    private static object? EvaluateIfs(FunctionData functionData)
    {
        EnsureIfsArgumentCount(functionData);

        for (var i = 0; i < functionData.Count; i += 2)
        {
            if (i == functionData.Count - 1)
                return functionData.Evaluate(i);

            if (Convert.ToBoolean(functionData.Evaluate(i), functionData.CultureInfo))
                return functionData.Evaluate(i + 1);
        }

        return null;
    }

    private static async ValueTask<object?> EvaluateIfsAsync(FunctionData functionData)
    {
        EnsureIfsArgumentCount(functionData);

        for (var i = 0; i < functionData.Count; i += 2)
        {
            if (i == functionData.Count - 1)
                return await functionData.EvaluateAsync(i);

            if (Convert.ToBoolean(await functionData.EvaluateAsync(i), functionData.CultureInfo))
                return await functionData.EvaluateAsync(i + 1);
        }

        return null;
    }

    private static void EnsureIfsArgumentCount(FunctionData functionData)
    {
        if (functionData.Count < 3 || functionData.Count % 2 != 1)
            throw new NCalcEvaluationException("ifs() takes at least 3 arguments, or an odd number of arguments");
    }

    private delegate object? UnaryFunction(object? value, FunctionData functionData);
    private delegate object? BinaryFunction(object? left, object? right, FunctionData functionData);

    private sealed class BuiltInFunctionDefinition(
        string name,
        Func<FunctionData, object?> evaluate,
        Func<FunctionData, ValueTask<object?>> evaluateAsync)
    {
        public string Name { get; } = name;
        public Func<FunctionData, object?> EvaluateFunc { get; } = evaluate;
        public Func<FunctionData, ValueTask<object?>> EvaluateFuncAsync { get; } = evaluateAsync;
    }
}
