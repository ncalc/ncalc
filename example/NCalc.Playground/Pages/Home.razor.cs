using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NCalc;
using NCalc.Playground.Helpers;
using NCalc.Playground.Models;

namespace NCalc.Playground.Pages;

public partial class Home
{
    private const int MaximumHistoryItems = 20;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private readonly List<VariableRow> _variables =
    [
        new("quantity", "12"),
        new("unitPrice", "19.95"),
        new("taxRate", "0.0825")
    ];

    private readonly List<EvaluationHistoryItem> _history = [];

    private string _expressionText = "Round([quantity] * [unitPrice] * (1 + [taxRate]), 2)";
    private ExpressionOptions _selectedOptions;
    private EvaluationHistoryItem? _selectedHistoryItem;

    private void Evaluate(string expression)
    {
        _expressionText = expression;

        var parameters = _variables
            .ConvertAll(variable => new VariableInput(variable.Name, variable.ValueText)) ;

        var evaluation = EvaluationHelper.Evaluate(_expressionText, parameters, _selectedOptions);
        var item = new EvaluationHistoryItem
        {
            Expression = evaluation.ExpressionString,
            OriginalExpressionString = _expressionText,
            Result = evaluation.Value,
            ReturnType = evaluation.Type,
            HasError = evaluation.HasError,
            Parameters = parameters,
            Options = _selectedOptions
        };

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
        _selectedOptions = item.Options;

        _variables.Clear();
        _variables.AddRange(item.Parameters.Select(parameter => new VariableRow(parameter.Name, parameter.ValueText)));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JsRuntime.InvokeVoidAsync("bootstrapPlayground.initializeTooltips");
    }
}
