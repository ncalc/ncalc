using NCalc.Cache;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Visitors;
using System.Diagnostics.CodeAnalysis;
using NCalc.Handlers;
using NCalc.Services;

namespace NCalc;

public class AsyncExpression
{
    /// <summary>
    /// Static property to enable or disable cache.
    /// Default Value: True
    /// </summary>
    public static bool CacheEnabled { get; set; } = true;
    
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
    
    protected AsyncExpressionContext Context { get; init; }

    /// <summary>
    /// Parameters for the expression evaluation.
    /// </summary>
    public IDictionary<string, object?> Parameters
    {
        get => Context.StaticParameters;
        set => Context.StaticParameters = value;
    }


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
        add => EvaluationService.EvaluateFunctionAsync += value;
        remove => EvaluationService.EvaluateFunctionAsync -= value;
    }
    
    /// <summary>
    /// Event triggered to handle parameter evaluation.
    /// </summary>
    public event AsyncEvaluateParameterHandler EvaluateParameterAsync
    {
        add => EvaluationService.EvaluateParameterAsync += value;
        remove => EvaluationService.EvaluateParameterAsync -= value;
    }
    
    /// <summary>
    /// Textual representation of the expression.
    /// </summary>
    public string? ExpressionString { get; protected init; }

    public LogicalExpression? LogicalExpression { get; protected set; }

    public Exception? Error { get; private set; }

    protected ILogicalExpressionCache LogicalExpressionCache { get;}

    protected ILogicalExpressionFactory LogicalExpressionFactory { get; }

    protected IAsyncEvaluationService EvaluationService { get; }
    
    
    private AsyncExpression(AsyncExpressionContext? context = null)
    {
        LogicalExpressionCache = Cache.LogicalExpressionCache.GetInstance();
        LogicalExpressionFactory = Factories.LogicalExpressionFactory.GetInstance();
        EvaluationService = new AsyncEvaluationService();
        Context = context ?? new();
    }

    public AsyncExpression(
        string expression,
        AsyncExpressionContext context,
        ILogicalExpressionFactory factory,
        ILogicalExpressionCache cache,
        IAsyncEvaluationService evaluationService)
    {
        ExpressionString = expression;
        Context = context;
        LogicalExpressionCache = cache;
        EvaluationService = evaluationService;
        LogicalExpressionFactory = factory;
    }
    
    public AsyncExpression(
        LogicalExpression logicalExpression,
        AsyncExpressionContext context,
        ILogicalExpressionFactory factory,
        ILogicalExpressionCache cache,
        IAsyncEvaluationService evaluationService)
    {
        LogicalExpression = logicalExpression;
        Context = context;
        LogicalExpressionCache = cache;
        EvaluationService = evaluationService;
        LogicalExpressionFactory = factory;
    }
    
    public AsyncExpression(string expression, AsyncExpressionContext? context = null) : this(context)
    {
        ExpressionString = expression;
    }


    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, ExpressionContext have implicit conversions.
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
    // Reason: False positive, ExpressionContext have implicit conversions.
    public AsyncExpression(LogicalExpression logicalExpression) : this(logicalExpression, ExpressionOptions.None)
    {
    }

    public AsyncExpression(LogicalExpression logicalExpression, ExpressionOptions options = ExpressionOptions.None,
        CultureInfo? cultureInfo = null) : this(logicalExpression, new AsyncExpressionContext(options, cultureInfo))
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
    /// Create the LogicalExpression in order to check syntax errors.
    /// If errors are detected, the Error property contains the exception.
    /// </summary>
    /// <returns>True if the expression syntax is correct, otherwise False.</returns>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool HasErrors()
    {
        try
        {
            LogicalExpression = LogicalExpressionFactory.Create(ExpressionString!, Context);

            // In case HasErrors() is called multiple times for the same expression
            return LogicalExpression != null && Error != null;
        }
        catch (Exception exception)
        {
            Error = exception;
            return true;
        }
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
            Context.StaticParameters["null"] = null;

        // If array evaluation, execute the same expression multiple times
        if (Options.HasFlag(ExpressionOptions.IterateParameters))
            return await IterateParametersAsync();

        return await EvaluationService.EvaluateAsync(LogicalExpression!, Context);
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
                
                results.Add(await EvaluationService.EvaluateAsync(LogicalExpression!, Context));
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
        var parameterExtractionVisitor = new ParameterExtractionVisitor();
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, Context);
        LogicalExpression.Accept(parameterExtractionVisitor);
        return parameterExtractionVisitor.Parameters.ToList();
    }
}