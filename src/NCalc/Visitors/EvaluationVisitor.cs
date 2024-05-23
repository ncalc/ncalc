using NCalc.Domain;
using NCalc.Handlers;
using NCalc.Helpers;

namespace NCalc.Visitors;

public class EvaluationVisitor : IEvaluationVisitor
{
    public event EvaluateFunctionHandler? EvaluateFunction;
    public event EvaluateParameterHandler? EvaluateParameter;
    
    public ExpressionOptions Options { get; set; } 
    public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentUICulture;
    public Dictionary<string, object?> Parameters { get; set; } = new();

    public object? Result { get; set; }

    public EvaluationVisitor()
    {
        
    }

    public EvaluationVisitor(ExpressionOptions options, CultureInfo cultureInfo)
    {
        Options = options;
        CultureInfo = cultureInfo;
    }
    
    private static readonly Type[] BuiltInTypes =
    [
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(long),
        typeof(ulong),
        typeof(int),
        typeof(uint),
        typeof(short),
        typeof(ushort),
        typeof(byte),
        typeof(sbyte),
        typeof(char),
        typeof(bool),
        typeof(string),
        typeof(object)
    ];
    
    private object? Evaluate(LogicalExpression expression)
    {
        expression.Accept(this);
        return Result;
    }

    public void Visit(LogicalExpression expression)
    {
        throw new NotSupportedException("The Visit method is not supported for this class.");
    }

    /// <summary>
    /// Gets the the most precise type.
    /// </summary>
    /// <param name="a">Type a.</param>
    /// <param name="b">Type b.</param>
    /// <returns></returns>
    private static Type GetMostPreciseType(Type? a, Type? b)
    {
        foreach (var t in BuiltInTypes)
        {
            if (a == t || b == t)
            {
                return t;
            }
        }

        return a ?? typeof(object);
    }

    public int CompareUsingMostPreciseType(object? a, object? b)
    {
        Type mpt;
        if (a == null)
            mpt = GetMostPreciseType(null, b?.GetType());
        else
            mpt = GetMostPreciseType(a.GetType(), b?.GetType());
        
        var aValue = a != null ? Convert.ChangeType(a, mpt, CultureInfo) : null;
        var bValue = b != null ? Convert.ChangeType(b, mpt, CultureInfo) : null;

        bool isCaseInsensitiveComparer = Options.HasOption(ExpressionOptions.CaseInsensitiveComparer);

        if (isCaseInsensitiveComparer)
            return CaseInsensitiveComparer.Default.Compare(aValue, bValue);

        return Comparer.Default.Compare(aValue, bValue);
    }

    public void Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        expression.LeftExpression.Accept(this);
        var left = Convert.ToBoolean(Result, CultureInfo);

        if (left)
        {
            expression.MiddleExpression.Accept(this);
        }
        else
        {
            expression.RightExpression.Accept(this);
        }
    }

    private static bool IsReal(object? value) => value is decimal or double or float;

    public void Visit(BinaryExpression expression)
    {
        // simulate Lazy<Func<>> behavior for late evaluation
        object? leftValue = null;

        // simulate Lazy<Func<>> behavior for late evaluations
        object? rightValue = null;

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                Result = Convert.ToBoolean(Left(), CultureInfo) && Convert.ToBoolean(Right(), CultureInfo);
                break;

            case BinaryExpressionType.Or:
                Result = Convert.ToBoolean(Left(), CultureInfo) || Convert.ToBoolean(Right(), CultureInfo);
                break;

            case BinaryExpressionType.Div:
                Result = IsReal(Left()) || IsReal(Right())
                    ? MathHelper.Divide(Left(), Right(), CultureInfo)
                    : MathHelper.Divide(Convert.ToDouble(Left(), CultureInfo), Right(), CultureInfo);
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
                Result = MathHelper.Subtract(Left(), Right(), CultureInfo);
                break;

            case BinaryExpressionType.Modulo:
                Result = MathHelper.Modulo(Left(), Right(), CultureInfo);
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
                    Result = MathHelper.Add(Left(), Right(), CultureInfo);
                }

                break;

            case BinaryExpressionType.Times:
                Result = MathHelper.Multiply(Left(), Right(), CultureInfo);
                break;

            case BinaryExpressionType.BitwiseAnd:
                Result = Convert.ToUInt16(Left(), CultureInfo) & Convert.ToUInt16(Right(), CultureInfo);
                break;

            case BinaryExpressionType.BitwiseOr:
                Result = Convert.ToUInt16(Left(), CultureInfo) | Convert.ToUInt16(Right(), CultureInfo);
                break;

            case BinaryExpressionType.BitwiseXOr:
                Result = Convert.ToUInt16(Left(), CultureInfo) ^ Convert.ToUInt16(Right(), CultureInfo);
                break;

            case BinaryExpressionType.LeftShift:
                Result = Convert.ToUInt16(Left(), CultureInfo) << Convert.ToUInt16(Right(), CultureInfo);
                break;

            case BinaryExpressionType.RightShift:
                Result = Convert.ToUInt16(Left(), CultureInfo) >> Convert.ToUInt16(Right(), CultureInfo);
                break;

            case BinaryExpressionType.Exponentiation:
                Result = Math.Pow(Convert.ToDouble(Left(), CultureInfo), Convert.ToDouble(Right(), CultureInfo));
                break;
        }

        return;

        object? Left()
        {
            if (leftValue != null)
                return leftValue;
            
            expression.LeftExpression.Accept(this);
            leftValue = Result;

            return leftValue;
        }

        object? Right()
        {
            if (rightValue != null) 
                return rightValue;
            
            expression.RightExpression.Accept(this);
            rightValue = Result;

            return rightValue;
        }
    }

    public void Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        expression.Expression.Accept(this);

        switch (expression.Type)
        {
            case UnaryExpressionType.Not:
                Result = !Convert.ToBoolean(Result, CultureInfo);
                break;

            case UnaryExpressionType.Negate:
                Result = MathHelper.Subtract(0, Result, CultureInfo);
                break;

            case UnaryExpressionType.BitwiseNot:
                Result = ~Convert.ToUInt16(Result, CultureInfo);
                break;

            case UnaryExpressionType.Positive:
                // No-op
                break;
        }
    }

    public void Visit(ValueExpression expression)
    {
        Result = expression.Value;
    }

    public void Visit(Function function)
    {
        var args = new FunctionArgs
        {
            Parameters = new Expression[function.Expressions.Length]
        };

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (var i = 0; i < function.Expressions.Length; i++)
        {
            args.Parameters[i] = new Expression(function.Expressions[i], Options, CultureInfo);
            args.Parameters[i].EvaluateFunction += EvaluateFunction;
            args.Parameters[i].EvaluateParameter += EvaluateParameter;

            // Assign the parameters of the Expression to the arguments so that custom Functions and Parameters can use them
            args.Parameters[i].Parameters = Parameters;
        }

        bool ignoreCase = Options.HasOption(ExpressionOptions.IgnoreCase);

        // Calls external implementation
        OnEvaluateFunction(ignoreCase ? function.Identifier.Name.ToLower() : function.Identifier.Name, args);

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
        var functionName = function.Identifier.Name.ToUpperInvariant();

        switch (functionName)
        {
            case "ABS":
                CheckCase("Abs", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Abs() takes exactly 1 argument");
                Result = Math.Abs(Convert.ToDecimal(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "ACOS":
                CheckCase("Acos", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Acos() takes exactly 1 argument");
                Result = Math.Acos(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "ASIN":
                CheckCase("Asin", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Asin() takes exactly 1 argument");
                Result = Math.Asin(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "ATAN":
                CheckCase("Atan", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Atan() takes exactly 1 argument");
                Result = Math.Atan(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "ATAN2":
                CheckCase("Atan2", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Atan2() takes exactly 2 argument");
                Result = Math.Atan2(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo),
                    Convert.ToDouble(Evaluate(function.Expressions[1]), CultureInfo));
                break;

            case "CEILING":
                CheckCase("Ceiling", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Ceiling() takes exactly 1 argument");
                Result = Math.Ceiling(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "COS":
                CheckCase("Cos", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Cos() takes exactly 1 argument");
                Result = Math.Cos(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "EXP":
                CheckCase("Exp", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Exp() takes exactly 1 argument");
                Result = Math.Exp(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "FLOOR":
                CheckCase("Floor", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Floor() takes exactly 1 argument");
                Result = Math.Floor(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "IEEEREMAINDER":
                CheckCase("IEEERemainder", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("IEEERemainder() takes exactly 2 arguments");
                Result = Math.IEEERemainder(Convert.ToDouble(Evaluate(function.Expressions[0])),
                    Convert.ToDouble(Evaluate(function.Expressions[1]), CultureInfo));
                break;

            case "LN":
                CheckCase("Ln", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Ln() takes exactly 1 argument");
                Result = Math.Log(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "LOG":
                CheckCase("Log", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Log() takes exactly 2 arguments");
                Result = Math.Log(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo),
                    Convert.ToDouble(Evaluate(function.Expressions[1]), CultureInfo));
                break;

            case "LOG10":
                CheckCase("Log10", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Log10() takes exactly 1 argument");
                Result = Math.Log10(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "POW":
                CheckCase("Pow", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Pow() takes exactly 2 arguments");
                Result = Math.Pow(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo),
                    Convert.ToDouble(Evaluate(function.Expressions[1]), CultureInfo));
                break;

            case "ROUND":
                CheckCase("Round", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Round() takes exactly 2 arguments");

                var rounding = (Options & ExpressionOptions.RoundAwayFromZero) == ExpressionOptions.RoundAwayFromZero ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven;

                Result = Math.Round(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo), Convert.ToInt16(Evaluate(function.Expressions[1]), CultureInfo), rounding);

                break;

            case "SIGN":
                CheckCase("Sign", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Sign() takes exactly 1 argument");
                Result = Math.Sign(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "SIN":
                CheckCase("Sin", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Sin() takes exactly 1 argument");
                Result = Math.Sin(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "SQRT":
                CheckCase("Sqrt", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Sqrt() takes exactly 1 argument");
                Result = Math.Sqrt(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "TAN":
                CheckCase("Tan", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Tan() takes exactly 1 argument");
                Result = Math.Tan(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "TRUNCATE":
                CheckCase("Truncate", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new ArgumentException("Truncate() takes exactly 1 argument");
                Result = Math.Truncate(Convert.ToDouble(Evaluate(function.Expressions[0]), CultureInfo));
                break;

            case "MAX":
                CheckCase("Max", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Max() takes exactly 2 arguments");
                var maxleft = Evaluate(function.Expressions[0]);
                var maxright = Evaluate(function.Expressions[1]);
                Result = MathHelper.Max(maxleft, maxright, CultureInfo);
                break;

            case "MIN":
                CheckCase("Min", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new ArgumentException("Min() takes exactly 2 arguments");
                var minleft = Evaluate(function.Expressions[0]);
                var minright = Evaluate(function.Expressions[1]);
                Result = MathHelper.Min(minleft, minright, CultureInfo);
                break;

            case "IFS":
                CheckCase("ifs", function.Identifier.Name);
                if (function.Expressions.Length < 3 || function.Expressions.Length % 2 != 1)
                    throw new ArgumentException("ifs() takes at least 3 arguments, or an odd number of arguments");
                foreach (var eval in function.Expressions.Where((_, i) => i % 2 == 0))
                {
                    var index = Array.IndexOf(function.Expressions, eval);
                    var tf = Convert.ToBoolean(Evaluate(eval), CultureInfo);
                    if (index == function.Expressions.Length - 1)
                        Result = Evaluate(eval);
                    else if (tf)
                    {
                        Result = Evaluate(function.Expressions[index + 1]);
                        break;
                    }
                }

                break;

            case "IF":
                CheckCase("if", function.Identifier.Name);
                if (function.Expressions.Length != 3)
                    throw new ArgumentException("if() takes exactly 3 arguments");
                var cond = Convert.ToBoolean(Evaluate(function.Expressions[0]), CultureInfo);
                Result = cond ? Evaluate(function.Expressions[1]) : Evaluate(function.Expressions[2]);
                break;

            case "IN":
                CheckCase("in", function.Identifier.Name);
                if (function.Expressions.Length < 2)
                    throw new ArgumentException("in() takes at least 2 arguments");
                var parameter = Evaluate(function.Expressions[0]);
                var evaluation = false;
                for (var i = 1; i < function.Expressions.Length; i++)
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
        bool ignoreCase = Options.HasOption(ExpressionOptions.IgnoreCase);

        if (!ignoreCase && function != called)
            throw new ArgumentException($"Function not found {called}. Try {function} instead.");
    }
    
    public void Visit(Identifier parameter)
    {
        if (Parameters.TryGetValue(parameter.Name, out var parameterValue))
        {
            // The parameter is defined in the hashtable
            if (parameterValue is Expression expression)
            {
                // Overloads parameters 
                foreach (var p in Parameters)
                {
                    if (expression.Parameters != null) 
                        expression.Parameters[p.Key] = p.Value;
                }

                expression.EvaluateFunction += EvaluateFunction;
                expression.EvaluateParameter += EvaluateParameter;

                Result = expression.Evaluate();
            }
            else
                Result = parameterValue;
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
    
    protected void OnEvaluateFunction(string name, FunctionArgs args)
    {
        EvaluateFunction?.Invoke(name, args);
    }

    protected void OnEvaluateParameter(string name, ParameterArgs args)
    {
        EvaluateParameter?.Invoke(name, args);
    }
}