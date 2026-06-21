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

#if !DOCFX
     extension(Expression expression)
    {
        public Func<TResult> ToLambda<TResult>(CancellationToken cancellationToken = default)
        {
            var body = expression.ToLinqExpression<TResult>(cancellationToken);
            var lambda = LinqExpression.Lambda<Func<TResult>>(body);

            if (UseSystemLinqCompiler)
                return lambda.Compile();

            return lambda.CompileFast();
        }

        public Func<TContext, TResult> ToLambda<TContext, TResult>(CancellationToken cancellationToken = default)
        {
            var linqExp = expression.ToLinqExpression<TContext, TResult>(cancellationToken);
            if (linqExp.Parameter != null)
            {
                var lambda = LinqExpression.Lambda<Func<TContext, TResult>>(linqExp.Expression, linqExp.Parameter);

                if (UseSystemLinqCompiler)
                    return lambda.Compile();

                return lambda.CompileFast();
            }

            throw new NCalcException("Linq expression parameter cannot be null");
        }

        public LinqExpression ToLinqExpression<TResult>(CancellationToken cancellationToken = default)
        {
            return expression.ToLinqExpressionInternal<Void, TResult>(cancellationToken).Expression;
        }

        public LinqExpressionWithParameter ToLinqExpression<TContext, TResult>(CancellationToken cancellationToken = default)
        {
            return expression.ToLinqExpressionInternal<TContext, TResult>(cancellationToken);
        }

        private LinqExpressionWithParameter ToLinqExpressionInternal<TContext, TResult>(CancellationToken cancellationToken)
        {
            expression.LogicalExpression ??= expression.GetLogicalExpression(cancellationToken);

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

            cancellationToken.ThrowIfCancellationRequested();
            var body = expression.LogicalExpression.Accept(visitor);
            if (!IsSameType(body, typeof(TResult)))
            {
                body = LinqExpression.Convert(body, typeof(TResult));
            }

            return new() { Expression = body, Parameter = parameter };
        }
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsVoidType<TContext>() => typeof(TContext) == typeof(Void);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSameType(LinqExpression expression, Type targetType) => expression.Type == targetType;
}
