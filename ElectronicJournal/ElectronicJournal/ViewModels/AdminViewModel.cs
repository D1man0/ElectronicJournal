using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicJournal.Models;
using ElectronicJournal.Services;

namespace ElectronicJournal.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private User? _userToDelete;

    //Список пользователей
    public ObservableCollection<User> Users { get; } = new();

    public User? CurrentUser { get; set; } //Админ не может удалить себя

    //Свойства админа
    [ObservableProperty] private string _newTeacherLogin = string.Empty;
    [ObservableProperty] private string _newTeacherPassword = string.Empty;
    [ObservableProperty] private string _adminMessage = string.Empty;
    [ObservableProperty] private User? _selectedUser;

    public event Action<User>? UserDeleted;

    //Для оверлея удаления
    [ObservableProperty] private bool _isConfirmDeleteVisible;
    [ObservableProperty] private string _confirmDeleteMessage = string.Empty;

    public AdminViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
        LoadUsers();//Загрузка списка при старте
    }

    public void LoadUsers()
    {
        Users.Clear();
        var allUsers = _dbService.GetAllUsers();
        foreach (var user in allUsers)
        {
            Users.Add(user);
        }
    }

    [RelayCommand]
    private void AddTeacher()
    {
        AdminMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(NewTeacherLogin) || string.IsNullOrWhiteSpace(NewTeacherPassword))
        {
            AdminMessage = "Заполните все поля";
            return;
        }

        try
        {
            _dbService.RegisterTeacher(NewTeacherLogin, NewTeacherPassword);
            AdminMessage = $"Учитель {NewTeacherLogin} успешно создан!";
            NewTeacherLogin = string.Empty;
            NewTeacherPassword = string.Empty;

            LoadUsers(); //Обновление списка
        }
        catch (Exception)
        {
            AdminMessage = "Ошибка: этот логин уже существует в системе.";
        }
    }

    //Удаление пользователя
    [RelayCommand]
    private void DeleteUser(User? user)
    {
        if (user is null) return;

        if (user.Id == CurrentUser?.Id)
        {
            AdminMessage = "Вы не можете удалить самого себя!";
            return;
        }

        _userToDelete = user;
        ConfirmDeleteMessage = $"Удалить пользователя «{user.Login}»?";
        IsConfirmDeleteVisible = true;
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        if (_userToDelete != null)
        {
            _dbService.DeleteUser(_userToDelete.Id);
            Users.Remove(_userToDelete);

            if (_userToDelete.StudentId.HasValue)
            {
                UserDeleted?.Invoke(_userToDelete);
            }

            _userToDelete = null;
        }
        IsConfirmDeleteVisible = false;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        _userToDelete = null;
        IsConfirmDeleteVisible = false;
    }
}