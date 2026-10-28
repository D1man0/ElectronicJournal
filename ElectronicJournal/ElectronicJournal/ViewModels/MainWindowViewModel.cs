using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicJournal.Models;
using ElectronicJournal.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ElectronicJournal.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;
    private int _nextId = 1;

    public ObservableCollection<SchoolClass> Classes { get; } = new();

    [ObservableProperty]
    private SchoolClass? _selectedClass;

    [ObservableProperty]
    private string _newClassName = string.Empty;

    public MainWindowViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [RelayCommand]
    private void AddClass()
    {
        var name = NewClassName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;

        Classes.Add(new SchoolClass { Id = _nextId++, Name = name });
        NewClassName = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteClassAsync()
    {
        if (SelectedClass is null)
            return;

        var confirmed = await _dialogService.ConfirmAsync(
            $"Удалить класс «{SelectedClass.Name}»?");
        if (!confirmed)
            return;

        Classes.Remove(SelectedClass);
        SelectedClass = null;
    }
}