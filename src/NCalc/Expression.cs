using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using NCalc.Domain;
using NCalc.Parser;

namespace NCalc;

public class Expression
{
    public event EvaluateParameterHandler EvaluateParameter
    {
        add 
        {
            if (EvaluationVisitor != null)
                EvaluationVisitor.EvaluateParameter += value;
        }
        remove 
        {
            if (EvaluationVisitor != null)
                EvaluationVisitor.EvaluateParameter -= value;
        }
    }

    public event EvaluateFunctionHandler EvaluateFunction 
    {
        add 
        {
            if (EvaluationVisitor != null)
                EvaluationVisitor.EvaluateFunction += value;
        }
        remove 
        {
            if (EvaluationVisitor != null)
                EvaluationVisitor.EvaluateFunction -= value;
        }
    }
    
    private EvaluateOptions _options;
    public EvaluateOptions Options
    {
        get => _options;
        set
        {
            _options = value;
            if (EvaluationVisitor != null) 
                EvaluationVisitor.Options = value;
        }
    }

    private Dictionary<string, object> _parameters = new();
    public Dictionary<string, object> Parameters
    {
        get => _parameters;
        set
        {
            _parameters = value;
            if (EvaluationVisitor != null) 
                EvaluationVisitor.Parameters = value;
        }
    }

    /// <summary>
    /// Textual representation of the expression to evaluate.
    /// </summary>
    public string OriginalExpression { get; protected set; }

    /// <summary>
    /// Get or set the culture info.
    /// </summary>
    protected CultureInfo CultureInfo { get; set; }
    
    protected EvaluationVisitor EvaluationVisitor { get; set; }
    
    public Expression(string expression) : this(expression, EvaluateOptions.None, CultureInfo.CurrentCulture)
    {
    }

    public Expression(string expression, CultureInfo cultureInfo) : this(expression, EvaluateOptions.None, cultureInfo)
    {
    }

    public Expression(string expression, EvaluateOptions options) : this(expression, options, CultureInfo.CurrentCulture)
    {
    }

    public Expression(string expression, EvaluateOptions options, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(expression))
            throw new
                ArgumentException("Expression can't be empty", nameof(expression));

        OriginalExpression = expression;
        Options = options;
        CultureInfo = cultureInfo;
        EvaluationVisitor = new EvaluationVisitor(Options, CultureInfo)
        {
            Parameters = Parameters
        };
    }
    
    public Expression(string expression, EvaluationVisitor evaluationVisitor, EvaluateOptions options, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(expression))
            throw new
                ArgumentException("Expression can't be empty", nameof(expression));

        OriginalExpression = expression;
        Options = options;
        CultureInfo = cultureInfo;
        EvaluationVisitor = evaluationVisitor;
    }

    public Expression(LogicalExpression expression) : this(expression, EvaluateOptions.None, CultureInfo.CurrentCulture)
    {
    }

    public Expression(LogicalExpression expression, EvaluateOptions options, CultureInfo cultureInfo)
    {
        ParsedExpression = expression ?? throw new
            ArgumentException("Expression can't be null", nameof(expression));
        Options = options;
        CultureInfo = cultureInfo;
        EvaluationVisitor = new EvaluationVisitor(Options,CultureInfo)
        {
            Parameters = Parameters
        };
    }
    public Expression(LogicalExpression expression, EvaluationVisitor evaluationVisitor, EvaluateOptions options, CultureInfo cultureInfo)
    {
        ParsedExpression = expression ?? throw new
            ArgumentException("Expression can't be null", nameof(expression));
        Options = options;
        CultureInfo = cultureInfo;
        EvaluationVisitor = evaluationVisitor;
    }

    #region Cache management
    private static bool _cacheEnabled = true;
    private static readonly ConcurrentDictionary<string, WeakReference<LogicalExpression>> CompiledExpressions = new();


    public static bool CacheEnabled
    {
        get => _cacheEnabled;
        set
        {
            _cacheEnabled = value;

            if (!CacheEnabled)
            {
                // Clears cache
                CompiledExpressions.Clear();
            }
        }
    }

    /// <summary>
    /// Removed unused entries from cached compiled expression
    /// </summary>
    private static void ClearCache()
    {
        foreach (var kvp in CompiledExpressions)
        {
            if (kvp.Value.TryGetTarget(out _)) 
                continue;

            if (CompiledExpressions.TryRemove(kvp.Key, out _))
                Trace.TraceInformation("Cache entry released: " + kvp.Key);
        }
    }

    #endregion

    /// Method used for backwards compatibility.
    [Obsolete("Please use Compile overload with EvaluateOptions.")]
    public static LogicalExpression Compile(string expression, bool nocache) 
    {
        return Compile(expression, nocache ? EvaluateOptions.NoCache : EvaluateOptions.None);
    }
    
    public static LogicalExpression Compile(string expression, EvaluateOptions options)
    {
        LogicalExpression logicalExpression;

        if (_cacheEnabled && !options.HasOption(EvaluateOptions.NoCache))
        {
            if (CompiledExpressions.TryGetValue(expression, out var wr))
            {
                Trace.TraceInformation("Expression retrieved from cache: " + expression);

                if (wr.TryGetTarget(out var target))
                    return target;
            }
        }
        
        try
        {
            logicalExpression = NCalcParser.Expression.Parse(expression);
        }
        catch(Exception ex)
        {
            //TODO: Handle errors like the old version.
            var message = new StringBuilder(ex.Message);
          

            throw new EvaluationException(message.ToString(), ex);
        }



        if (!_cacheEnabled || options.HasOption(EvaluateOptions.NoCache))
            return logicalExpression;
        
        CompiledExpressions[expression] = new WeakReference<LogicalExpression>(logicalExpression);
            
        ClearCache();

        Trace.TraceInformation("Expression added to cache: " + expression);
        
        return logicalExpression;
    }

    /// <summary>
    /// Pre-compiles the expression in order to check syntax errors.
    /// If errors are detected, the Error property contains the message.
    /// </summary>
    /// <returns>True if the expression syntax is correct, otherwiser False</returns>
    public bool HasErrors()
    {
        try
        {
            ParsedExpression ??= Compile(OriginalExpression, Options);

            // In case HasErrors() is called multiple times for the same expression
            return ParsedExpression != null && Error != null;
        }
        catch(Exception e)
        {
            Error = e.Message;
            return true;
        }
    }

    public string Error { get; private set; }

    public LogicalExpression ParsedExpression { get; private set; }

    protected Dictionary<string, IEnumerator> ParameterEnumerators;

    public object Evaluate()
    {
        if (HasErrors())
            throw new EvaluationException(Error);

        ParsedExpression ??= Compile(OriginalExpression, Options);

        // if array evaluation, execute the same expression multiple times
        if (Options.HasOption(EvaluateOptions.IterateParameters))
        {
            int size = -1;

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
                        throw new EvaluationException("When IterateParameters option is used, IEnumerable parameters must have the same number of items");
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

            var results = new List<object>();
            for (var i = 0; i < size; i++)
            {
                foreach (var key in ParameterEnumerators.Keys)
                {
                    var enumerator = ParameterEnumerators[key];
                    enumerator.MoveNext();
                    Parameters[key] = enumerator.Current;
                }

                ParsedExpression.Accept(EvaluationVisitor);
                results.Add(EvaluationVisitor.Result);
            }

            return results;
        }

        ParsedExpression.Accept(EvaluationVisitor);
        return EvaluationVisitor.Result;

    }

    /// <summary>
    /// Returns an array with all parameters names from the expression.
    /// </summary>
    public string[] GetParametersNames()
    {
        var extractionVisitor = new ParameterExtractionVisitor();
        Compile(OriginalExpression, Options).Accept(extractionVisitor);
        return new List<string>(extractionVisitor.Parameters).ToArray();
    }
}