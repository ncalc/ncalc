using System.Diagnostics.CodeAnalysis;
using NCalc.Cache;
using NCalc.Exceptions;
using NCalc.Extensions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Visitors;

namespace NCalc;

/// <summary>
/// A reusable parsed expression and its immutable parsing and evaluation options.
/// Runtime parameters, functions, and handlers are supplied through <see cref="ExpressionContext"/>.
/// </summary>
public class Expression
{
    /// <summary>
    /// Textual representation of the expression.
    /// </summary>
    public string? ExpressionString { get; protected init; }

    /// <summary>
    /// Parsed logical expression tree.
    /// </summary>
    public LogicalExpression? LogicalExpression { get; set; }

    /// <summary>
    /// Gets the last parsing error, if any.
    /// </summary>
    public Exception? Error { get; private set; }

    /// <summary>
    /// Gets or sets parsing, evaluation, and caching configuration.
    /// </summary>
    public ExpressionConfiguration Configuration { get; set; }

    /// <summary>
    /// Gets the parser options from <see cref="Configuration"/>.
    /// </summary>
    public LogicalExpressionParserOptions ParserOptions => Configuration.Parsing;

    /// <summary>
    /// Gets the evaluation options from <see cref="Configuration"/>.
    /// </summary>
    public ExpressionEvaluationOptions EvaluationOptions => Configuration.Evaluation;

    /// <summary>
    /// Culture information for parsing and evaluation.
    /// </summary>
    public CultureInfo CultureInfo { get; set; }

    /// <summary>
    /// Runtime context that stores parameters, functions, and handlers.
    /// </summary>
    public ExpressionContext Context { get; set; }

    /// <summary>
    /// Static parameters for the expression evaluation.
    /// </summary>
    public IDictionary<string, object?> Parameters
    {
        get => Context.Parameters;
        set => Context.Parameters = value;
    }

    /// <summary>
    /// Replaces <see cref="Configuration"/> with a configuration converted from <see cref="ExpressionOptions"/> flags.
    /// </summary>
    /// <remarks>
    /// Prefer setting <see cref="Configuration"/> directly.
    /// </remarks>
    public ExpressionOptions Options
    {
        set => Configuration = ExpressionConfiguration.FromOptions(value);
    }

    /// <summary>
    /// Dynamic parameters for the expression evaluation.
    /// </summary>
    public IDictionary<string, ExpressionParameter> DynamicParameters => Context.DynamicParameters;

    /// <summary>
    /// Async dynamic parameters for the expression evaluation.
    /// </summary>
    public IDictionary<string, AsyncExpressionParameter> AsyncParameters => Context.AsyncParameters;

    /// <summary>
    /// Custom functions for the expression evaluation.
    /// </summary>
    public IDictionary<string, ExpressionFunction> Functions => Context.Functions;

    /// <summary>
    /// Async custom functions for the expression evaluation.
    /// </summary>
    public IDictionary<string, AsyncExpressionFunction> AsyncFunctions => Context.AsyncFunctions;

    /// <summary>
    /// Event triggered to handle function evaluation.
    /// </summary>
    public event EvaluateFunctionHandler EvaluateFunction
    {
        add => Context.EvaluateFunctionHandler += value;
        remove => Context.EvaluateFunctionHandler -= value;
    }

    /// <summary>
    /// Event triggered to handle binary evaluation.
    /// </summary>
    public event EvaluateBinaryHandler EvaluateBinary
    {
        add => Context.EvaluateBinaryHandler += value;
        remove => Context.EvaluateBinaryHandler -= value;
    }

    /// <summary>
    /// Event triggered to handle async binary evaluation.
    /// </summary>
    public event EvaluateBinaryAsyncHandler EvaluateBinaryAsync
    {
        add => Context.EvaluateBinaryAsyncHandler += value;
        remove => Context.EvaluateBinaryAsyncHandler -= value;
    }

    /// <summary>
    /// Event triggered to handle async function evaluation.
    /// </summary>
    public event EvaluateAsyncFunctionHandler EvaluateAsyncFunction
    {
        add => Context.EvaluateAsyncFunctionHandler += value;
        remove => Context.EvaluateAsyncFunctionHandler -= value;
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
    /// Event triggered to handle async parameter evaluation.
    /// </summary>
    public event EvaluateAsyncParameterHandler EvaluateAsyncParameter
    {
        add => Context.EvaluateAsyncParameterHandler += value;
        remove => Context.EvaluateAsyncParameterHandler -= value;
    }

    private ILogicalExpressionCache LogicalExpressionCache { get; }
    private ILogicalExpressionFactory LogicalExpressionFactory { get; }

    /// <summary>
    /// Factory used to create evaluation visitors.
    /// </summary>
    protected IEvaluationVisitorFactory? EvaluationVisitorFactory { get; private set; }

    protected Expression(IEvaluationVisitorFactory? evaluationVisitorFactory = null)
    {
        LogicalExpressionCache = Cache.LogicalExpressionCache.GetInstance();
        LogicalExpressionFactory = Factories.LogicalExpressionFactory.GetInstance();
        EvaluationVisitorFactory = evaluationVisitorFactory;
        Configuration ??= new ExpressionConfiguration();
        Context ??= new ExpressionContext();
        CultureInfo ??= CultureInfo.CurrentCulture;
    }

    protected internal Expression(
        string expressionString,
        ExpressionConfiguration? configuration,
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache,
        IEvaluationVisitorFactory? evaluationVisitorFactory = null)
        : this(evaluationVisitorFactory)
    {
        ExpressionString = expressionString;
        Configuration = configuration ?? new ExpressionConfiguration();
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
    }

    protected internal Expression(
        LogicalExpression logicalExpression,
        ExpressionConfiguration? configuration,
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache,
        IEvaluationVisitorFactory? evaluationVisitorFactory = null)
        : this(evaluationVisitorFactory)
    {
        LogicalExpression = logicalExpression ?? throw new ArgumentNullException(nameof(logicalExpression));
        Configuration = configuration ?? new ExpressionConfiguration();
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
    }

    public Expression(string? expression, ExpressionConfiguration? configuration = null, CultureInfo? cultureInfo = null, IEvaluationVisitorFactory? evaluationVisitorFactory = null) : this(evaluationVisitorFactory)
    {
        ExpressionString = expression;
        Configuration = configuration ?? new ExpressionConfiguration();
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }

    public Expression(string? expression, CultureInfo cultureInfo) : this()
    {
        ExpressionString = expression;
        CultureInfo = cultureInfo;
    }

    public Expression(string? expression, ExpressionContext context) : this()
    {
        ExpressionString = expression;
        Context = context;
        CultureInfo = CultureInfo.CurrentCulture;
    }

    public Expression(string? expression, ExpressionConfiguration? configuration, ExpressionContext? context, CultureInfo? cultureInfo = null, IEvaluationVisitorFactory? evaluationVisitorFactory = null) : this(evaluationVisitorFactory)
    {
        ExpressionString = expression;
        Context = context ?? new ExpressionContext();
        Configuration = configuration ?? new ExpressionConfiguration();
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }

    public Expression(LogicalExpression logicalExpression, ExpressionConfiguration? configuration = null, CultureInfo? cultureInfo = null, IEvaluationVisitorFactory? evaluationVisitorFactory = null) : this(evaluationVisitorFactory)
    {
        LogicalExpression = logicalExpression ?? throw new ArgumentNullException(nameof(logicalExpression));
        Configuration = configuration ?? new ExpressionConfiguration();
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }

    protected virtual EvaluationVisitor CreateEvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default)
    {
        return EvaluationVisitorFactory?.CreateEvaluationVisitor(context, EvaluationOptions, CultureInfo, cancellationToken)
               ?? new EvaluationVisitor(context, EvaluationOptions, CultureInfo, cancellationToken: cancellationToken);
    }

    protected virtual AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default)
    {
        return EvaluationVisitorFactory?.CreateAsyncEvaluationVisitor(context, EvaluationOptions, CultureInfo, cancellationToken)
               ?? new AsyncEvaluationVisitor(context, EvaluationOptions, CultureInfo, cancellationToken: cancellationToken);
    }

    internal void SetEvaluationVisitorFactory(IEvaluationVisitorFactory? evaluationVisitorFactory)
    {
        EvaluationVisitorFactory ??= evaluationVisitorFactory;
    }

    /// <summary>
    /// Evaluates the logical expression.
    /// </summary>
    /// <exception cref="NCalcException">Thrown when there is an error in the expression.</exception>
    public object? Evaluate(CancellationToken cancellationToken = default)
    {
        LogicalExpression ??= GetLogicalExpression(cancellationToken);

        if (Error is not null)
            throw Error;

        try
        {
            if (EvaluationOptions.IterateParameters)
                return IterateParameters(cancellationToken);

            return LogicalExpression?.Accept(CreateEvaluationVisitor(Context, cancellationToken));
        }
        catch (InvalidCastException exception)
        {
            throw new NCalcEvaluationException("Error evaluating expression.", exception);
        }
    }

    /// <summary>
    /// Evaluates the logical expression and converts the result to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to which the evaluation result should be converted.</typeparam>
    /// <returns>The evaluation result converted to type <typeparamref name="T"/>, or <c>default</c> if the result is <c>null</c>.</returns>
    /// <exception cref="NCalcCastException">Thrown when the result cannot be cast to type <typeparamref name="T"/>.</exception>
    /// <exception cref="NCalcException">Thrown when there is an error in the expression.</exception>
    public T? Evaluate<T>(CancellationToken cancellationToken = default)
    {
        return CastResult<T>(Evaluate(cancellationToken), CultureInfo);
    }

    /// <summary>
    /// Asynchronously evaluates the logical expression.
    /// </summary>
    /// <returns>The result of the evaluation.</returns>
    /// <exception cref="NCalcException">Thrown when there is an error in the expression.</exception>
    public async ValueTask<object?> EvaluateAsync(CancellationToken cancellationToken = default)
    {
        LogicalExpression ??= GetLogicalExpression(cancellationToken);

        if (Error is not null)
            throw Error;

        try
        {
            if (EvaluationOptions.IterateParameters)
                return await IterateParametersAsync(cancellationToken).ConfigureAwait(false);

            if (LogicalExpression is null)
                return null;

            return await LogicalExpression.Accept(CreateAsyncEvaluationVisitor(Context, cancellationToken)).ConfigureAwait(false);
        }
        catch (InvalidCastException exception)
        {
            throw new NCalcEvaluationException("Error evaluating expression.", exception);
        }
    }

    /// <summary>
    /// Asynchronously evaluates the logical expression and converts the result to type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to which the evaluation result should be converted.</typeparam>
    /// <returns>The evaluation result converted to type <typeparamref name="T"/>, or <c>default</c> if the result is <c>null</c>.</returns>
    /// <exception cref="NCalcCastException">Thrown when the result cannot be cast to type <typeparamref name="T"/>.</exception>
    /// <exception cref="NCalcException">Thrown when there is an error in the expression.</exception>
    public async ValueTask<T?> EvaluateAsync<T>(CancellationToken cancellationToken = default)
    {
        return CastResult<T>(await EvaluateAsync(cancellationToken).ConfigureAwait(false), CultureInfo);
    }

    private static T? CastResult<T>(object? result, CultureInfo cultureInfo)
    {
        if (result is null)
            return default;
        if (result is T typed)
            return typed;
        if (result is not IConvertible convertible)
            throw new NCalcCastException(result, typeof(T));

        try
        {
            return (T)Convert.ChangeType(convertible, typeof(T), cultureInfo);
        }
        catch (Exception exception)
        {
            throw new NCalcCastException(result, typeof(T), exception);
        }
    }

    private async ValueTask<object?> IterateParametersAsync(CancellationToken cancellationToken)
    {
        var parameterEnumerators = ParametersHelper.GetEnumerators(Context.Parameters, out var size);
        if (LogicalExpression is null)
            return null;

        var visitor = CreateAsyncEvaluationVisitor(Context, cancellationToken);
        if (size is null)
            return await LogicalExpression.Accept(visitor).ConfigureAwait(false);

        var results = new List<object?>(size.Value);
        for (var i = 0; i < size; i++)
        {
            foreach (var parameter in parameterEnumerators)
            {
                parameter.Value.MoveNext();
                Context.Parameters[parameter.Key] = parameter.Value.Current;
            }

            results.Add(await LogicalExpression.Accept(visitor).ConfigureAwait(false));
        }

        return results;
    }

    private object? IterateParameters(CancellationToken cancellationToken)
    {
        var parameterEnumerators = ParametersHelper.GetEnumerators(Context.Parameters, out var size);
        if (LogicalExpression is null)
            return null;

        var visitor = CreateEvaluationVisitor(Context, cancellationToken);
        if (size is null)
            return LogicalExpression.Accept(visitor);

        var results = new List<object?>(size.Value);
        for (var i = 0; i < size; i++)
        {
            foreach (var parameter in parameterEnumerators)
            {
                parameter.Value.MoveNext();
                Context.Parameters[parameter.Key] = parameter.Value.Current;
            }

            results.Add(LogicalExpression.Accept(visitor));
        }

        return results;
    }

    /// <summary>
    /// Returns all parameter names from the expression.
    /// </summary>
    /// <returns>Parameter names used by the expression.</returns>
    public List<string> GetParameterNames(CancellationToken cancellationToken = default)
    {
        LogicalExpression ??= CreateLogicalExpression(cancellationToken);
        return LogicalExpression.Accept(new ParameterExtractionVisitor());
    }

    private LogicalExpression CreateLogicalExpression(CancellationToken cancellationToken)
    {
        return LogicalExpressionFactory.Create(ExpressionString!, ParserOptions, CultureInfo, cancellationToken);
    }

    /// <summary>
    /// Returns all function names from the expression.
    /// </summary>
    /// <returns>Function names used by the expression.</returns>
    public List<string> GetFunctionNames(CancellationToken cancellationToken = default)
    {
        LogicalExpression ??= CreateLogicalExpression(cancellationToken);
        return LogicalExpression.Accept(new FunctionExtractionVisitor());
    }

    /// <summary>
    /// Creates the logical expression to check for syntax errors.
    /// </summary>
    /// <returns><c>true</c> if errors were detected; otherwise, <c>false</c>.</returns>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool HasErrors(CancellationToken cancellationToken = default)
    {
        try
        {
            Error = null;
            LogicalExpression = CreateLogicalExpression(cancellationToken);
            return false;
        }
        catch (Exception exception)
        {
            Error = exception;
            return true;
        }
    }

    /// <summary>
    /// Gets the parsed logical expression, using the cache when enabled.
    /// </summary>
    /// <returns>The parsed logical expression, or <c>null</c> when parsing fails.</returns>
    public LogicalExpression? GetLogicalExpression(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(ExpressionString))
        {
            if (EvaluationOptions.AllowNullOrEmptyExpressions)
                return ExpressionString?.Length == 0 ? new ValueExpression(string.Empty) : null;

            throw new NCalcException($"{nameof(ExpressionString)} cannot be null or empty.");
        }

        if (Configuration.CacheEnabled && LogicalExpressionCache.TryGetValue(ExpressionString!, out var cached))
            return cached;

        try
        {
            Error = null;
            var expression = CreateLogicalExpression(cancellationToken);
            if (Configuration.CacheEnabled)
                LogicalExpressionCache.Set(ExpressionString!, expression);
            return expression;
        }
        catch (Exception exception)
        {
            Error = exception;
            return null;
        }
    }

    /// <summary>
    /// Returns the normalized expression string.
    /// </summary>
    /// <param name="evaluateParameters">Whether parameter values should be substituted.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The expression string.</returns>
    public string ToExpressionString(bool evaluateParameters = false, CancellationToken cancellationToken = default)
    {
        var logicalExpression = GetLogicalExpression(cancellationToken);
        if (Error is not null)
            throw Error;
        if (logicalExpression is null)
            return string.Empty;
        if (!evaluateParameters)
            return logicalExpression.ToExpressionString();

        return logicalExpression.Accept(new ParameterSubstitutionVisitor(Context, EvaluationOptions, cancellationToken)).TrimEnd();
    }
}
