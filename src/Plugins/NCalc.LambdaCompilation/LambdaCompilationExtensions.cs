using System.Runtime.CompilerServices;
using FastExpressionCompiler;
using NCalc.Exceptions;
using NCalc.LambdaCompilation.Visitors;
using LinqExpression = System.Linq.Expressions.Expression;
using LinqParameterExpression = System.Linq.Expressions.ParameterExpression;

namespace NCalc.LambdaCompilation;

public static class LambdaCompilationExtensions
{
    private static readonly bool UseSystemLinqCompiler;

    static LambdaCompilationExtensions()
    {
        UseSystemLinqCompiler = AppContext.TryGetSwitch("NCalc.UseSystemLinqCompiler", out var enabled) && enabled;
    }

    extension(Expression expression)
    {
        public Func<TResult> ToLambda<TResult>(CancellationToken ct = default)
        {
            var body = expression.ToLinqExpression<TResult>(ct);
            var lambda = LinqExpression.Lambda<Func<TResult>>(body);

            if (UseSystemLinqCompiler)
                return lambda.Compile();

            return lambda.CompileFast();
        }

        public Func<TContext, TResult> ToLambda<TContext, TResult>(CancellationToken ct = default)
        {
            var linqExp = expression.ToLinqExpression<TContext, TResult>(ct);
            if (linqExp.Parameter != null)
            {
                var lambda = LinqExpression.Lambda<Func<TContext, TResult>>(linqExp.Expression, linqExp.Parameter);

                if (UseSystemLinqCompiler)
                    return lambda.Compile();

                return lambda.CompileFast();
            }

            throw new NCalcException("Linq expression parameter cannot be null");
        }

        public LinqExpression ToLinqExpression<TResult>(CancellationToken ct = default)
        {
            return expression.ToLinqExpressionInternal<Void, TResult>(ct).Expression;
        }

        public LinqExpressionWithParameter ToLinqExpression<TContext, TResult>(CancellationToken ct = default)
        {
            return expression.ToLinqExpressionInternal<TContext, TResult>(ct);
        }

        private LinqExpressionWithParameter ToLinqExpressionInternal<TContext, TResult>(CancellationToken ct)
        {
            expression.LogicalExpression ??= expression.GetLogicalExpression(ct);

            if (expression.LogicalExpression is null)
                throw expression.Error!;

            LambdaExpressionVisitor visitor;
            LinqParameterExpression? parameter = null;
            if (IsVoidType<TContext>())
            {
                visitor = new(expression.Parameters, expression.Options);
            }
            else
            {
                parameter = LinqExpression.Parameter(typeof(TContext), "ctx");
                visitor = new(parameter, expression.Options);
            }

            var body = expression.LogicalExpression.Accept(visitor, ct);
            if (!IsSameType(body, typeof(TResult)))
            {
                body = LinqExpression.Convert(body, typeof(TResult));
            }

            return new() { Expression = body, Parameter = parameter };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsVoidType<TContext>() => typeof(TContext) == typeof(Void);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSameType(LinqExpression expression, Type targetType) => expression.Type == targetType;
}
