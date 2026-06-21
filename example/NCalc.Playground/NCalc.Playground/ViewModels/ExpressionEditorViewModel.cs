using CommunityToolkit.Mvvm.ComponentModel;

namespace NCalc.Playground.ViewModels;

public sealed partial class ExpressionEditorViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string ExpressionText { get; set; } = "Round([quantity] * [unitPrice] * (1 + [taxRate]), 2)";
}
