﻿#nullable disable

using System.Reflection;
using LinqExpression = System.Linq.Expressions.Expression;

namespace NCalc.Reflection;

internal class ExtendedMethodInfo
{
    public MethodInfo BaseMethodInfo { get; init; }
    public LinqExpression[] PreparedArguments { get; init; }
    public int Score { get; init; }
}