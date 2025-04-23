using System;
using FastExpressionCompiler;
using FastExpressionCompiler.LightExpression;
using NCalc.Exceptions;
using NCalc.Visitors;
using LinqExpression = FastExpressionCompiler.LightExpression.Expression;
using LinqParameterExpression = FastExpressionCompiler.LightExpression.ParameterExpression;

namespace NCalc;

public partial class Expression
{
    internal static readonly bool UseSystemLynqCompiler;

    static Expression()
    {
        UseSystemLynqCompiler = AppContext.TryGetSwitch("NCalc.UseSystemLynqCompiler", out var enabled) && enabled;
    }

    private readonly struct Void;
    protected record struct LinqExpressionWithParameter(LinqExpression Expression, LinqParameterExpression? Parameter);

    private LinqExpressionWithParameter ToLinqExpressionInternal<TContext, TResult>()
    {
        LogicalExpression ??= GetLogicalExpression();

        if (LogicalExpression is null)
            throw Error!;

        LambdaFastExpressionVisitor visitor;
        LinqParameterExpression? parameter = null;
        if (typeof(TContext) != typeof(Void))
        {
            parameter = LinqExpression.Parameter(typeof(TContext), "ctx");
            visitor = new(parameter, Options);
        }
        else
        {
            visitor = new(Parameters, Options);
        }

        var body = LogicalExpression.Accept(visitor);
        if (body.Type != typeof(TResult))
        {
            body = LinqExpression.Convert(body, typeof(TResult));
        }

        return new() { Expression = body, Parameter = parameter };
    }

    protected virtual LinqExpression ToLinqExpression<TResult>()
    {
        return ToLinqExpressionInternal<Void, TResult>().Expression;
    }

    protected virtual LinqExpressionWithParameter ToLinqExpression<TContext, TResult>()
    {
        return ToLinqExpressionInternal<TContext, TResult>();
    }

    public Func<TResult> ToLambda<TResult>()
    {
        var body = ToLinqExpression<TResult>();
        var lambda = LinqExpression.Lambda<Func<TResult>>(body);

        return lambda.CompileFast();
    }

    public Func<TContext, TResult> ToLambda<TContext, TResult>()
    {
        var linqExp = ToLinqExpression<TContext, TResult>();
        if (linqExp.Parameter != null)
        {
            var lambda = LinqExpression.Lambda<Func<TContext, TResult>>(linqExp.Expression, linqExp.Parameter);
            return lambda.CompileFast();
        }

        throw new NCalcException("Linq expression parameter cannot be null");
    }
}