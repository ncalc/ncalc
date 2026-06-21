using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using Classic.Avalonia.Theme;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NCalc.Playground.Helpers;
using NCalc.Playground.Models;

namespace NCalc.Playground.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ThemeVariant SelectedTheme { get; set; }

    public ExpressionEditorViewModel Expression { get; } = new();
    public VariablesGridViewModel Variables { get; } = new();

    public HistoryViewModel History { get; } = new();

    public IReadOnlyList<ThemeVariant> Themes { get; } = ClassicTheme.AllVariants;

    public MainViewModel()
    {
        SelectedTheme = Themes[0];
        History.SelectedItemChanged += ApplyHistoryItem;
    }

    partial void OnSelectedThemeChanged(ThemeVariant value)
    {
        if (Application.Current is { } application)
            application.RequestedThemeVariant = value;
    }

    [RelayCommand]
    private async Task Evaluate()
    {
        var parameters = Variables.ToInputs().ToList();
        var evaluation = EvaluationHelper.Evaluate(Expression.ExpressionText, parameters);


        History.Add(new EvaluationHistoryItem(
            evaluation.ExpressionString,
            Expression.ExpressionText,
            evaluation.Value,
            evaluation.ValueType,
            evaluation.HasError,
            parameters));
    }


    private void ApplyHistoryItem(EvaluationHistoryItem item)
    {
        Expression.ExpressionText = item.OriginalExpressionString;
        Variables.Load(item.Parameters);
    }
}
