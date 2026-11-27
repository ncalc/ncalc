using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using NCalc.Handlers;

namespace NCalc;

/// <summary>
/// Per-evaluation runtime state, including parameters, functions, and event handlers.
/// </summary>
/// <remarks>
/// The parameter and function dictionaries are created the first time they are read. Most expressions use only one
/// of them, or none, so creating all five up front was the largest allocation of a new <see cref="Expression"/>.
/// </remarks>
public sealed class ExpressionContext
{
    private IDictionary<string, object?>? _parameters;
    private IDictionary<string, ExpressionParameter>? _dynamicParameters;
    private IDictionary<string, AsyncExpressionParameter>? _asyncParameters;
    private IDictionary<string, ExpressionFunction>? _functions;
    private IDictionary<string, AsyncExpressionFunction>? _asyncFunctions;

    public IDictionary<string, object?> Parameters
    {
        get => _parameters ?? GetOrCreate(ref _parameters);
        init => _parameters = value;
    }

    [Obsolete("Please use Parameters instead.")]
    public IDictionary<string, object?> StaticParameters
    {
        get => Parameters;
        init => Parameters = value;
    }

    public IDictionary<string, ExpressionParameter> DynamicParameters
    {
        get => _dynamicParameters ?? GetOrCreate(ref _dynamicParameters);
        init => _dynamicParameters = value;
    }

    public IDictionary<string, AsyncExpressionParameter> AsyncParameters
    {
        get => _asyncParameters ?? GetOrCreate(ref _asyncParameters);
        init => _asyncParameters = value;
    }

    public IDictionary<string, ExpressionFunction> Functions
    {
        get => _functions ?? GetOrCreate(ref _functions);
        init => _functions = value;
    }

    public IDictionary<string, AsyncExpressionFunction> AsyncFunctions
    {
        get => _asyncFunctions ?? GetOrCreate(ref _asyncFunctions);
        init => _asyncFunctions = value;
    }

    public EvaluateBinaryHandler? EvaluateBinaryHandler { get; set; }
    public EvaluateBinaryAsyncHandler? EvaluateBinaryAsyncHandler { get; set; }
    public EvaluateParameterHandler? EvaluateParameterHandler { get; set; }
    public EvaluateAsyncParameterHandler? EvaluateAsyncParameterHandler { get; set; }
    public EvaluateFunctionHandler? EvaluateFunctionHandler { get; set; }
    public EvaluateAsyncFunctionHandler? EvaluateAsyncFunctionHandler { get; set; }

    /// <summary>
    /// Creates a copy of another <see cref="ExpressionContext"/> with independent parameter and function dictionaries.
    /// </summary>
    /// <remarks>
    /// Dictionary entries and event handlers are copied by reference.
    /// </remarks>
    /// <param name="context">The context to copy.</param>
    public ExpressionContext(ExpressionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _parameters = CopyDictionary(context._parameters);
        _dynamicParameters = CopyDictionary(context._dynamicParameters);
        _asyncParameters = CopyDictionary(context._asyncParameters);
        _functions = CopyDictionary(context._functions);
        _asyncFunctions = CopyDictionary(context._asyncFunctions);

        EvaluateBinaryHandler = context.EvaluateBinaryHandler;
        EvaluateBinaryAsyncHandler = context.EvaluateBinaryAsyncHandler;
        EvaluateParameterHandler = context.EvaluateParameterHandler;
        EvaluateAsyncParameterHandler = context.EvaluateAsyncParameterHandler;
        EvaluateFunctionHandler = context.EvaluateFunctionHandler;
        EvaluateAsyncFunctionHandler = context.EvaluateAsyncFunctionHandler;
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    public ExpressionContext(
        IDictionary<string, object?>? parameters = null,
        IDictionary<string, ExpressionParameter>? dynamicParameters = null,
        IDictionary<string, ExpressionFunction>? functions = null,
        IDictionary<string, AsyncExpressionFunction>? asyncFunctions = null,
        IDictionary<string, AsyncExpressionParameter>? asyncParameters = null)
    {
        _parameters = parameters;
        _dynamicParameters = dynamicParameters;
        _asyncParameters = asyncParameters;
        _functions = functions;
        _asyncFunctions = asyncFunctions;
    }

    internal bool TryGetParameter(string name, out object? value)
    {
        if (_parameters is { } parameters)
            return parameters.TryGetValue(name, out value);

        value = null;
        return false;
    }

    internal bool TryGetDynamicParameter(string name, [NotNullWhen(true)] out ExpressionParameter? value) =>
        TryGet(_dynamicParameters, name, out value);

    internal bool TryGetAsyncParameter(string name, [NotNullWhen(true)] out AsyncExpressionParameter? value) =>
        TryGet(_asyncParameters, name, out value);

    internal bool TryGetFunction(string name, [NotNullWhen(true)] out ExpressionFunction? value) =>
        TryGet(_functions, name, out value);

    internal bool TryGetAsyncFunction(string name, [NotNullWhen(true)] out AsyncExpressionFunction? value) =>
        TryGet(_asyncFunctions, name, out value);

    // A lookup in a dictionary that was never created is a miss; it does not create the dictionary.
    private static bool TryGet<TValue>(IDictionary<string, TValue>? dictionary, string name, out TValue? value)
    {
        if (dictionary is not null)
            return dictionary.TryGetValue(name, out value);

        value = default;
        return false;
    }

    /// <summary>
    /// Copies this context's parameters into <paramref name="child"/> so a nested expression can resolve them.
    /// Dictionaries that were never created on either side are left alone.
    /// </summary>
    internal void ShareParametersWith(ExpressionContext child)
    {
        CopyEntries(_parameters, ref child._parameters);
        CopyEntries(_dynamicParameters, ref child._dynamicParameters);
        CopyEntries(_asyncParameters, ref child._asyncParameters);
    }

    /// <summary>
    /// Attaches the handlers of <paramref name="parent"/> to this context until the returned scope is disposed.
    /// </summary>
    internal ParentHandlerScope AttachHandlersFrom(ExpressionContext parent) => new(this, parent);

    private static void CopyEntries<TValue>(IDictionary<string, TValue>? source, ref IDictionary<string, TValue>? target)
    {
        if (source is null || source.Count == 0)
            return;

        var destination = target ?? GetOrCreate(ref target);
        foreach (var entry in source)
            destination[entry.Key] = entry.Value;
    }

    private static IDictionary<string, TValue> GetOrCreate<TValue>(ref IDictionary<string, TValue>? field)
    {
        var created = new Dictionary<string, TValue>();
        return Interlocked.CompareExchange(ref field, created, null) ?? created;
    }

    private static IDictionary<string, TValue>? CopyDictionary<TValue>(IDictionary<string, TValue>? source)
    {
        if (source is null)
            return null;

        return source switch
        {
            Dictionary<string, TValue> d => new Dictionary<string, TValue>(d, d.Comparer),
#if NET
            ConcurrentDictionary<string, TValue> d => new ConcurrentDictionary<string, TValue>(d, d.Comparer),
#else
            // The comparer cannot be retrieved on .NET Standard.
            ConcurrentDictionary<string, TValue> d => new ConcurrentDictionary<string, TValue>(d),
#endif
            ImmutableDictionary<string, TValue> d => d.ToBuilder().ToImmutable(),
            FrozenDictionary<string, TValue> d => d.ToFrozenDictionary(d.Comparer),
#if NET9_0_OR_GREATER
            OrderedDictionary<string, TValue> d => new OrderedDictionary<string, TValue>(d, d.Comparer),
#endif
            SortedDictionary<string, TValue> d => new SortedDictionary<string, TValue>(d, d.Comparer),
            SortedList<string, TValue> d => new SortedList<string, TValue>(d, d.Comparer),
            ImmutableSortedDictionary<string, TValue> d => d.ToBuilder().ToImmutable(),
            _ => new Dictionary<string, TValue>(source)
        };
    }
}
