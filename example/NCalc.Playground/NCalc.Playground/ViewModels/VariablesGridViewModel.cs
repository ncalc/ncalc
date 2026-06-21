using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using NCalc.Playground.Models;

namespace NCalc.Playground.ViewModels;

public sealed partial class VariablesGridViewModel : ViewModelBase
{
    public ObservableCollection<VariableRowViewModel> Variables { get; } =
    [
        new("quantity", "12"),
        new("unitPrice", "19.95"),
        new("taxRate", "0.0825")
    ];

    public IEnumerable<VariableInput> ToInputs()
    {
        return Variables.Select(variable => new VariableInput(variable.Name, variable.ValueText));
    }

    public void Load(IEnumerable<VariableInput> variables)
    {
        Variables.Clear();

        foreach (var variable in variables)
            Variables.Add(new VariableRowViewModel(variable.Name, variable.ValueText));
    }

    [RelayCommand]
    private void AddVariable()
    {
        Variables.Add(new VariableRowViewModel("", ""));
    }

    [RelayCommand]
    private void RemoveVariable(VariableRowViewModel? variable)
    {
        if (variable is not null)
            Variables.Remove(variable);
    }
}
