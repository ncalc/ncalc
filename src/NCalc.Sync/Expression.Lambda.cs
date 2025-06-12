using System.Runtime.CompilerServices;

using FastExpressionCompiler;

using NCalc.Exceptions;
using NCalc.Visitors;

using LinqExpression = System.Linq.Expressions.Expression;
using LinqParameterExpression = System.Linq.Expressions.ParameterExpression;

namespace NCalc;

public partial class Expression
{
    internal static readonly bool UseSystemLinqCompiler;

    static Expression()
    {
        UseSystemLinqCompiler = AppContext.TryGetSwitch("NCalc.UseSystemLinqCompiler", out var enabled) && enabled;
    }

    private readonly struct Void;
    protected record struct LinqExpressionWithParameter(LinqExpression Expression, LinqParameterExpression? Parameter);

    private LinqExpressionWithParameter ToLinqExpressionInternal<TContext, TResult>()
    {
        LogicalExpression ??= GetLogicalExpression();

        if (LogicalExpression is null)
            throw Error!;

        LambdaExpressionVisitor visitor;
        LinqParameterExpression? parameter = null;
        if (IsVoidType<TContext>())
        {
            visitor = new(null, this.Context, Parameters, Options);
        }
        else
        {
            parameter = LinqExpression.Parameter(typeof(TContext), "ctx");
            visitor = new(parameter, this.Context, null, Options);
        }

        var body = LogicalExpression.Accept(visitor);
        if (!IsSameType(body, typeof(TResult)))
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

        if (UseSystemLinqCompiler)
            return lambda.Compile();

        return lambda.CompileFast();
    }

    public Func<TContext, TResult> ToLambda<TContext, TResult>()
    {
        var linqExp = ToLinqExpression<TContext, TResult>();
        if (linqExp.Parameter != null)
        {
            var lambda = LinqExpression.Lambda<Func<TContext, TResult>>(linqExp.Expression, linqExp.Parameter);

            if (UseSystemLinqCompiler)
                return lambda.Compile();

            return lambda.CompileFast();
        }

        throw new NCalcException("Linq expression parameter cannot be null");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsVoidType<TContext>() => typeof(TContext) == typeof(Void);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSameType(LinqExpression expression, Type targetType) => expression.Type == targetType;
}