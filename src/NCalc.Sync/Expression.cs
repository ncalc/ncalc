using NCalc.Cache;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Helpers;
using NCalc.Services;

namespace NCalc;

/// <summary>
/// This class represents a mathematical or logical expression that can be evaluated.
/// It supports caching, custom parameter and function evaluation, and options for handling null parameters and iterating over parameter collections.
/// The class manages the parsing, validation, and evaluation of expressions, and provides mechanisms for error detection and reporting.
/// </summary>
public partial class Expression : ExpressionBase<ExpressionContext>
{
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

    protected IEvaluationService EvaluationService { get; }

    private Expression(ExpressionContext? context = null) : base(context ?? new ExpressionContext())
    {
        EvaluationService = new EvaluationService();
    }

    public Expression(
        string expression,
        ExpressionContext context,
        ILogicalExpressionFactory factory,
        ILogicalExpressionCache cache,
        IEvaluationService evaluationService) : base(expression, context, factory, cache)
    {
        EvaluationService = evaluationService;
    }

    public Expression(
        LogicalExpression logicalExpression,
        ExpressionContext context,
        ILogicalExpressionFactory factory,
        ILogicalExpressionCache cache,
        IEvaluationService evaluationService) : base(logicalExpression, context, factory, cache)
    {
        EvaluationService = evaluationService;
    }

    public Expression(string? expression, ExpressionContext? context = null) : this(context)
    {
        ExpressionString = expression;
    }

    // ReSharper disable once RedundantOverload.Global
    // Reason: False positive, ExpressionContext have implicit conversions.
    public Expression(string? expression) : this(expression, ExpressionOptions.None)
    {
    }

    public Expression(string? expression, ExpressionOptions options = ExpressionOptions.None,
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
    /// Evaluates the logical expression.
    /// </summary>
    /// <returns>The result of the evaluation.</returns>
    /// <exception cref="NCalcException">Thrown when there is an error in the expression.</exception>
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

        return EvaluationService.Evaluate(LogicalExpression, Context);
    }

    private object? IterateParameters()
    {
        var parameterEnumerators = ParametersHelper.GetEnumerators(Parameters, out var size);

        if (size != null)
        {
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

        return EvaluationService.Evaluate(LogicalExpression!, Context);
    }
}