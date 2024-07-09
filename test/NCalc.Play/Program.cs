using NCalc;
using NCalc.Exceptions;

while (true)
{
    Console.Write("Enter an expression (or type 'exit' to quit): ");
    var input = Console.ReadLine();

    if (input?.Trim().ToLower() == "exit")
        break;

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Expression cannot be empty.");
        continue;
    }
    
    try
    {
        var expression = new Expression(input);
        var result = expression.Evaluate();
        Console.WriteLine("Result: {0}", result);
    }
    catch (NCalcParserException ex)
    {
        Console.WriteLine("Error parsing the expression: {0}", ex.Message);
    }
    catch (NCalcEvaluationException ex)
    {
        Console.WriteLine("Error evaluating the expression: {0}", ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Unexpected error: {0}", ex.Message);
    }
}