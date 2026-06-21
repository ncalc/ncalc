using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NCalc.Playground.Models;

namespace NCalc.Playground.ViewModels;

public sealed partial class HistoryViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial EvaluationHistoryItem? SelectedItem { get; set; }

    public ObservableCollection<EvaluationHistoryItem> Items { get; } = [];

    public event Action<EvaluationHistoryItem>? SelectedItemChanged;

    public void Add(EvaluationHistoryItem item)
    {
        Items.Insert(0, item);

        while (Items.Count > 20)
            Items.RemoveAt(Items.Count - 1);
    }

    [RelayCommand]
    private void Clear()
    {
        Items.Clear();
        SelectedItem = null;
    }

    partial void OnSelectedItemChanged(EvaluationHistoryItem? value)
    {
        if (value is not null)
            SelectedItemChanged?.Invoke(value);
    }
}
