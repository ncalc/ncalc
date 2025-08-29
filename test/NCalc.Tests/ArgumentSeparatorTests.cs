using System.Globalization;
using NCalc;
using Xunit;

public class ArgumentSeparatorTests
{
    [Fact]
    public void FunctionArguments_CommaSeparator_Works()
    {
        var expr = new Expression("sum(1,2,3)");
        expr.Context.ArgumentSeparator = ',';
        expr.EvaluateFunction += (name, args) => {
            if (name == "sum")
                args.Result = args.Parameters.Sum(x => Convert.ToInt32(x));
        };
        var result = expr.Evaluate();
        Assert.Equal(6, result);
    }

    [Fact]
    public void FunctionArguments_SemicolonSeparator_Works()
    {
        var expr = new Expression("sum(1;2;3)");
        expr.Context.ArgumentSeparator = ';';
        expr.EvaluateFunction += (name, args) => {
            if (name == "sum")
                args.Result = args.Parameters.Sum(x => Convert.ToInt32(x));
        };
        var result = expr.Evaluate();
        Assert.Equal(6, result);
    }

    [Fact]
    public void FunctionArguments_CustomSeparator_FailsWithWrongSeparator()
    {
        var expr = new Expression("sum(1,2,3)");
        expr.Context.ArgumentSeparator = ';';
        expr.EvaluateFunction += (name, args) => {
            if (name == "sum")
                args.Result = args.Parameters.Sum(x => Convert.ToInt32(x));
        };
        Assert.ThrowsAny<Exception>(() => expr.Evaluate());
    }
}
