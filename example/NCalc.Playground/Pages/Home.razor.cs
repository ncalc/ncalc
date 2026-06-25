using NCalc.Playground.Helpers;
using NCalc.Playground.Models;

namespace NCalc.Playground.Pages;

public partial class Home
{
    private const int MaximumHistoryItems = 20;

    private readonly List<VariableRow> _variables =
    [
        new("quantity", "12"),
        new("unitPrice", "19.95"),
        new("taxRate", "0.0825")
    ];

    private readonly List<EvaluationHistoryItem> _history = [];

    private string _expressionText = "Round([quantity] * [unitPrice] * (1 + [taxRate]), 2)";
    private EvaluationHistoryItem? _selectedHistoryItem;

    private void Evaluate(string expression)
    {
        _expressionText = expression;

        var parameters = _variables
            .ConvertAll(variable => new VariableInput(variable.Name, variable.ValueText)) ;

        var evaluation = EvaluationHelper.Evaluate(_expressionText, parameters);
        var item = new EvaluationHistoryItem(
            evaluation.ExpressionString,
            _expressionText,
            evaluation.Value,
            evaluation.ValueType,
            evaluation.HasError,
            parameters);

        _history.Insert(0, item);
        _selectedHistoryItem = item;

        while (_history.Count > MaximumHistoryItems)
            _history.RemoveAt(_history.Count - 1);
    }

    private void AddVariable()
    {
        _variables.Add(new VariableRow("", ""));
    }

    private void RemoveVariable(VariableRow variable)
    {
        _variables.Remove(variable);
    }

    private void ClearHistory()
    {
        _history.Clear();
        _selectedHistoryItem = null;
    }

    private void ApplyHistoryItem(EvaluationHistoryItem item)
    {
        _selectedHistoryItem = item;
        _expressionText = item.OriginalExpressionString;

        _variables.Clear();
        _variables.AddRange(item.Parameters.Select(parameter => new VariableRow(parameter.Name, parameter.ValueText)));
    }
}
