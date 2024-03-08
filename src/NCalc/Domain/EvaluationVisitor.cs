using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NCalc.Domain;

public class EvaluationVisitor(EvaluateOptions options, CultureInfo cultureInfo) : LogicalExpressionVisitor
{
    private delegate T Func<out T>();

    private bool IgnoreCase => (options & EvaluateOptions.IgnoreCase) == EvaluateOptions.IgnoreCase;

    public EvaluationVisitor(EvaluateOptions options) : this(options, CultureInfo.CurrentCulture)
    {
    }

    public object Result { get; protected set; }

    private object Evaluate(LogicalExpression expression)
    {
        expression.Accept(this);
        return Result;
    }

    public override void Visit(LogicalExpression expression)
    {
        throw new Exception("The method or operation is not implemented.");
    }


    private static readonly Type[] CommonTypes = [typeof(double), typeof(long), typeof(bool), typeof(string), typeof(decimal)];


    /// <summary>
    /// Gets the the most precise type.
    /// </summary>
    /// <param name="a">Type a.</param>
    /// <param name="b">Type b.</param>
    /// <returns></returns>
    private static Type GetMostPreciseType(Type a, Type b)
    {
        foreach (var t in CommonTypes)
        {
            if (a == t || b == t)
            {
                return t;
            }
        }

        return a;
    }

    public int CompareUsingMostPreciseType(object a, object b)
    {
        Type mpt;
        if (a == null)
        {
            if (b == null)
                return 0;
            mpt = GetMostPreciseType(null, b.GetType());
        }
        else
        {
            mpt = GetMostPreciseType(a.GetType(), b?.GetType());
        }

        bool isCaseSensitiveComparer = (options & EvaluateOptions.CaseInsensitiveComparer) == 0;

        var comparer = isCaseSensitiveComparer ? (IComparer)Comparer.Default : CaseInsensitiveComparer.Default;

        return comparer.Compare(Convert.ChangeType(a, mpt, cultureInfo), Convert.ChangeType(b, mpt, cultureInfo));
    }

    public override void Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        expression.LeftExpression.Accept(this);
        bool left = Convert.ToBoolean(Result, cultureInfo);

        if (left)
        {
            expression.MiddleExpression.Accept(this);
        }
        else
        {
            expression.RightExpression.Accept(this);
        }
    }

    private static bool IsReal(object value)
    {
        var typeCode = Type.GetTypeCode(value.GetType());

        return typeCode is TypeCode.Decimal or TypeCode.Double or TypeCode.Single;
    }

    public override void Visit(BinaryExpression expression)
    {
        // simulate Lazy<Func<>> behavior for late evaluation
        object leftValue = null;

        // simulate Lazy<Func<>> behavior for late evaluations
        object rightValue = null;

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                Result = Convert.ToBoolean(Left(), cultureInfo) && Convert.ToBoolean(Right(), cultureInfo);
                break;

            case BinaryExpressionType.Or:
                Result = Convert.ToBoolean(Left(), cultureInfo) || Convert.ToBoolean(Right(), cultureInfo);
                break;

            case BinaryExpressionType.Div:
                Result = IsReal(Left()) || IsReal(Right())
                    ? Numbers.Divide(Left(), Right(), cultureInfo)
                    : Numbers.Divide(Convert.ToDouble(Left(), cultureInfo), Right(), cultureInfo);
                break;

            case BinaryExpressionType.Equal:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) == 0;
                break;

            case BinaryExpressionType.Greater:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) > 0;
                break;

            case BinaryExpressionType.GreaterOrEqual:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) >= 0;
                break;

            case BinaryExpressionType.Lesser:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) < 0;
                break;

            case BinaryExpressionType.LesserOrEqual:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) <= 0;
                break;

            case BinaryExpressionType.Minus:
                Result = Numbers.Subtract(Left(), Right(), cultureInfo);
                break;

            case BinaryExpressionType.Modulo:
                Result = Numbers.Modulo(Left(), Right(), cultureInfo);
                break;

            case BinaryExpressionType.NotEqual:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) != 0;
                break;

            case BinaryExpressionType.Plus:
                if (Left() is string)
                {
                    Result = string.Concat(Left(), Right());
                }
                else
                {
                    Result = Numbers.Add(Left(), Right(), cultureInfo);
                }

                break;

            case BinaryExpressionType.Times:
                Result = Numbers.Multiply(Left(), Right(), cultureInfo);
                break;

            case BinaryExpressionType.BitwiseAnd:
                Result = Convert.ToUInt16(Left(), cultureInfo) & Convert.ToUInt16(Right(), cultureInfo);
                break;

            case BinaryExpressionType.BitwiseOr:
                Result = Convert.ToUInt16(Left(), cultureInfo) | Convert.ToUInt16(Right(), cultureInfo);
                break;

            case BinaryExpressionType.BitwiseXOr:
                Result = Convert.ToUInt16(Left(), cultureInfo) ^ Convert.ToUInt16(Right(), cultureInfo);
                break;

            case BinaryExpressionType.LeftShift:
                Result = Convert.ToUInt16(Left(), cultureInfo) << Convert.ToUInt16(Right(), cultureInfo);
                break;

            case BinaryExpressionType.RightShift:
                Result = Convert.ToUInt16(Left(), cultureInfo) >> Convert.ToUInt16(Right(), cultureInfo);
                break;

            case BinaryExpressionType.Exponentiation:
                Result = Math.Pow(Convert.ToDouble(Left(), cultureInfo), Convert.ToDouble(Right(), cultureInfo));
                break;
        }

        return;

        object Left()
        {
            if (leftValue != null)
                return leftValue;
            
            expression.LeftExpression.Accept(this);
            leftValue = Result;

            return leftValue;
        }

        object Right()
        {
            if (rightValue != null) 
                return rightValue;
            
            expression.RightExpression.Accept(this);
            rightValue = Result;

            return rightValue;
        }
    }

    public override void Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        expression.Expression.Accept(this);

        switch (expression.Type)
        {
            case UnaryExpressionType.Not:
                Result = !Convert.ToBoolean(Result, cultureInfo);
                break;

            case UnaryExpressionType.Negate:
                Result = Numbers.Subtract(0, Result, cultureInfo);
                break;

            case UnaryExpressionType.BitwiseNot:
                Result = ~Convert.ToUInt16(Result, cultureInfo);
                break;

            case UnaryExpressionType.Positive:
                // No-op
                break;
        }
    }

    public override void Visit(ValueExpression expression)
    {
        Result = expression.Value;
    }

    public override void Visit(Function function)
    {
        var args = new FunctionArgs
        {
            Parameters = new Expression[function.Expressions.Length]
        };

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (int i = 0; i < function.Expressions.Length; i++)
        {
            args.Parameters[i] = new Expression(function.Expressions[i], options, cultureInfo);
            args.Parameters[i].EvaluateFunction += EvaluateFunction;
            args.Parameters[i].EvaluateParameter += EvaluateParameter;

            // Assign the parameters of the Expression to the arguments so that custom Functions and Parameters can use them
            args.Parameters[i].Parameters = Parameters;
        }

        // Calls external implementation
        OnEvaluateFunction(IgnoreCase ? function.Identifier.Name.ToLower() : function.Identifier.Name, args);

        // If an external implementation was found get the result back
        if (args.HasResult)
        {
            Result = args.Result;
            return;
        }

        ExecuteBuiltInFunction(function);
    }

    private void ExecuteBuiltInFunction(Function function)
    {
        var functionName = function.Identifier.Name.ToLower();

        switch (functionName)
        {
            case "abs":
                CheckCase("Abs", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Abs() takes exactly 1 argument");
                Result = Math.Abs(Convert.ToDecimal(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "acos":
                CheckCase("Acos", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Acos() takes exactly 1 argument");
                Result = Math.Acos(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "asin":
                CheckCase("Asin", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Asin() takes exactly 1 argument");
                Result = Math.Asin(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "atan":
                CheckCase("Atan", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Atan() takes exactly 1 argument");
                Result = Math.Atan(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "atan2":
                CheckCase("Atan2", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Atan2() takes exactly 2 argument");
                Result = Math.Atan2(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo),
                    Convert.ToDouble(Evaluate(function.Expressions[1]), cultureInfo));
                break;

            case "ceiling":
                CheckCase("Ceiling", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Ceiling() takes exactly 1 argument");
                Result = Math.Ceiling(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "cos":
                CheckCase("Cos", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Cos() takes exactly 1 argument");
                Result = Math.Cos(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "exp":
                CheckCase("Exp", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Exp() takes exactly 1 argument");
                Result = Math.Exp(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "floor":
                CheckCase("Floor", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Floor() takes exactly 1 argument");
                Result = Math.Floor(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "ieeeremainder":
                CheckCase("IEEERemainder", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("IEEERemainder() takes exactly 2 arguments");
                Result = Math.IEEERemainder(Convert.ToDouble(Evaluate(function.Expressions[0])),
                    Convert.ToDouble(Evaluate(function.Expressions[1]), cultureInfo));
                break;

            case "ln":
                CheckCase("Ln", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Ln() takes exactly 1 argument");
                Result = Math.Log(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "log":
                CheckCase("Log", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Log() takes exactly 2 arguments");
                Result = Math.Log(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo),
                    Convert.ToDouble(Evaluate(function.Expressions[1]), cultureInfo));
                break;

            case "log10":
                CheckCase("Log10", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Log10() takes exactly 1 argument");
                Result = Math.Log10(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "pow":
                CheckCase("Pow", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Pow() takes exactly 2 arguments");
                Result = Math.Pow(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo),
                    Convert.ToDouble(Evaluate(function.Expressions[1]), cultureInfo));
                break;

            case "round":
                CheckCase("Round", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Round() takes exactly 2 arguments");
                MidpointRounding rounding =
                    (options & EvaluateOptions.RoundAwayFromZero) == EvaluateOptions.RoundAwayFromZero
                        ? MidpointRounding.AwayFromZero
                        : MidpointRounding.ToEven;
                Result = Math.Round(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo),
                    Convert.ToInt16(Evaluate(function.Expressions[1]), cultureInfo), rounding);
                break;

            case "sign":
                CheckCase("Sign", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Sign() takes exactly 1 argument");
                Result = Math.Sign(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "sin":
                CheckCase("Sin", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Sin() takes exactly 1 argument");
                Result = Math.Sin(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "sqrt":
                CheckCase("Sqrt", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Sqrt() takes exactly 1 argument");
                Result = Math.Sqrt(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "tan":
                CheckCase("Tan", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Tan() takes exactly 1 argument");
                Result = Math.Tan(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "truncate":
                CheckCase("Truncate", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Truncate() takes exactly 1 argument");
                Result = Math.Truncate(Convert.ToDouble(Evaluate(function.Expressions[0]), cultureInfo));
                break;

            case "max":
                CheckCase("Max", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Max() takes exactly 2 arguments");
                object maxleft = Evaluate(function.Expressions[0]);
                object maxright = Evaluate(function.Expressions[1]);
                Result = Numbers.Max(maxleft, maxright, cultureInfo);
                break;

            case "min":
                CheckCase("Min", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Min() takes exactly 2 arguments");
                object minleft = Evaluate(function.Expressions[0]);
                object minright = Evaluate(function.Expressions[1]);
                Result = Numbers.Min(minleft, minright, cultureInfo);
                break;

            case "ifs":
                CheckCase("ifs", function.Identifier.Name);
                if (function.Expressions.Length < 3 || function.Expressions.Length % 2 != 1)
                    throw new ArgumentException("ifs() takes at least 3 arguments, or an odd number of arguments");
                foreach (var eval in function.Expressions.Where((_, i) => i % 2 == 0))
                {
                    var index = Array.IndexOf(function.Expressions, eval);
                    var tf = Convert.ToBoolean(Evaluate(eval), cultureInfo);
                    if (index == function.Expressions.Length - 1)
                        Result = Evaluate(eval);
                    else if (tf)
                    {
                        Result = Evaluate(function.Expressions[index + 1]);
                        break;
                    }
                }

                break;

            case "if":
                CheckCase("if", function.Identifier.Name);
                if (function.Expressions.Length != 3)
                    throw new ArgumentException("if() takes exactly 3 arguments");
                bool cond = Convert.ToBoolean(Evaluate(function.Expressions[0]), cultureInfo);
                Result = cond ? Evaluate(function.Expressions[1]) : Evaluate(function.Expressions[2]);
                break;

            case "in":
                CheckCase("in", function.Identifier.Name);
                if (function.Expressions.Length < 2)
                    throw new ArgumentException("in() takes at least 2 arguments");
                object parameter = Evaluate(function.Expressions[0]);
                bool evaluation = false;
                for (int i = 1; i < function.Expressions.Length; i++)
                {
                    var argument = Evaluate(function.Expressions[i]);
                    if (CompareUsingMostPreciseType(parameter, argument) == 0)
                    {
                        evaluation = true;
                        break;
                    }
                }

                Result = evaluation;
                break;

            default:
                throw new ArgumentException("Function not found", function.Identifier.Name);
        }
    }

    private void CheckCase(string function, string called)
    {
        if (IgnoreCase)
        {
            if (function.Equals(called, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new ArgumentException("Function not found", called);
        }

        if (function != called)
        {
            throw new ArgumentException($"Function not found {called}. Try {function} instead.");
        }
    }
    
    public event EvaluateFunctionHandler EvaluateFunction;
    
    protected void OnEvaluateFunction(string name, FunctionArgs args)
    {
        EvaluateFunction?.Invoke(name, args);
    }

    public override void Visit(Identifier parameter)
    {
        if (Parameters.ContainsKey(parameter.Name))
        {
            // The parameter is defined in the hashtable
            if (Parameters[parameter.Name] is Expression)
            {
                // The parameter is itself another Expression
                var expression = (Expression)Parameters[parameter.Name];

                // Overloads parameters 
                foreach (var p in Parameters)
                {
                    expression.Parameters[p.Key] = p.Value;
                }

                expression.EvaluateFunction += EvaluateFunction;
                expression.EvaluateParameter += EvaluateParameter;

                Result = ((Expression)Parameters[parameter.Name]).Evaluate();
            }
            else
                Result = Parameters[parameter.Name];
        }
        else
        {
            // The parameter should be defined in a call back method
            var args = new ParameterArgs();

            // Calls external implementation
            OnEvaluateParameter(parameter.Name, args);

            if (!args.HasResult)
                throw new ArgumentException("Parameter was not defined", parameter.Name);

            Result = args.Result;
        }
    }



    public event EvaluateParameterHandler EvaluateParameter;

    protected void OnEvaluateParameter(string name, ParameterArgs args)
    {
        EvaluateParameter?.Invoke(name, args);
    }

    public Dictionary<string, object> Parameters { get; set; }
}