using System.Collections;
using NCalc.Domain;

namespace NCalc.Tests;

using Xunit;

[Trait("Category","Event Handlers")]
public class EventHandlersTests
{
    [Fact]
    public void ExpressionShouldEvaluateCustomFunctions()
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.EvaluateFunction += delegate(string name, FunctionArgs args)
        {
            if (name == "SecretOperation")
                args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
        };

        Assert.Equal(9, e.Evaluate());
    }

    [Fact]
    public void ExpressionShouldEvaluateCustomFunctionsWithParameters()
    {
        var e = new Expression("SecretOperation([e], 6) + f");
        e.Parameters["e"] = 3;
        e.Parameters["f"] = 1;

        e.EvaluateFunction += delegate(string name, FunctionArgs args)
        {
            if (name == "SecretOperation")
                args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
        };

        Assert.Equal(10, e.Evaluate());
    }

    [Fact]
    public void ExpressionShouldEvaluateParameters()
    {
        var e = new Expression("Round(Pow(Pi, 2) + Pow([Pi Squared], 2) + [X], 2)");

        e.Parameters["Pi Squared"] = new Expression("Pi * [Pi]");
        e.Parameters["X"] = 10;

        e.EvaluateParameter += delegate(string name, ParameterArgs args)
        {
            if (name == "Pi")
                args.Result = 3.14;
        };

        Assert.Equal(117.07, e.Evaluate());
    }
    
    [Fact]
    public void Should_Evaluate_Function_Only_Once_Issue_107()
    {
        var counter = 0;
        var totalCounter = 0;

        var expression = new Expression("MyFunc()");

        expression.EvaluateFunction += Expression_EvaluateFunction;

        for (var i = 0; i < 10; i++)
        {
            counter = 0;
            _ = expression.Evaluate();
        }


        void Expression_EvaluateFunction(string name, FunctionArgs args)
        {
            if (name != "MyFunc") 
                return;
            args.Result = 1;
            counter++;
            totalCounter++;
        }

        Assert.Equal(10, totalCounter);
    }
    
    [Fact]
    public void ShouldOverrideExistingFunctions()
    {
        var e = new Expression("Round(1.99, 2)");

        Assert.Equal(1.99d, e.Evaluate());

        e.EvaluateFunction += delegate(string name, FunctionArgs args)
        {
            if (name == "Round")
                args.Result = 3;
        };

        Assert.Equal(3, e.Evaluate());
    }
    
    [Fact]
    public void ShouldEvaluateArrayParameters()
    {
        var e = new Expression("x * x", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0, 1, 2, 3, 4 }
            }
        };

        var result = (IList)e.Evaluate();

        Assert.Equal(0, result[0]);
        Assert.Equal(1, result[1]);
        Assert.Equal(4, result[2]);
        Assert.Equal(9, result[3]);
        Assert.Equal(16, result[4]);
    }
    
    [Fact]
    public void ShouldHandleCustomParametersWhenNoSpecificParameterIsDefined()
    {
        var e = new Expression("Round(Pow([Pi], 2) + Pow([Pi], 2) + 10, 2)");

        e.EvaluateParameter += delegate(string name, ParameterArgs arg)
        {
            if (name == "Pi")
                arg.Result = 3.14;
        };

        e.Evaluate();
    }

    [Fact]
    public void ShouldHandleCustomFunctionsInFunctions()
    {
        var e = new Expression("if(true, func1(x) + func2(func3(y)), 0)");

        e.EvaluateFunction += delegate(string name, FunctionArgs arg)
        {
            switch (name)
            {
                case "func1":
                    arg.Result = 1;
                    break;
                case "func2":
                    arg.Result = 2 * Convert.ToDouble(arg.Parameters[0].Evaluate());
                    break;
                case "func3":
                    arg.Result = 3 * Convert.ToDouble(arg.Parameters[0].Evaluate());
                    break;
            }
        };

        e.EvaluateParameter += delegate(string name, ParameterArgs arg)
        {
            switch (name)
            {
                case "x":
                    arg.Result = 1;
                    break;
                case "y":
                    arg.Result = 2;
                    break;
                case "z":
                    arg.Result = 3;
                    break;
            }
        };

        Assert.Equal(13d, e.Evaluate());
    }

    [Fact]
    public void CustomFunctionShouldReturnNull()
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.EvaluateFunction += delegate(string name, FunctionArgs args)
        {
            Assert.False(args.HasResult);
            if (name == "SecretOperation")
                args.Result = null;
            Assert.True(args.HasResult);
        };

        Assert.Null(e.Evaluate());
    }

    [Fact]
    public void CustomParametersShouldReturnNull()
    {
        var e = new Expression("x");

        e.EvaluateParameter += delegate(string name, ParameterArgs args)
        {
            Assert.False(args.HasResult);
            if (name == "x")
                args.Result = null;
            Assert.True(args.HasResult);
        };

        Assert.Null(e.Evaluate());
    }
}
