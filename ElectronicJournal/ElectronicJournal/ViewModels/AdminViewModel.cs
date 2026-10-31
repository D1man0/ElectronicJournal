using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicJournal.Models;
using ElectronicJournal.Services;

namespace ElectronicJournal.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private readonly List<SchoolClass> _allClasses;
    private User? _userToDelete;

    public ObservableCollection<User> Users { get; } = new();
    public User? CurrentUser { get; set; }
    public event Action<User>? UserDeleted;

    //Списки для ComboBox
    public List<string> Roles { get; } = new() { "Учитель", "Классный руководитель" };
    public List<string> Subjects { get; } = SubjectCatalog.AllSubjects;

    [ObservableProperty] private ObservableCollection<SchoolClass> _classes = new();

    [ObservableProperty] private string _newTeacherLogin = string.Empty;
    [ObservableProperty] private string _newTeacherPassword = string.Empty;
    [ObservableProperty] private string _adminMessage = string.Empty;
    [ObservableProperty] private User? _selectedUser;

    //Свойства для формы
    [ObservableProperty] private string _selectedRole = "Учитель";
    [ObservableProperty] private string _selectedSubject = SubjectCatalog.AllSubjects.First();
    [ObservableProperty] private SchoolClass? _selectedClass;
    [ObservableProperty] private bool _isTeacherRoleSelected = true;
    [ObservableProperty] private bool _isClassTeacherRoleSelected = false;

    //Свойства для удаления
    [ObservableProperty] private bool _isConfirmDeleteVisible;
    [ObservableProperty] private string _confirmDeleteMessage = string.Empty;

    public AdminViewModel(DatabaseService dbService, List<SchoolClass> allClasses)
    {
        _dbService = dbService;
        _allClasses = allClasses;
        LoadUsers();
    }

    public void LoadUsers()
    {
        Users.Clear();
        foreach (var user in _dbService.GetAllUsers())
        {
            Users.Add(user);
        }

        //Обновление списка класса
        Classes.Clear();
        foreach (var c in _allClasses) Classes.Add(c);
    }
    partial void OnSelectedRoleChanged(string value)
    {
        if (value == "Учитель")
        {
            IsTeacherRoleSelected = true;
            IsClassTeacherRoleSelected = false;
            SelectedClass = null;
        }
        else
        {
            IsTeacherRoleSelected = false;
            IsClassTeacherRoleSelected = true;
        }
    }

        [RelayCommand]
    private void AddTeacher()
    {
        AdminMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(NewTeacherLogin) || string.IsNullOrWhiteSpace(NewTeacherPassword))
        {
            AdminMessage = "Заполните логин и пароль";
            return;
        }

        var role = SelectedRole == "Учитель" ? UserRole.Teacher : UserRole.ClassTeacher;

        if (role == UserRole.ClassTeacher && SelectedClass == null)
        {
            AdminMessage = "Выберите класс для классного руководителя";
            return;
        }

        try
        {
            string? subject = role == UserRole.Teacher ? SelectedSubject : null;
            int? classId = role == UserRole.ClassTeacher ? SelectedClass?.Id : null;

            _dbService.RegisterStaff(NewTeacherLogin, NewTeacherPassword, role, subject, classId);

            AdminMessage = $"Сотрудник {NewTeacherLogin} успешно создан!";
            NewTeacherLogin = string.Empty;
            NewTeacherPassword = string.Empty;
            LoadUsers();
        }
        catch (Exception)
        {
            AdminMessage = "Ошибка: этот логин уже существует.";
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
                foreach (var schoolClass in _allClasses)
                {
                    var student = schoolClass.Students.FirstOrDefault(s => s.Id == _userToDelete.StudentId);
                    if (student != null)
                    {
                        schoolClass.Students.Remove(student);
                        break;
                    }
                }
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