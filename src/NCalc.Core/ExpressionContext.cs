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

    /// <summary>
    /// Creates a copy of another <see cref="ExpressionContext"/> with independent parameter and function dictionaries.
    /// </summary>
    /// <remarks>
    /// Dictionary entries and event handlers are copied by reference.
    /// </remarks>
    /// <param name="context">The context to copy.</param>
    public ExpressionContext(ExpressionContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        Parameters = CopyDictionary(context.Parameters);
        DynamicParameters = CopyDictionary(context.DynamicParameters);
        AsyncParameters = CopyDictionary(context.AsyncParameters);
        Functions = CopyDictionary(context.Functions);
        AsyncFunctions = CopyDictionary(context.AsyncFunctions);

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

    private static IDictionary<string, TValue> CopyDictionary<TValue>(IDictionary<string, TValue> source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

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
