using NCalc.Cache;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Visitors;

namespace NCalc;


/// <summary>
/// The Expression class represents a mathematical or logical expression that can be evaluated.
/// It supports caching, custom parameter and function evaluation, and options for handling null parameters and iterating over parameter collections.
/// The class manages the parsing, validation, and evaluation of expressions, and provides mechanisms for error detection and reporting.
/// </summary>
public class Expression
{
    /// <summary>
    /// Static property to enable or disable cache.
    /// Default Value: True
    /// </summary>
    public static bool CacheEnabled { get; set; } = true;
    
    /// <summary>
    /// Event triggered to handle parameter evaluation.
    /// </summary>
    public event EvaluateParameterHandler EvaluateParameter
    {
        add => EvaluationVisitor.EvaluateParameter += value;
        remove => EvaluationVisitor.EvaluateParameter -= value;
    }

    /// <summary>
    /// Event triggered to handle function evaluation.
    /// </summary>
    public event EvaluateFunctionHandler EvaluateFunction 
    {
        add => EvaluationVisitor.EvaluateFunction += value;
        remove => EvaluationVisitor.EvaluateFunction -= value;
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
            EvaluationVisitor.Context = value;
        }
    }

    /// <summary>
    /// Parameters for the expression evaluation.
    /// </summary>
    public Dictionary<string, object?> Parameters
    {
        get => EvaluationVisitor.Parameters;
        set => EvaluationVisitor.Parameters = value;
    }

    /// <summary>
    /// Textual representation of the expression.
    /// </summary>
    public string? ExpressionString { get; protected init; }

    public LogicalExpression? LogicalExpression { get; protected set; }
    
    public Exception? Error { get; private set; }

    protected ILogicalExpressionCache LogicalExpressionCache { get; init; }

    protected ILogicalExpressionFactory LogicalExpressionFactory { get; init; }

    protected IEvaluationVisitor EvaluationVisitor { get; init; }

    protected IParameterExtractionVisitor ParameterExtractionVisitor { get; init; }

    private Expression()
    {
        LogicalExpressionCache = Cache.LogicalExpressionCache.GetInstance();
        LogicalExpressionFactory =  Factories.LogicalExpressionFactory.GetInstance();
        ParameterExtractionVisitor = new ParameterExtractionVisitor();
        EvaluationVisitor = new EvaluationVisitor();
    }
    
    protected Expression(
        ILogicalExpressionFactory logicalExpressionFactory, 
        ILogicalExpressionCache logicalExpressionCache,
        IEvaluationVisitor evaluationVisitor, 
        IParameterExtractionVisitor parameterExtractionVisitor)
    {
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
        EvaluationVisitor = evaluationVisitor;
        ParameterExtractionVisitor = parameterExtractionVisitor;
    }
    
    public Expression(string expression, ExpressionContext? context = null) : this()
    {
        Context = context ?? new();
        ExpressionString = expression;
    }
    
    
    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, ExpressionContext have implicit conversions.
    public Expression(string expression) : this(expression, ExpressionOptions.None)
    {
     
    }
    
    public Expression(string expression, ExpressionOptions options = ExpressionOptions.None, CultureInfo? cultureInfo = null) : this(expression,new ExpressionContext(options, cultureInfo))
    {

    }
    
    public Expression(LogicalExpression logicalExpression, ExpressionContext? context = null) : this()
    {
        Context = context ?? new();
        LogicalExpression = logicalExpression ?? throw new
            ArgumentException("Expression can't be null", nameof(logicalExpression));
    }
    
    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, ExpressionContext have implicit conversions.
    public Expression(LogicalExpression logicalExpression) : this(logicalExpression, ExpressionOptions.None)
    {
     
    }
    
    public Expression(LogicalExpression logicalExpression, ExpressionOptions options = ExpressionOptions.None, CultureInfo? cultureInfo = null) : this(logicalExpression, new ExpressionContext(options, cultureInfo))
    {

    }

    private bool IsCacheEnabled()
    {
        return CacheEnabled && !Options.HasOption(ExpressionOptions.NoCache);
    }
    
    /// <summary>
    /// Create the LogicalExpression in order to check syntax errors.
    /// If errors are detected, the Error property contains the exception.
    /// </summary>
    /// <returns>True if the expression syntax is correct, otherwiser False.</returns>
    public bool HasErrors()
    {
        try
        {
            LogicalExpression = LogicalExpressionFactory.Create(ExpressionString!, Options);

            // In case HasErrors() is called multiple times for the same expression
            return LogicalExpression != null && Error != null;
        }
        catch(Exception exception)
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

        if (Options.HasOption(ExpressionOptions.AllowNullParameter))
            EvaluationVisitor.Parameters["null"] = null;

        // If array evaluation, execute the same expression multiple times
        if (Options.HasOption(ExpressionOptions.IterateParameters))
            return IterateParameters();

        LogicalExpression!.Accept(EvaluationVisitor);
        return EvaluationVisitor.Result;
    }

    private LogicalExpression? GetLogicalExpression()
    {
        if (string.IsNullOrEmpty(ExpressionString))
            throw new NCalcException("Expression cannot be null or empty.");

        var isCacheEnabled = IsCacheEnabled();
        
        LogicalExpression? logicalExpression = null;
        
        if (isCacheEnabled && LogicalExpressionCache.TryGetValue(ExpressionString!, out logicalExpression))
            return logicalExpression!;

        try
        {
            logicalExpression = LogicalExpressionFactory.Create(ExpressionString!, Context);
            if(isCacheEnabled)
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
        var size = -1;

        var parameterEnumerators = new Dictionary<string, IEnumerator>();

        foreach (var parameter in Parameters.Values)
        {
            if (parameter is IEnumerable enumerable)
            {
                var localsize = enumerable.Cast<object>().Count();

                if (size == -1)
                {
                    size = localsize;
                }
                else if (localsize != size)
                {
                    throw new NCalcEvaluationException("When IterateParameters option is used, IEnumerable parameters must have the same number of items");
                }
            }
        }

        foreach (var key in Parameters.Keys)
        {
            if (Parameters[key] is IEnumerable parameter)
            {
                parameterEnumerators.Add(key,parameter.GetEnumerator());
            }
        }

        var results = new List<object?>();
        for (var i = 0; i < size; i++)
        {
            foreach (var key in parameterEnumerators.Keys)
            {
                var enumerator = parameterEnumerators[key];
                enumerator.MoveNext();
                Parameters[key] = enumerator.Current;
            }

            LogicalExpression!.Accept(EvaluationVisitor);
            results.Add(EvaluationVisitor.Result);
        }

        return results;
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