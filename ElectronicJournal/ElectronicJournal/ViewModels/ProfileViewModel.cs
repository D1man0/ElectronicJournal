using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicJournal.Models;
using ElectronicJournal.Services;

namespace ElectronicJournal.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;

    //Ссылка на текущего пользователя (кто залогинен)
    public User? CurrentUser { get; set; }

    //Событие, на которое подпишется главное окно
    public event Action? LogoutRequested;

    //Свойства профиля
    [ObservableProperty] private bool _isProfileVisible = false;
    [ObservableProperty] private string _profileNewLogin = string.Empty;
    [ObservableProperty] private string _profileNewPassword = string.Empty;
    [ObservableProperty] private string _profileMessage = string.Empty;

    public ProfileViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    [RelayCommand]
    private void OpenProfile()
    {
        if (CurrentUser == null) return;
        ProfileNewLogin = CurrentUser.Login;
        ProfileNewPassword = string.Empty;
        ProfileMessage = string.Empty;
        IsProfileVisible = true;
    }

    [RelayCommand]
    private void CloseProfile()
    {
        IsProfileVisible = false;
        ProfileMessage = string.Empty;
    }

    [RelayCommand]
    private void SaveProfile()
    {
        if (CurrentUser == null) return;

        ProfileMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(ProfileNewLogin))
        {
            ProfileMessage = "Логин не может быть пустым";
            return;
        }

        string? newHash = string.IsNullOrWhiteSpace(ProfileNewPassword) ? null : PasswordHasher.Hash(ProfileNewPassword);

        bool success = _dbService.UpdateUser(CurrentUser.Id, ProfileNewLogin, newHash);
        if (success)
        {
            CurrentUser.Login = ProfileNewLogin;
            ProfileMessage = "Данные успешно обновлены!";
            ProfileNewPassword = string.Empty;
        }
        else
        {
            ProfileMessage = "Ошибка: этот логин уже занят другим пользователем.";
        }
    }

    [RelayCommand]
    private void Logout()
    {
        IsProfileVisible = false;
        LogoutRequested?.Invoke();
    }
}
