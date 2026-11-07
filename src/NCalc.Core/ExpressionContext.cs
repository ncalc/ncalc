using System.Collections.Frozen;
using System.Collections.Immutable;
using NCalc.Handlers;

namespace NCalc;

/// <summary>
/// Per-evaluation runtime state, including parameters, functions, and event handlers.
/// </summary>
public sealed class ExpressionContext
{
    public IDictionary<string, object?> Parameters { get; init; }

    [Obsolete("Please use Parameters instead.")]
    public IDictionary<string, object?> StaticParameters
    {
        get => Parameters;
        init => Parameters = value;
    }

    public IDictionary<string, ExpressionParameter> DynamicParameters { get; init; }
    public IDictionary<string, AsyncExpressionParameter> AsyncParameters { get; init; }
    public IDictionary<string, ExpressionFunction> Functions { get; init; }
    public IDictionary<string, AsyncExpressionFunction> AsyncFunctions { get; init; }

    public EvaluateBinaryHandler? EvaluateBinaryHandler { get; set; }
    public EvaluateBinaryAsyncHandler? EvaluateBinaryAsyncHandler { get; set; }
    public EvaluateParameterHandler? EvaluateParameterHandler { get; set; }
    public EvaluateAsyncParameterHandler? EvaluateAsyncParameterHandler { get; set; }
    public EvaluateFunctionHandler? EvaluateFunctionHandler { get; set; }
    public EvaluateAsyncFunctionHandler? EvaluateAsyncFunctionHandler { get; set; }

    internal ExpressionContext? IterationSource { get; private init; }

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

        Parameters = CopyDictionary(context.Parameters);
        DynamicParameters = CopyDictionary(context.DynamicParameters);
        AsyncParameters = CopyDictionary(context.AsyncParameters);
        Functions = CopyDictionary(context.Functions);
        AsyncFunctions = CopyDictionary(context.AsyncFunctions);

        CopyHandlersFrom(context);
    }

    internal ExpressionContext CreateIterationContext()
    {
        var iteration = new ExpressionContext(CopyDictionary(Parameters, writable: true),
            DynamicParameters, Functions, AsyncFunctions, AsyncParameters)
        {
            IterationSource = IterationSource ?? this
        };
        iteration.CopyHandlersFrom(this);
        return iteration;
    }

    internal ExpressionContext CreateChildIterationContext(ExpressionContext child)
    {
        // A stored expression sharing the original context must see this row,
        // without copying scalar values or adding its handlers to that original.
        if (ReferenceEquals(child, IterationSource) || ReferenceEquals(child, this))
            return this;

        var result = new ExpressionContext(CopyDictionary(child.Parameters, writable: true),
            CopyDictionary(child.DynamicParameters, writable: true), child.Functions,
            child.AsyncFunctions, CopyDictionary(child.AsyncParameters, writable: true))
        {
            IterationSource = child.IterationSource ?? child
        };
        result.CopyHandlersFrom(child);
        foreach (var parameter in Parameters)
            result.Parameters[parameter.Key] = parameter.Value;
        foreach (var parameter in DynamicParameters)
            result.DynamicParameters[parameter.Key] = parameter.Value;
        foreach (var parameter in AsyncParameters)
            result.AsyncParameters[parameter.Key] = parameter.Value;

        result.EvaluateFunctionHandler += EvaluateFunctionHandler;
        result.EvaluateAsyncFunctionHandler += EvaluateAsyncFunctionHandler;
        result.EvaluateParameterHandler += EvaluateParameterHandler;
        result.EvaluateAsyncParameterHandler += EvaluateAsyncParameterHandler;
        result.EvaluateBinaryHandler += EvaluateBinaryHandler;
        result.EvaluateBinaryAsyncHandler += EvaluateBinaryAsyncHandler;
        return result;
    }

    private void CopyHandlersFrom(ExpressionContext context)
    {
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
        Parameters = parameters ?? new Dictionary<string, object?>();
        DynamicParameters = dynamicParameters ?? new Dictionary<string, ExpressionParameter>();
        AsyncParameters = asyncParameters ?? new Dictionary<string, AsyncExpressionParameter>();
        Functions = functions ?? new Dictionary<string, ExpressionFunction>();
        AsyncFunctions = asyncFunctions ?? new Dictionary<string, AsyncExpressionFunction>();
    }

    private static IDictionary<string, TValue> CopyDictionary<TValue>(IDictionary<string, TValue> source, bool writable = false)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source switch
        {
            Dictionary<string, TValue> d => new Dictionary<string, TValue>(d, d.Comparer),
#if NET
            ConcurrentDictionary<string, TValue> d => new ConcurrentDictionary<string, TValue>(d, d.Comparer),
#else
            // The comparer cannot be retrieved on .NET Standard.
            ConcurrentDictionary<string, TValue> d => new ConcurrentDictionary<string, TValue>(d),
#endif
            ImmutableDictionary<string, TValue> d => writable ? d.ToBuilder() : d.ToBuilder().ToImmutable(),
            ImmutableDictionary<string, TValue>.Builder d => d.ToImmutable().ToBuilder(),
            FrozenDictionary<string, TValue> d => writable ? new Dictionary<string, TValue>(d, d.Comparer) : d.ToFrozenDictionary(d.Comparer),
#if NET9_0_OR_GREATER
            OrderedDictionary<string, TValue> d => new OrderedDictionary<string, TValue>(d, d.Comparer),
#endif
            SortedDictionary<string, TValue> d => new SortedDictionary<string, TValue>(d, d.Comparer),
            SortedList<string, TValue> d => new SortedList<string, TValue>(d, d.Comparer),
            ImmutableSortedDictionary<string, TValue> d => writable ? d.ToBuilder() : d.ToBuilder().ToImmutable(),
            ImmutableSortedDictionary<string, TValue>.Builder d => d.ToImmutable().ToBuilder(),
            _ => new Dictionary<string, TValue>(source)
        };
    }
}
