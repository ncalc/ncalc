using NCalc;

var expr = new Expression("p1 + GetValue(p1*2)");
expr.DynamicParameters["p1"] = _ =>
{
    return 10;
};
expr.Functions["GetValue"] = (args2) =>
{
    var args2Result =  args2[0].Evaluate();
    args2.Context.DynamicParameters["p1"] = _ =>
    {
        return 11;
    };

    return args2Result;
};



var res = expr.Evaluate(); // Now should be 21
Console.Write(res);