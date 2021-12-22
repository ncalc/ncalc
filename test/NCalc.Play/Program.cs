using System;

namespace NCalc.Play
{
    /// <summary>
    /// Summary description for Program.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var expressions = new[]
            {
                "2 * (3 + 5)",
                "2 * (2*(2*(2+1)))",
                "10 % 3",
                "false || not (false and true)",
                "3 > 2 and 1 <= (3-2)",
                "3 % 2 != 10 % 3",
                "'A' = 'A' ? 10 : ('B' = 'B' ? 15 : 20)",
                "if ('A' = 'A', 10, if ('B' = 'B', 15, 20))"
                //"if( [age] >= 18, 'majeur', 'mineur')",
                //"CalculateBenefits([user]) * [Taxes]"
            };

            foreach (string expression in expressions)
                Console.WriteLine("{0} = {1}",
                    expression,
                    new Expression(expression).Evaluate());

            Console.ReadKey();
        }
    }
}
