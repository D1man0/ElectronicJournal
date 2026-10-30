using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicJournal.Models;

namespace ElectronicJournal.ViewModels;

public partial class StudentProfileViewModel : ObservableObject
{
    public event Action? BackRequested;

    [ObservableProperty] private Student? _currentStudent;
    [ObservableProperty] private SchoolClass? _currentClass;
    [ObservableProperty] private bool _canGoBack = true;

    [RelayCommand]
    private void GoBack()
    {
        BackRequested?.Invoke();
    }
}