﻿using NCalc.Cache;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Extensions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Visitors;

namespace NCalc;

public class AsyncExpression
{
    /// <summary>
    /// Static property to enable or disable cache.
    /// Default Value: True
    /// </summary>
    public static bool CacheEnabled { get; set; } = true;
    
    /// <summary>
    /// Event triggered to handle async function evaluation.
    /// </summary>
    public event AsyncEvaluateFunctionHandler? EvaluateFunctionAsync
    {
        add => AsyncEvaluationVisitor.EvaluateFunctionAsync += value;
        remove => AsyncEvaluationVisitor.EvaluateFunctionAsync -= value;
    }

    /// <summary>
    /// Event triggered to handle async parameter evaluation.
    /// </summary>
    public event AsyncEvaluateParameterHandler? EvaluateParameterAsync
    {
        add => AsyncEvaluationVisitor.EvaluateParameterAsync += value;
        remove => AsyncEvaluationVisitor.EvaluateParameterAsync -= value;
    }
    
    
    /// <summary>
    /// Options for the expression evaluation.
    /// </summary>
    public ExpressionOptions Options
    {
        get => Context.Options;
        set => Context.Options = value;
    }

    /// <summary>
    /// Culture information for the expression evaluation.
    /// </summary>
    public CultureInfo CultureInfo
    {
        get => Context.CultureInfo;
        set => Context.CultureInfo = value;
    }

    private ExpressionContext _context = null!;

    protected ExpressionContext Context
    {
        get => _context;
        set
        {
            _context = value;
            AsyncEvaluationVisitor.Context = value;
        }
    }

    /// <summary>
    /// Parameters for the expression evaluation.
    /// </summary>
    public Dictionary<string, object?> Parameters
    {
        get => Context.Parameters;
        set => Context.Parameters = value;
    }

    /// <summary>
    /// Textual representation of the expression.
    /// </summary>
    public string? ExpressionString { get; protected init; }

    public LogicalExpression? LogicalExpression { get; protected set; }

    public Exception? Error { get; private set; }

    protected ILogicalExpressionCache LogicalExpressionCache { get; init; }

    protected ILogicalExpressionFactory LogicalExpressionFactory { get; init; }

    protected IAsyncEvaluationVisitor AsyncEvaluationVisitor { get; init; }
    
    protected IParameterExtractionVisitor ParameterExtractionVisitor { get; init; }
    
    
    private AsyncExpression()
    {
        LogicalExpressionCache = Cache.LogicalExpressionCache.GetInstance();
        LogicalExpressionFactory = Factories.LogicalExpressionFactory.GetInstance();
        ParameterExtractionVisitor = new ParameterExtractionVisitor();
        AsyncEvaluationVisitor = new AsyncEvaluationVisitor();
    }

    protected AsyncExpression(
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache,
        IAsyncEvaluationVisitor evaluationVisitor,
        IParameterExtractionVisitor parameterExtractionVisitor)
    {
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
        AsyncEvaluationVisitor = evaluationVisitor;
        ParameterExtractionVisitor = parameterExtractionVisitor;
    }

    public AsyncExpression(string expression, ExpressionContext? context = null) : this()
    {
        Context = context ?? new();
        ExpressionString = expression;
    }


    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, ExpressionContext have implicit conversions.
    public AsyncExpression(string expression) : this(expression, ExpressionOptions.None)
    {
    }

    public AsyncExpression(string expression, ExpressionOptions options = ExpressionOptions.None,
        CultureInfo? cultureInfo = null) : this(expression, new ExpressionContext(options, cultureInfo))
    {
    }

    public AsyncExpression(LogicalExpression logicalExpression, ExpressionContext? context = null) : this()
    {
        Context = context ?? new();
        LogicalExpression = logicalExpression ?? throw new
            ArgumentException("Expression can't be null", nameof(logicalExpression));
    }

    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, ExpressionContext have implicit conversions.
    public AsyncExpression(LogicalExpression logicalExpression) : this(logicalExpression, ExpressionOptions.None)
    {
    }

    public AsyncExpression(LogicalExpression logicalExpression, ExpressionOptions options = ExpressionOptions.None,
        CultureInfo? cultureInfo = null) : this(logicalExpression, new ExpressionContext(options, cultureInfo))
    {
    }
    
    private LogicalExpression? GetLogicalExpression()
    {
        if (string.IsNullOrEmpty(ExpressionString))
            throw new NCalcException("Expression cannot be null or empty.");

        var isCacheEnabled = CacheEnabled && !Options.HasFlag(ExpressionOptions.NoCache);

        LogicalExpression? logicalExpression = null;

        if (isCacheEnabled && LogicalExpressionCache.TryGetValue(ExpressionString!, out logicalExpression))
            return logicalExpression!;

        try
        {
            logicalExpression = LogicalExpressionFactory.Create(ExpressionString!, Context);
            if (isCacheEnabled)
                LogicalExpressionCache.Set(ExpressionString!, logicalExpression);
        }
        catch (Exception exception)
        {
            Error = exception;
        }

        return logicalExpression;
    }
    
    /// <summary>
    /// Asynchronous evaluate the expression and return the Task.
    /// </summary>
    /// <returns>The result of the evaluation.</returns>
    public async Task<object?> EvaluateAsync()
    {
        LogicalExpression ??= GetLogicalExpression();

        if (Error is not null)
            throw Error;

        if (Options.HasFlag(ExpressionOptions.AllowNullParameter))
            AsyncEvaluationVisitor.Context.Parameters["null"] = null;

        // If array evaluation, execute the same expression multiple times
        if (Options.HasFlag(ExpressionOptions.IterateParameters))
            return await IterateParametersAsync();

        await LogicalExpression!.AcceptAsync(AsyncEvaluationVisitor);
        return AsyncEvaluationVisitor.Result;
    }
    
    private async Task<List<object?>> IterateParametersAsync()
    {
        var parameterEnumerators = new Dictionary<string, IEnumerator>();
        int? size = null;
        var results = new List<object?>();

        foreach (var parameter in Parameters.Values)
        {
            if (parameter is not IEnumerable enumerable)
                continue;

            var localsize = enumerable.Cast<object>().Count();

            if (size == null)
                size = localsize;
            else if (localsize != size)
                throw new NCalcException(
                    "When IterateParameters option is used, IEnumerable parameters must have the same number of items");
        }

        try
        {
            foreach (var key in Parameters.Keys)
            {
                if (Parameters[key] is IEnumerable parameter)
                {
                    parameterEnumerators[key] = parameter.GetEnumerator();
                }
            }

            for (var i = 0; i < size; i++)
            {
                foreach (var kvp in parameterEnumerators)
                {
                    kvp.Value.MoveNext();
                    Parameters[kvp.Key] = kvp.Value.Current;
                }

                await LogicalExpression!.AcceptAsync(AsyncEvaluationVisitor);
                results.Add(AsyncEvaluationVisitor.Result);
            }

            return results;
        }
        finally
        {
            foreach (var enumerator in parameterEnumerators.Values)
            {
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
    
    /// <summary>
    /// Returns a list with all parameters names from the expression.
    /// </summary>
    public List<string> GetParametersNames()
    {
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, Context);
        LogicalExpression.Accept(ParameterExtractionVisitor);
        return ParameterExtractionVisitor.Parameters.ToList();
    }
}