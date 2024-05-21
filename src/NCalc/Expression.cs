using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Visitors;

namespace NCalc;

public class Expression
{
    public static bool CacheEnabled
    {
        get => LogicalExpressionFactory.EnableCache;
        set => LogicalExpressionFactory.EnableCache = value;
    }
    
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
    
    private CultureInfo _cultureInfo = CultureInfo.CurrentUICulture;
    public CultureInfo CultureInfo
    {
        get => _cultureInfo;
        set
        {
            _cultureInfo = value;
            EvaluationVisitor.CultureInfo = value;
        }
    }

    private Dictionary<string, object?> _parameters = new();
    public Dictionary<string, object?> Parameters
    {
        get => _parameters;
        set
        {
            _parameters = value;
            EvaluationVisitor.Parameters = value;
        }
    }

    /// <summary>
    /// Textual representation of the expression.
    /// </summary>
    public string? ExpressionString { get; protected set; }

    public LogicalExpression? LogicalExpression { get; private set; }
    
    public Exception? Error { get; private set; }
    
    protected EvaluationVisitor EvaluationVisitor { get; set; }
    
    protected Dictionary<string, IEnumerator>? ParameterEnumerators;
    
    public Expression(string expressionString, ExpressionOptions options = ExpressionOptions.None, CultureInfo? cultureInfo = null)
    {
        if (string.IsNullOrEmpty(expressionString))
            throw new
                ArgumentException("Expression can't be empty", nameof(expressionString));

        EvaluationVisitor = new EvaluationVisitor(options, cultureInfo ?? CultureInfo.CurrentCulture)
        {
            Parameters = Parameters
        };
        Options = EvaluationVisitor.Options;
        CultureInfo = EvaluationVisitor.CultureInfo;
        ExpressionString = expressionString;
    }
    
    public Expression(string expressionString, CultureInfo cultureInfo) : this(expressionString, ExpressionOptions.None, cultureInfo)
    {
        
    }
    
    public Expression(LogicalExpression expression, ExpressionOptions options = ExpressionOptions.None, CultureInfo? cultureInfo = null)
    {
        LogicalExpression = expression ?? throw new
            ArgumentException("Expression can't be null", nameof(expression));
        EvaluationVisitor = new EvaluationVisitor(options,cultureInfo ?? CultureInfo.CurrentCulture)
        {
            Parameters = Parameters
        };
        Options = EvaluationVisitor.Options;
        CultureInfo = EvaluationVisitor.CultureInfo;
    }
    
    public Expression(LogicalExpression expression, CultureInfo cultureInfo) : this(expression, ExpressionOptions.None, cultureInfo)
    {
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
            LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, Options);

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
        if (HasErrors() && Error is not null)
            throw Error;

        // If array evaluation, execute the same expression multiple times
        if (Options.HasOption(ExpressionOptions.IterateParameters))
            return IterateParameters();

        LogicalExpression!.Accept(EvaluationVisitor);
        return EvaluationVisitor.Result;
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
        var extractionVisitor = new ParameterExtractionVisitor();
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, Options);
        LogicalExpression.Accept(extractionVisitor);
        return extractionVisitor.Parameters;
    }
}