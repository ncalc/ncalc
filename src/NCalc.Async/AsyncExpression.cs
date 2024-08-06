using NCalc.Cache;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Services;

namespace NCalc;

/// <summary>
/// This class represents a mathematical or logical expression that can be asynchronous evaluated.
/// It supports caching, custom parameter and function evaluation, and options for handling null parameters and iterating over parameter collections.
/// The class manages the parsing, validation, and evaluation of expressions, and provides mechanisms for error detection and reporting.
/// </summary>
public class AsyncExpression : ExpressionBase<AsyncExpressionContext>
{
    public IDictionary<string, AsyncExpressionParameter> DynamicParameters
    {
        get => Context.DynamicParameters;
        set => Context.DynamicParameters = value;
    }

    public IDictionary<string, AsyncExpressionFunction> Functions
    {
        get => Context.Functions;
        set => Context.Functions = value;
    }

    /// <summary>
    /// Event triggered to handle function evaluation.
    /// </summary>
    public event AsyncEvaluateFunctionHandler EvaluateFunctionAsync
    {
        add => Context.AsyncEvaluateFunctionHandler += value;
        remove => Context.AsyncEvaluateFunctionHandler -= value;
    }

    /// <summary>
    /// Event triggered to handle parameter evaluation.
    /// </summary>
    public event AsyncEvaluateParameterHandler EvaluateParameterAsync
    {
        add => Context.AsyncEvaluateParameterHandler += value;
        remove => Context.AsyncEvaluateParameterHandler -= value;
    }

    protected IAsyncEvaluationService EvaluationService { get; }

    private AsyncExpression(AsyncExpressionContext? context = null) : base(context ?? new AsyncExpressionContext())
    {
        EvaluationService = new AsyncEvaluationService();
    }

    public AsyncExpression(
        string expression,
        AsyncExpressionContext context,
        ILogicalExpressionFactory factory,
        ILogicalExpressionCache cache,
        IAsyncEvaluationService evaluationService) : base(expression, context, factory, cache)
    {
        EvaluationService = evaluationService;
    }

    public AsyncExpression(
        LogicalExpression logicalExpression,
        AsyncExpressionContext context,
        ILogicalExpressionFactory factory,
        ILogicalExpressionCache cache,
        IAsyncEvaluationService evaluationService) : base(logicalExpression, context, factory, cache)
    {
        EvaluationService = evaluationService;
    }

    public AsyncExpression(string expression, AsyncExpressionContext? context = null) : this(context)
    {
        ExpressionString = expression;
    }

    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, AsyncExpressionContext have implicit conversions.
    public AsyncExpression(string expression) : this(expression, ExpressionOptions.None)
    {
    }

    public AsyncExpression(string expression, ExpressionOptions options = ExpressionOptions.None,
        CultureInfo? cultureInfo = null) : this(expression, new AsyncExpressionContext(options, cultureInfo))
    {
    }

    public AsyncExpression(LogicalExpression logicalExpression, AsyncExpressionContext? context = null) : this(context)
    {
        LogicalExpression = logicalExpression ?? throw new
            ArgumentException("Expression can't be null", nameof(logicalExpression));
    }

    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, AsyncExpressionContext have implicit conversions.
    public AsyncExpression(LogicalExpression logicalExpression) : this(logicalExpression, ExpressionOptions.None)
    {
    }

    public AsyncExpression(LogicalExpression logicalExpression, ExpressionOptions options = ExpressionOptions.None,
        CultureInfo? cultureInfo = null) : this(logicalExpression, new AsyncExpressionContext(options, cultureInfo))
    {
    }

    /// <summary>
    /// Asynchronously evaluates the logical expression.
    /// </summary>
    /// <returns>The result of the evaluation.</returns>
    /// <exception cref="NCalcException">Thrown when there is an error in the expression.</exception>
    public ValueTask<object?> EvaluateAsync()
    {
        LogicalExpression ??= GetLogicalExpression();

        if (Error is not null)
            throw Error;

        if (Options.HasFlag(ExpressionOptions.AllowNullParameter))
            Context.StaticParameters["null"] = null;

        // If array evaluation, execute the same expression multiple times
        if (Options.HasFlag(ExpressionOptions.IterateParameters))
            return IterateParametersAsync();

        return EvaluationService.EvaluateAsync(LogicalExpression!, Context);
    }

    private async ValueTask<object?> IterateParametersAsync()
    {
        var parameterEnumerators = ParametersHelper.GetEnumerators(Parameters, out var size);

        if (size != null)
        {
            var results = new List<object?>();

            for (int i = 0; i < size; i++)
            {
                foreach (var kvp in parameterEnumerators)
                {
                    kvp.Value.MoveNext();
                    Parameters[kvp.Key] = kvp.Value.Current;
                }

                results.Add(await EvaluationService.EvaluateAsync(LogicalExpression!, Context));
            }

            return results;
        }

        return await EvaluationService.EvaluateAsync(LogicalExpression!, Context);
    }
}