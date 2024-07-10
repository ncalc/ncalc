using NCalc.Cache;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Visitors;
using System.Diagnostics.CodeAnalysis;
using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Services;

namespace NCalc;

/// <summary>
/// This class represents a mathematical or logical expression that can be evaluated.
/// It supports caching, custom parameter and function evaluation, and options for handling null parameters and iterating over parameter collections.
/// The class manages the parsing, validation, and evaluation of expressions, and provides mechanisms for error detection and reporting.
/// </summary>
public partial class Expression
{
    /// <summary>
    /// Static property to enable or disable cache.
    /// Default Value: True
    /// </summary>
    public static bool CacheEnabled { get; set; } = true;
    
    /// <summary>
    /// Event triggered to handle function evaluation.
    /// </summary>
    public event EvaluateFunctionHandler EvaluateFunction
    {
        add => Context.EvaluateFunctionHandler += value;
        remove => Context.EvaluateFunctionHandler -= value;
    }
    
    /// <summary>
    /// Event triggered to handle parameter evaluation.
    /// </summary>
    public event EvaluateParameterHandler EvaluateParameter
    {
        add => Context.EvaluateParameterHandler += value;
        remove => Context.EvaluateParameterHandler -= value;
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

    protected ExpressionContext Context { get; init; }

    /// <summary>
    /// Parameters for the expression evaluation.
    /// </summary>
    public IDictionary<string, object?> Parameters
    {
        get => Context.StaticParameters;
        set => Context.StaticParameters = value;
    }
    
    public IDictionary<string, ExpressionParameter> DynamicParameters
    {
        get => Context.DynamicParameters;
        set => Context.DynamicParameters = value;
    }
    
    public IDictionary<string, ExpressionFunction> Functions
    {
        get => Context.Functions;
        set => Context.Functions = value;
    }

    /// <summary>
    /// Textual representation of the expression.
    /// </summary>
    public string? ExpressionString { get; protected init; }

    public LogicalExpression? LogicalExpression { get; protected set; }

    public Exception? Error { get; private set; }

    protected ILogicalExpressionCache LogicalExpressionCache { get; }
    protected ILogicalExpressionFactory LogicalExpressionFactory { get; }
    protected IEvaluationService EvaluationService { get; }
    
    private Expression(ExpressionContext? context = null)
    {
        LogicalExpressionCache = Cache.LogicalExpressionCache.GetInstance();
        LogicalExpressionFactory = Factories.LogicalExpressionFactory.GetInstance();
        EvaluationService = new EvaluationService();
        Context = context ?? new();
    }

    public Expression(
        string expression,
        ExpressionContext context,
        ILogicalExpressionFactory factory,
        ILogicalExpressionCache cache,
        IEvaluationService evaluationService)
    {
        ExpressionString = expression;
        Context = context;
        LogicalExpressionCache = cache;
        EvaluationService = evaluationService;
        LogicalExpressionFactory = factory;
    }
    
    public Expression(
        LogicalExpression logicalExpression,
        ExpressionContext context,
        ILogicalExpressionFactory factory,
        ILogicalExpressionCache cache,
        IEvaluationService evaluationService)
    {
        LogicalExpression = logicalExpression;
        Context = context;
        LogicalExpressionCache = cache;
        EvaluationService = evaluationService;
        LogicalExpressionFactory = factory;
    }

    public Expression(string expression, ExpressionContext? context = null) : this(context)
    {
        ExpressionString = expression;
    }
    
    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, ExpressionContext have implicit conversions.
    public Expression(string expression) : this(expression, ExpressionOptions.None)
    {
    }

    public Expression(string expression, ExpressionOptions options = ExpressionOptions.None,
        CultureInfo? cultureInfo = null) : this(expression, new ExpressionContext(options, cultureInfo))
    {
    }

    public Expression(LogicalExpression logicalExpression, ExpressionContext? context = null) : this(context)
    {
        LogicalExpression = logicalExpression ?? throw new
            ArgumentException("Expression can't be null", nameof(logicalExpression));
    }

    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, ExpressionContext have implicit conversions.
    public Expression(LogicalExpression logicalExpression) : this(logicalExpression, ExpressionOptions.None)
    {
    }

    public Expression(LogicalExpression logicalExpression, ExpressionOptions options = ExpressionOptions.None,
        CultureInfo? cultureInfo = null) : this(logicalExpression, new ExpressionContext(options, cultureInfo))
    {
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
            LogicalExpression = LogicalExpressionFactory.Create(ExpressionString!, Context.Options);

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
    /// Evaluate the expression and return the result.
    /// </summary>
    /// <returns>The result of the evaluation.</returns>
    public object? Evaluate()
    {
        LogicalExpression ??= GetLogicalExpression();

        if (Error is not null)
            throw Error;

        if (Options.HasFlag(ExpressionOptions.AllowNullParameter))
            Parameters["null"] = null;

        // If array evaluation, execute the same expression multiple times
        if (Options.HasFlag(ExpressionOptions.IterateParameters))
            return IterateParameters();

        return EvaluationService.Evaluate(LogicalExpression!, Context);
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
            logicalExpression = LogicalExpressionFactory.Create(ExpressionString!, Context.Options);
            if (isCacheEnabled)
                LogicalExpressionCache.Set(ExpressionString!, logicalExpression);
        }
        catch (Exception exception)
        {
            Error = exception;
        }

        return logicalExpression;
    }

    private List<object?> IterateParameters()
    {
        var parameterEnumerators = ParametersHelper.GetEnumerators(Parameters, out var size);

        var results = new List<object?>();

        for (int i = 0; i < size; i++)
        {
            foreach (var kvp in parameterEnumerators)
            {
                kvp.Value.MoveNext();
                Parameters[kvp.Key] = kvp.Value.Current;
            }

            results.Add(EvaluationService.Evaluate(LogicalExpression!, Context));
        }

        return results;
    }

    /// <summary>
    /// Returns a list with all parameters names from the expression.
    /// </summary>
    public List<string> GetParametersNames()
    {
        var parameterExtractionVisitor = new ParameterExtractionVisitor();
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, Context.Options);
        return LogicalExpression.Accept(parameterExtractionVisitor);
    }
}