using System.Diagnostics.CodeAnalysis;
using NCalc.Cache;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Visitors;

namespace NCalc;

/// <summary>
/// Base class with common utilities of AST parsing and evaluation.
/// </summary>
public abstract class ExpressionBase<TExpressionContext> where TExpressionContext : ExpressionContextBase, new()
{
    protected TExpressionContext Context { get; }

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

    public LogicalExpression? LogicalExpression { get; protected set; }

    public Exception? Error { get; private set; }

    private ILogicalExpressionCache LogicalExpressionCache { get; }
    private ILogicalExpressionFactory LogicalExpressionFactory { get; }

    protected ExpressionBase(TExpressionContext? context = null)
    {
        LogicalExpressionCache = Cache.LogicalExpressionCache.GetInstance();
        LogicalExpressionFactory = Factories.LogicalExpressionFactory.GetInstance();
        Context = context ?? new TExpressionContext();
    }

    protected ExpressionBase(
        string expressionString,
        TExpressionContext context,
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache)
    {
        ExpressionString = expressionString;
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
        Context = context;
    }

    protected ExpressionBase(
        LogicalExpression logicalExpression,
        TExpressionContext context,
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache logicalExpressionCache)
    {
        LogicalExpression = logicalExpression;
        LogicalExpressionCache = logicalExpressionCache;
        LogicalExpressionFactory = logicalExpressionFactory;
        Context = context;
    }

    /// <summary>
    /// Returns a list with all parameter names from the expression.
    /// </summary>
    public List<string> GetParameterNames()
    {
        var parameterExtractionVisitor = new ParameterExtractionVisitor();
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, Context.Options);
        return LogicalExpression.Accept(parameterExtractionVisitor);
    }

    /// <summary>
    /// Returns a list with all function names from the expression.
    /// </summary>
    public List<string> GetFunctionNames()
    {
        var functionExtractionVisitor = new FunctionExtractionVisitor();
        LogicalExpression ??= LogicalExpressionFactory.Create(ExpressionString!, Context.Options);
        return LogicalExpression.Accept(functionExtractionVisitor);
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
}