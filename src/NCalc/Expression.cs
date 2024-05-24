using NCalc.Cache;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Visitors;

namespace NCalc;


/// <summary>
/// NCalc principal class. 
/// </summary>
public class Expression
{
    /// <summary>
    /// Static property to enable or disable cache.
    /// </summary>
    public static bool CacheEnabled { get; set; }
    
    public event EvaluateParameterHandler EvaluateParameter
    {
        add => EvaluationVisitor.EvaluateParameter += value;
        remove => EvaluationVisitor.EvaluateParameter -= value;
    }

    public event EvaluateFunctionHandler EvaluateFunction 
    {
        add => EvaluationVisitor.EvaluateFunction += value;
        remove => EvaluationVisitor.EvaluateFunction -= value;
    }
    
    private ExpressionOptions _options;
    public ExpressionOptions Options
    {
        get => _options;
        set
        {
            _options = value;
            EvaluationVisitor.Options = value;
        }
    }
    

    public CultureInfo CultureInfo
    {
        get => EvaluationVisitor.CultureInfo;
        set => EvaluationVisitor.CultureInfo = value;
    }
    
    public Dictionary<string, object?> Parameters
    {
        get => EvaluationVisitor.Parameters;
        set => EvaluationVisitor.Parameters = value;
    }

    /// <summary>
    /// Textual representation of the expression.
    /// </summary>
    public string? ExpressionString { get; protected set; }

    public LogicalExpression? LogicalExpression { get; protected set; }
    
    public Exception? Error { get; private set; }
    
    protected Dictionary<string, IEnumerator>? ParameterEnumerators;

    protected virtual ILogicalExpressionCache LogicalExpressionCache { get; } = LogicalExpressionCacheWrapper.GetInstance();
    
    protected virtual ILogicalExpressionFactory LogicalExpressionFactory { get; } = LogicalExpressionFactoryWrapper.GetInstance();

    protected virtual IEvaluationVisitor EvaluationVisitor { get; } = new EvaluationVisitor
    {
        Parameters = new Dictionary<string, object?>()
    };
    
    protected virtual IParameterExtractionVisitor ParameterExtractionVisitor { get; } = new ParameterExtractionVisitor();

    protected Expression()
    {
        
    }
    
    public Expression(string expression, ExpressionContext? context = null)
    {
        Options = context?.Options ?? ExpressionOptions.None;
        CultureInfo = context?.CultureInfo ?? CultureInfo.CurrentCulture;
        ExpressionString = expression;
    }
    
    
    public Expression(string expression) : this(expression, ExpressionOptions.None)
    {
     
    }
    
    public Expression(string expression, ExpressionOptions options = ExpressionOptions.None, CultureInfo? cultureInfo = null) : this(expression,new ExpressionContext(options, cultureInfo))
    {

    }

    public Expression(LogicalExpression logicalExpression, ExpressionContext? context = null)
    {
        LogicalExpression = logicalExpression ?? throw new
            ArgumentException("Expression can't be null", nameof(logicalExpression));
        Options = context?.Options ?? ExpressionOptions.None;
        CultureInfo = context?.CultureInfo ?? CultureInfo.CurrentCulture;
    }
    
    public Expression(LogicalExpression logicalExpression) : this(logicalExpression, ExpressionOptions.None)
    {
     
    }
    
    public Expression(LogicalExpression logicalExpression, ExpressionOptions options = ExpressionOptions.None, CultureInfo? cultureInfo = null) : this(logicalExpression, new ExpressionContext(options, cultureInfo))
    {

    }
    
    public bool IsCacheEnabled()
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
        
        //Yes, this is the first time in my life I need a bitwise AND, because logicalExpression needs to be declared.
        if (IsCacheEnabled() & LogicalExpressionCache.TryGetValue(ExpressionString!, out var logicalExpression))
            return logicalExpression!;

        try
        {
            logicalExpression = LogicalExpressionFactory.Create(ExpressionString!, Options);
            if(IsCacheEnabled())
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

        ParameterEnumerators = new Dictionary<string, IEnumerator>();

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
                ParameterEnumerators.Add(key,parameter.GetEnumerator());
            }
        }

        var results = new List<object?>();
        for (var i = 0; i < size; i++)
        {
            foreach (var key in ParameterEnumerators.Keys)
            {
                var enumerator = ParameterEnumerators[key];
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
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, Options);
        LogicalExpression.Accept(ParameterExtractionVisitor);
        return ParameterExtractionVisitor.Parameters.ToList();
    }
}