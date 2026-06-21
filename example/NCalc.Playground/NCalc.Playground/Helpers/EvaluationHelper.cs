using System;
using System.Collections.Generic;
using System.Globalization;
using NCalc.Playground.Models;

namespace NCalc.Playground.Helpers;

public static class EvaluationHelper
{
    public static EvaluationResult Evaluate(string expressionText, IEnumerable<VariableInput> variables)
    {
        try
        {
            var parameters = BuildParameters(variables);
            var expression = new Expression(expressionText, CultureInfo.InvariantCulture);

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

    private static Dictionary<string, object?> BuildParameters(IEnumerable<VariableInput> variables)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var variable in variables)
        {
            var name = variable.Name.Trim();
            if (name.Length == 0 && variable.ValueText.Trim().Length == 0)
                continue;

            if (!IdentifierValidator.IsValid(name))
                throw new InvalidOperationException($"'{variable.Name}' is not a valid variable name.");

            parameters[name] = VariableValueParser.Parse(variable.ValueText);
        }

        return parameters;
    }
}
