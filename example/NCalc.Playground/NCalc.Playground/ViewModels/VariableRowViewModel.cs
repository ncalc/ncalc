using CommunityToolkit.Mvvm.ComponentModel;

namespace NCalc.Playground.ViewModels;

public sealed partial class VariableRowViewModel(string name, string valueText) : ViewModelBase
{
    [ObservableProperty]
    public partial string Name { get; set; } = name;

    [ObservableProperty]
    public partial string ValueText { get; set; } = valueText;
}
