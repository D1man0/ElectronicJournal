using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicJournal.Models;

namespace ElectronicJournal.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private int _nextId = 1;
    private SchoolClass? _classToDelete;

    public ObservableCollection<SchoolClass> Classes { get; } = new();

    [ObservableProperty]
    private SchoolClass? _selectedClass;

    [ObservableProperty]
    private string _newClassName = string.Empty;

    [ObservableProperty]
    private bool _isConfirmDeleteVisible;

    [ObservableProperty]
    private string _confirmMessage = string.Empty;

    public MainWindowViewModel()
    {
        // Демо-данные
        Classes.Add(new SchoolClass { Id = _nextId++, Name = "5А" });
        Classes.Add(new SchoolClass { Id = _nextId++, Name = "6Б" });
    }

    [RelayCommand] //Добавление класса
    private void AddClass()
    {
        var name = NewClassName?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        Classes.Add(new SchoolClass { Id = _nextId++, Name = name });
        NewClassName = string.Empty;
    }

    [RelayCommand] //Открытие класса
    private void OpenClass(SchoolClass? schoolClass)
    {

    }

    [RelayCommand] //Подтверждение удаления класса
    private void DeleteClass(SchoolClass? schoolClass)
    {
        if (schoolClass is null) return;

        _classToDelete = schoolClass;
        ConfirmMessage = $"Удалить класс «{schoolClass.Name}»?";
        IsConfirmDeleteVisible = true;
    }

    [RelayCommand] //Удаление класса
    private void ConfirmDelete()
    {
        if (_classToDelete != null)
        {
            Classes.Remove(_classToDelete);
            if (SelectedClass == _classToDelete)
                SelectedClass = null;

            _classToDelete = null;
        }
        IsConfirmDeleteVisible = false;
    }

    [RelayCommand] //Отмена удаления
    private void CancelDelete()
    {
        _classToDelete = null;
        IsConfirmDeleteVisible = false;
    }
}