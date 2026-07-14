using NCalc.Playground.Models;

namespace NCalc.Playground.Helpers;

public static class EvaluationHelper
{
    public static EvaluationResult Evaluate(string expressionText, IEnumerable<VariableInput> variables, ExpressionOptions options)
    {
        try
        {
            var parameters = BuildParameters(variables, options);
            var context = new ExpressionContext(parameters);
            var expression = new Expression(expressionText, options, context, CultureInfo.InvariantCulture);

            foreach (var (name, value) in parameters)
                expression.Parameters[name] = value;

            var result = expression.Evaluate();

            var evaluationResult = EvaluationResult.Success(result);

            evaluationResult.ExpressionString = expression.ToExpressionString(evaluateParameters: true);

            return evaluationResult;
        }
        catch (Exception exception)
        {
            var evaluationResult = EvaluationResult.Failure(exception);
            evaluationResult.ExpressionString = expressionText;

            return evaluationResult;
        }
    }

    private static Dictionary<string, object?> BuildParameters(IEnumerable<VariableInput> variables, ExpressionConfiguration configuration)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var (name, value) in variables)
        {
            var normalizedName = name.Trim();
            if (normalizedName.Length == 0 && value.Trim().Length == 0)
                continue;

            if (!IdentifierValidator.IsValid(normalizedName))
                throw new InvalidOperationException($"'{name}' is not a valid variable name.");

            var hasNumbers = value.Any(char.IsDigit);

            if(hasNumbers)
                parameters[normalizedName] = new Expression(value, configuration).Evaluate();
            else
                parameters[normalizedName] = value;
        }

        return parameters;
    }
}
