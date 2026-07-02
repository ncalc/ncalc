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
/// This class represents a mathematical or logical expression that can be asynchronous evaluated.
/// It supports caching, custom parameter and function evaluation, and options for handling null parameters and iterating over parameter collections.
/// The class manages the parsing, validation, and evaluation of expressions, and provides mechanisms for error detection and reporting.
/// </summary>
public class Expression
{
    public IDictionary<string, ExpressionParameter> DynamicParameters
    {
        get => Context.DynamicParameters;
        set => Context.DynamicParameters = value;
    }

    public IDictionary<string, AsyncExpressionParameter> AsyncParameters
    {
        get => Context.AsyncParameters;
        set => Context.AsyncParameters = value;
    }

    public IDictionary<string, ExpressionFunction> Functions
    {
        get => Context.Functions;
        set => Context.Functions = value;
    }

    public IDictionary<string, AsyncExpressionFunction> AsyncFunctions
    {
        get => Context.AsyncFunctions;
        set => Context.AsyncFunctions = value;
    }

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
    /// Event triggered to handle binary evaluation.
    /// </summary>
    public event EvaluateBinaryAsyncHandler EvaluateBinaryAsync
    {
        add => Context.EvaluateBinaryAsyncHandler+= value;
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

    protected ExpressionContext Context { get; }

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

    /// <summary>
    /// Parameters for the expression evaluation.
    /// </summary>
    public IDictionary<string, object?> Parameters
    {
        get => Context.StaticParameters;
        set => Context.StaticParameters = value;
    }

    /// <summary>
    /// Textual representation of the expression.
    /// </summary>
    public string? ExpressionString { get; protected init; }

    public LogicalExpression? LogicalExpression { get; set; }
    public Exception? Error { get; private set; }
    private ILogicalExpressionCache LogicalExpressionCache { get; }
    private ILogicalExpressionFactory LogicalExpressionFactory { get; }
    protected internal IEvaluationVisitorFactory? EvaluationVisitorFactory { get; private set; }

    protected Expression(ExpressionContext? context = null, IEvaluationVisitorFactory? evaluationVisitorFactory = null)
    {
        LogicalExpressionCache = Cache.LogicalExpressionCache.GetInstance();
        LogicalExpressionFactory = Factories.LogicalExpressionFactory.GetInstance();
        Context = context ?? new ExpressionContext();
        EvaluationVisitorFactory = evaluationVisitorFactory;
    }

    public Expression(
        string expressionString,
        ExpressionContext context,
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache,
        IEvaluationVisitorFactory? evaluationVisitorFactory = null)
        : this(context, evaluationVisitorFactory)
    {
        ExpressionString = expressionString;
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
    }

    public Expression(
        LogicalExpression logicalExpression,
        ExpressionContext context,
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache,
        IEvaluationVisitorFactory? evaluationVisitorFactory = null)
        : this(context, evaluationVisitorFactory)
    {
        LogicalExpression = logicalExpression;
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
    }

    public Expression(string? expression, ExpressionContext? context = null, IEvaluationVisitorFactory? evaluationVisitorFactory = null) : this(context, evaluationVisitorFactory)
    {
        ExpressionString = expression;
    }

    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, ExpressionContext have implicit conversions.
    public Expression(string? expression) : this(expression, ExpressionOptions.None)
    {
    }

    public Expression(string? expression, ExpressionOptions options = ExpressionOptions.None,
        CultureInfo? cultureInfo = null,
        IEvaluationVisitorFactory? evaluationVisitorFactory = null) : this(expression, new ExpressionContext(options, cultureInfo), evaluationVisitorFactory)
    {
    }

    public Expression(LogicalExpression logicalExpression, ExpressionContext? context = null, IEvaluationVisitorFactory? evaluationVisitorFactory = null) : this(context, evaluationVisitorFactory)
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
        CultureInfo? cultureInfo = null,
        IEvaluationVisitorFactory? evaluationVisitorFactory = null) : this(logicalExpression, new ExpressionContext(options, cultureInfo), evaluationVisitorFactory)
    {
    }

    protected virtual EvaluationVisitor CreateEvaluationVisitor(CancellationToken cancellationToken = default)
    {
        return EvaluationVisitorFactory?.CreateEvaluationVisitor(Context, cancellationToken)
               ?? new EvaluationVisitor(Context, cancellationToken: cancellationToken);
    }

    protected virtual AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(CancellationToken cancellationToken = default)
    {
        return EvaluationVisitorFactory?.CreateAsyncEvaluationVisitor(Context, cancellationToken)
               ?? new AsyncEvaluationVisitor(Context, cancellationToken: cancellationToken);
    }

    internal void SetEvaluationVisitorFactory(IEvaluationVisitorFactory? evaluationVisitorFactory)
    {
        EvaluationVisitorFactory ??= evaluationVisitorFactory;
    }

    /// <summary>
    /// Evaluates the logical expression.
    /// </summary>
    /// <returns>The result of the evaluation.</returns>
    /// <exception cref="NCalcException">Thrown when there is an error in the expression.</exception>
    public object? Evaluate(CancellationToken cancellationToken = default)
    {
        LogicalExpression ??= GetLogicalExpression(cancellationToken);

        if (Error is not null)
            throw Error;

        try
        {
            // If array evaluation, execute the same expression multiple times
            if (Options.HasFlag(ExpressionOptions.IterateParameters))
                return IterateParameters(cancellationToken);

            var evaluationVisitor = CreateEvaluationVisitor(cancellationToken);

            return LogicalExpression?.Accept(evaluationVisitor);
        }
        catch (InvalidCastException exception)
        {
            throw new NCalcEvaluationException("Error evaluating expression.", exception);
        }
    }

    /// <summary>
    /// Evaluates the logical expression and converts the result to type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">The type to which the evaluation result should be converted.</typeparam>
    /// <returns>The result of the evaluation, converted to type <typeparamref name="T"/>, or <c>default</c> if the result is <c>null</c>.</returns>
    /// <exception cref="NCalcCastException">Thrown when the result cannot be cast to type <typeparamref name="T"/>.</exception>
    /// <exception cref="NCalcException">Thrown when there is an error in the expression.</exception>
    public T? Evaluate<T>(CancellationToken cancellationToken = default)
    {
        var result = Evaluate(cancellationToken);

        return CastResult<T>(result, Context.CultureInfo);
    }

    /// <summary>
    /// Asynchronously evaluates the logical expression and converts the result to type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">The type to which the evaluation result should be converted.</typeparam>
    /// <returns>A task that represents the asynchronous evaluation. The task result contains the value converted to type <typeparamref name="T"/>, or <c>default</c> if the result is <c>null</c>.</returns>
    /// <exception cref="NCalcCastException">Thrown when the result cannot be cast to type <typeparamref name="T"/>.</exception>
    /// <exception cref="NCalcException">Thrown when there is an error in the expression.</exception>
    public async Task<T?> EvaluateAsync<T>(CancellationToken cancellationToken = default)
    {
        var result = await EvaluateAsync(cancellationToken);

        return CastResult<T>(result, Context.CultureInfo);
    }

    private static T? CastResult<T>(object? result, CultureInfo cultureInfo)
    {
        switch (result)
        {
            case null:
                return default;
            case T typed:
                return typed;
        }

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

    /// <summary>
    /// Asynchronously evaluates the logical expression.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the evaluation.</returns>
    /// <exception cref="NCalcException">Thrown when there is an error in the expression.</exception>
    public async ValueTask<object?> EvaluateAsync(CancellationToken cancellationToken = default)
    {
        LogicalExpression ??= GetLogicalExpression(cancellationToken);

        if (Error is not null)
            throw Error;

        try
        {
            // If array evaluation, execute the same expression multiple times
            if (Options.HasFlag(ExpressionOptions.IterateParameters))
                return await IterateParametersAsync(cancellationToken).ConfigureAwait(false);

            var evaluationVisitor = CreateAsyncEvaluationVisitor(cancellationToken);

            if (LogicalExpression is null)
                return null;

            return await LogicalExpression.Accept(evaluationVisitor).ConfigureAwait(false);
        }
        catch (InvalidCastException exception)
        {
            throw new NCalcEvaluationException("Error evaluating expression.", exception);
        }
    }

    private async ValueTask<object?> IterateParametersAsync(CancellationToken cancellationToken)
    {
        var parameterEnumerators = ParametersHelper.GetEnumerators(Parameters, out var size);

        var evaluationVisitor = CreateAsyncEvaluationVisitor(cancellationToken);

        if (LogicalExpression is null)
            return null;

        if (size == null)
            return await LogicalExpression.Accept(evaluationVisitor);

        var results = new List<object?>(size.Value);

        for (var i = 0; i < size; i++)
        {
            foreach (var kvp in parameterEnumerators)
            {
                kvp.Value.MoveNext();
                Parameters[kvp.Key] = kvp.Value.Current;
            }

            results.Add(await LogicalExpression.Accept(evaluationVisitor));
        }

        return results;
    }

    private object? IterateParameters(CancellationToken cancellationToken)
    {
        var parameterEnumerators = ParametersHelper.GetEnumerators(Parameters, out var size);

        var evaluationVisitor = CreateEvaluationVisitor(cancellationToken);

        if (LogicalExpression is null)
            return null;

        if (size == null)
            return LogicalExpression.Accept(evaluationVisitor);

        var results = new List<object?>(size.Value);

        for (var i = 0; i < size; i++)
        {
            foreach (var kvp in parameterEnumerators)
            {
                kvp.Value.MoveNext();
                Parameters[kvp.Key] = kvp.Value.Current;
            }

            results.Add(LogicalExpression.Accept(evaluationVisitor));
        }

        return results;
    }

    /// <summary>
    /// Returns a list with all parameter names from the expression.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public List<string> GetParameterNames(CancellationToken cancellationToken = default)
    {
        var parameterExtractionVisitor = new ParameterExtractionVisitor();
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, CultureInfo, Context.Options, cancellationToken);
        return LogicalExpression.Accept(parameterExtractionVisitor);
    }

    /// <summary>
    /// Returns a list with all function names from the expression.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public List<string> GetFunctionNames(CancellationToken cancellationToken = default)
    {
        var functionExtractionVisitor = new FunctionExtractionVisitor();
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, CultureInfo, Context.Options, cancellationToken);
        return LogicalExpression.Accept(functionExtractionVisitor);
    }

    /// <summary>
    /// Create the LogicalExpression in order to check syntax errors.
    /// If errors are detected, the Error property contains the exception.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the expression syntax is correct, otherwise False.</returns>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool HasErrors(CancellationToken cancellationToken = default)
    {
        try
        {
            Error = null;
            LogicalExpression = LogicalExpressionFactory.Create(ExpressionString!, CultureInfo, Context.Options, cancellationToken);

            // In case HasErrors() is called multiple times for the same expression
            return LogicalExpression != null && Error != null;
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
            if (Options.HasFlag(ExpressionOptions.AllowNullOrEmptyExpressions))
            {
                return ExpressionString?.Length == 0 ? new ValueExpression(string.Empty) : null;
            }

            throw new NCalcException($"{nameof(ExpressionString)} cannot be null or empty.");
        }

        var isCacheEnabled = !Options.HasFlag(ExpressionOptions.NoCache);

        LogicalExpression? logicalExpression = null;

        if (isCacheEnabled && LogicalExpressionCache.TryGetValue(ExpressionString!, out logicalExpression))
            return logicalExpression!;

        try
        {
            Error = null;

            logicalExpression = LogicalExpressionFactory.Create(ExpressionString!, CultureInfo, Context.Options, cancellationToken);
            if (isCacheEnabled)
                LogicalExpressionCache.Set(ExpressionString!, logicalExpression);
        }
        catch (Exception exception)
        {
            Error = exception;
        }

        return logicalExpression;
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

        return logicalExpression.Accept(new ParameterSubstitutionVisitor(Context, cancellationToken)).TrimEnd();
    }
}
