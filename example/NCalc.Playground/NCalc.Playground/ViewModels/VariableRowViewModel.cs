using CommunityToolkit.Mvvm.ComponentModel;

namespace NCalc.Playground.ViewModels;

public sealed partial class VariableRowViewModel(string name, string valueText) : ViewModelBase
{
    [ObservableProperty] private string _name = name;
    [ObservableProperty] private string _valueText = valueText;
}
