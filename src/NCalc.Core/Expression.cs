using System.Diagnostics.CodeAnalysis;
using NCalc.Cache;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Visitors;

namespace NCalc;

public partial class Expression
{
    public event EvaluateFunctionHandler EvaluateFunction
    {
        add => Context.EvaluateFunctionHandler += value;
        remove => Context.EvaluateFunctionHandler -= value;
    }

    public event EvaluateParameterHandler EvaluateParameter
    {
        add => Context.EvaluateParameterHandler += value;
        remove => Context.EvaluateParameterHandler -= value;
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

    public IDictionary<string, ExpressionAsyncParameter> AsyncDynamicParameters
    {
        get => Context.AsyncDynamicParameters;
        set => Context.AsyncDynamicParameters = value;
    }

    public IDictionary<string, ExpressionAsyncFunction> AsyncFunctions
    {
        get => Context.AsyncFunctions;
        set => Context.AsyncFunctions = value;
    }

    public ExpressionOptions Options
    {
        get => Context.Options;
        set => Context.Options = value;
    }

    public CultureInfo CultureInfo
    {
        get => Context.CultureInfo;
        set => Context.CultureInfo = value;
    }

    public IDictionary<string, object?> Parameters
    {
        get => Context.StaticParameters;
        set => Context.StaticParameters = value;
    }

    public string? ExpressionString { get; protected init; }

    public LogicalExpression? LogicalExpression { get; protected set; }

    public Exception? Error { get; private set; }

    protected ExpressionContext Context { get; }

    protected IEvaluationVisitorFactory EvaluationVisitorFactory { get; }

    private ILogicalExpressionCache LogicalExpressionCache { get; }

    private ILogicalExpressionFactory LogicalExpressionFactory { get; }

    private Expression(ExpressionContext? context = null)
    {
        Context = context ?? new ExpressionContext();
        LogicalExpressionCache = Cache.LogicalExpressionCache.GetInstance();
        LogicalExpressionFactory = Factories.LogicalExpressionFactory.GetInstance();
        EvaluationVisitorFactory = new EvaluationVisitorFactory();
    }
    public Expression(
        string expression,
        ExpressionContext context,
        ILogicalExpressionFactory factory,
        ILogicalExpressionCache cache,
        IEvaluationVisitorFactory evaluationVisitorFactory)
    {
        ExpressionString = expression;
        Context = context;
        LogicalExpressionFactory = factory;
        LogicalExpressionCache = cache;
        EvaluationVisitorFactory = evaluationVisitorFactory;
    }

    public Expression(
        LogicalExpression logicalExpression,
        ExpressionContext context,
        ILogicalExpressionFactory factory,
        ILogicalExpressionCache cache,
        IEvaluationVisitorFactory evaluationVisitorFactory)
    {
        LogicalExpression = logicalExpression;
        Context = context;
        LogicalExpressionFactory = factory;
        LogicalExpressionCache = cache;
        EvaluationVisitorFactory = evaluationVisitorFactory;
    }

    public Expression(string? expression, ExpressionContext? context = null) : this(context)
    {
        ExpressionString = expression;
    }

    public Expression(string? expression) : this(expression, ExpressionOptions.None)
    {
    }

    public Expression(string? expression, ExpressionOptions options = ExpressionOptions.None,
        CultureInfo? cultureInfo = null) : this(expression, new ExpressionContext(options, cultureInfo))
    {
    }

    public Expression(LogicalExpression logicalExpression, ExpressionContext? context = null) : this(context)
    {
        LogicalExpression = logicalExpression ?? throw new ArgumentException("Expression can't be null", nameof(logicalExpression));
    }

    public Expression(LogicalExpression logicalExpression) : this(logicalExpression, ExpressionOptions.None)
    {
    }

    public Expression(LogicalExpression logicalExpression, ExpressionOptions options = ExpressionOptions.None,
        CultureInfo? cultureInfo = null) : this(logicalExpression, new ExpressionContext(options, cultureInfo))
    {
    }

    public ValueTask<object?> EvaluateAsync()
    {
        LogicalExpression ??= GetLogicalExpression();

        if (Error is not null)
            throw Error;

        if (Options.HasFlag(ExpressionOptions.AllowNullParameter))
            Parameters["null"] = null;

        if (Options.HasFlag(ExpressionOptions.IterateParameters))
            return IterateParametersAsync();

        var evaluationVisitor = EvaluationVisitorFactory.Create(Context);

        if (LogicalExpression is null)
            return new ValueTask<object?>();

        return LogicalExpression.Accept(evaluationVisitor);
    }

    public object? Evaluate()
    {
        var task = EvaluateAsync();
        return task.IsCompletedSuccessfully ? task.Result : task.AsTask().GetAwaiter().GetResult();
    }

    private async ValueTask<object?> IterateParametersAsync()
    {
        var parameterEnumerators = ParametersHelper.GetEnumerators(Parameters, out var size);

        var evaluationVisitor = EvaluationVisitorFactory.Create(Context);

        if (LogicalExpression is null)
            return null;

        if (size == null)
            return await LogicalExpression.Accept(evaluationVisitor);

        var results = new List<object?>();

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

    public List<string> GetParameterNames()
    {
        var parameterExtractionVisitor = new ParameterExtractionVisitor();
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, CultureInfo, Context.Options);
        return LogicalExpression.Accept(parameterExtractionVisitor);
    }

    public List<string> GetFunctionNames()
    {
        var functionExtractionVisitor = new FunctionExtractionVisitor();
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, CultureInfo, Context.Options);
        return LogicalExpression.Accept(functionExtractionVisitor);
    }

    [MemberNotNullWhen(true, nameof(Error))]
    public bool HasErrors()
    {
        try
        {
            LogicalExpression = LogicalExpressionFactory.Create(ExpressionString!, CultureInfo, Context.Options);
            return LogicalExpression != null && Error != null;
        }
        catch (Exception exception)
        {
            Error = exception;
            return true;
        }
    }

    protected LogicalExpression? GetLogicalExpression()
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
            logicalExpression = LogicalExpressionFactory.Create(ExpressionString!, CultureInfo, Context.Options);
            if (isCacheEnabled)
                LogicalExpressionCache.Set(ExpressionString!, logicalExpression);
        }
        catch (Exception exception)
        {
            Error = exception;
        }

        return logicalExpression;
    }
}
