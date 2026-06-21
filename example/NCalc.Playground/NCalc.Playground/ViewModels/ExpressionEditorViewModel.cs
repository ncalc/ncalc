using CommunityToolkit.Mvvm.ComponentModel;

namespace NCalc.Playground.ViewModels;

public sealed partial class ExpressionEditorViewModel : ViewModelBase
{
    [ObservableProperty] private string _expressionText = "Round([quantity] * [unitPrice] * (1 + [taxRate]), 2)";
}
