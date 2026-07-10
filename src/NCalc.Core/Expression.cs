using System.Diagnostics.CodeAnalysis;
using NCalc.Cache;
using NCalc.Exceptions;
using NCalc.Extensions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Parser;
using NCalc.Visitors;

namespace NCalc;

/// <summary>
/// A reusable parsed expression and its immutable parsing and evaluation options.
/// Runtime parameters, functions, and handlers are supplied through <see cref="ExpressionContext"/>.
/// </summary>
public class Expression
{
    public string? ExpressionString { get; protected init; }
    public LogicalExpression? LogicalExpression { get; set; }
    public Exception? Error { get; private set; }

    public ExpressionConfiguration Configuration { get; set; }

    public LogicalExpressionParserOptions ParserOptions => Configuration.Parsing;
    public ExpressionEvaluationOptions EvaluationOptions => Configuration.Evaluation;

    public CultureInfo CultureInfo { get; set; }

    public bool CacheEnabled { get; set; } = true;

    public ExpressionContext Context { get; }

    public IDictionary<string, object?> Parameters
    {
        get => Context.Parameters;
        set => Context.Parameters = value;
    }
    public IDictionary<string, ExpressionParameter> DynamicParameters => Context.DynamicParameters;
    public IDictionary<string, AsyncExpressionParameter> AsyncParameters => Context.AsyncParameters;
    public IDictionary<string, ExpressionFunction> Functions => Context.Functions;
    public IDictionary<string, AsyncExpressionFunction> AsyncFunctions => Context.AsyncFunctions;

    public event EvaluateFunctionHandler EvaluateFunction
    {
        add => Context.EvaluateFunctionHandler += value;
        remove => Context.EvaluateFunctionHandler -= value;
    }

    public event EvaluateBinaryHandler EvaluateBinary
    {
        add => Context.EvaluateBinaryHandler += value;
        remove => Context.EvaluateBinaryHandler -= value;
    }

    public event EvaluateBinaryAsyncHandler EvaluateBinaryAsync
    {
        add => Context.EvaluateBinaryAsyncHandler += value;
        remove => Context.EvaluateBinaryAsyncHandler -= value;
    }

    public event EvaluateAsyncFunctionHandler EvaluateAsyncFunction
    {
        add => Context.EvaluateAsyncFunctionHandler += value;
        remove => Context.EvaluateAsyncFunctionHandler -= value;
    }

    public event EvaluateParameterHandler EvaluateParameter
    {
        add => Context.EvaluateParameterHandler += value;
        remove => Context.EvaluateParameterHandler -= value;
    }

    public event EvaluateAsyncParameterHandler EvaluateAsyncParameter
    {
        add => Context.EvaluateAsyncParameterHandler += value;
        remove => Context.EvaluateAsyncParameterHandler -= value;
    }

    private ILogicalExpressionCache LogicalExpressionCache { get; }
    private ILogicalExpressionFactory LogicalExpressionFactory { get; }
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
        CacheEnabled = configuration?.CacheEnabled ?? true;
        Configuration = configuration ?? new ExpressionConfiguration();
        CultureInfo = (CultureInfo?)null ?? CultureInfo.CurrentCulture;
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
        CacheEnabled = configuration?.CacheEnabled ?? true;
        Configuration = configuration ?? new ExpressionConfiguration();
        CultureInfo = (CultureInfo?)null ?? CultureInfo.CurrentCulture;
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
    }

    public Expression(string? expression, ExpressionConfiguration? configuration = null, CultureInfo? cultureInfo = null, IEvaluationVisitorFactory? evaluationVisitorFactory = null) : this(evaluationVisitorFactory)
    {
        ExpressionString = expression;
        CacheEnabled = configuration?.CacheEnabled ?? true;
        Configuration = configuration ?? new ExpressionConfiguration();
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }

    public Expression(string? expression, CultureInfo cultureInfo) : this()
    {
        ExpressionString = expression;
        CacheEnabled = ((ExpressionConfiguration?)null)?.CacheEnabled ?? true;
        Configuration = new ExpressionConfiguration();
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }

    public Expression(string? expression, ExpressionContext context) : this()
    {
        ExpressionString = expression;
        Context = context;
        CacheEnabled = ((ExpressionConfiguration?)null)?.CacheEnabled ?? true;
        Configuration = (ExpressionConfiguration?)null ?? new ExpressionConfiguration();
        CultureInfo = CultureInfo.CurrentCulture;
    }

    public Expression(string? expression, ExpressionConfiguration configuration, ExpressionContext context, CultureInfo? cultureInfo = null, IEvaluationVisitorFactory? evaluationVisitorFactory = null) : this(evaluationVisitorFactory)
    {
        ExpressionString = expression;
        Context = context;
        CacheEnabled = configuration?.CacheEnabled ?? true;
        Configuration = configuration ?? new ExpressionConfiguration();
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }

    public Expression(LogicalExpression logicalExpression, ExpressionConfiguration? configuration = null, CultureInfo? cultureInfo = null, IEvaluationVisitorFactory? evaluationVisitorFactory = null) : this(evaluationVisitorFactory)
    {
        LogicalExpression = logicalExpression ?? throw new ArgumentNullException(nameof(logicalExpression));
        CacheEnabled = configuration?.CacheEnabled ?? true;
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

    public T? Evaluate<T>(CancellationToken cancellationToken = default)
    {
        return CastResult<T>(Evaluate(cancellationToken), CultureInfo);
    }

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

    public List<string> GetParameterNames(CancellationToken cancellationToken = default)
    {
        LogicalExpression ??= CreateLogicalExpression(cancellationToken);
        return LogicalExpression.Accept(new ParameterExtractionVisitor());
    }

    private LogicalExpression CreateLogicalExpression(CancellationToken cancellationToken)
    {
        return LogicalExpressionFactory.Create(ExpressionString!, ParserOptions, CultureInfo, cancellationToken);
    }

    public List<string> GetFunctionNames(CancellationToken cancellationToken = default)
    {
        LogicalExpression ??= CreateLogicalExpression(cancellationToken);
        return LogicalExpression.Accept(new FunctionExtractionVisitor());
    }

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

    public LogicalExpression? GetLogicalExpression(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(ExpressionString))
        {
            if (EvaluationOptions.AllowNullOrEmptyExpressions)
                return ExpressionString?.Length == 0 ? new ValueExpression(string.Empty) : null;

            throw new NCalcException($"{nameof(ExpressionString)} cannot be null or empty.");
        }

        if (CacheEnabled && LogicalExpressionCache.TryGetValue(ExpressionString!, out var cached))
            return cached;

        try
        {
            Error = null;
            var expression = CreateLogicalExpression(cancellationToken);
            if (CacheEnabled)
                LogicalExpressionCache.Set(ExpressionString!, expression);
            return expression;
        }
        catch (Exception exception)
        {
            Error = exception;
            return null;
        }
    }

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
