using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicJournal.Models;
using ElectronicJournal.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ElectronicJournal.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private int _nextClassId = 1;
    private int _nextStudentId = 1;

    public ProfileViewModel Profile { get; }
    public AdminViewModel Admin { get; }
    public StudentProfileViewModel StudentProfile { get; }

    private SchoolClass? _classToDelete;
    private Student? _studentToDelete;

    private readonly List<SchoolClass> _allClasses = new();

    public ObservableCollection<SchoolClass> Classes { get; } = new();
    public ObservableCollection<Student> FilteredStudents { get; } = new();
    private readonly DatabaseService _dbService;

    #region свойства

    //Свойства авторизации
    [ObservableProperty]
    private bool _isLoginVisible = true;

    [ObservableProperty]
    private string _loginInput = string.Empty;

    [ObservableProperty]
    private string _passwordInput = string.Empty;

    [ObservableProperty]
    private string _loginError = string.Empty;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private bool _isTeacherViewVisible = false;

    //Свойства навигации
    [ObservableProperty]
    private bool _isClassesViewVisible = true;

    [ObservableProperty]
    private bool _isStudentsViewVisible = false;

    [ObservableProperty]
    private bool _isAdminViewVisible = false;

    [ObservableProperty]
    private bool _isStudentProfileViewVisible = false;

    //Свойства классов
    [ObservableProperty]
    private SchoolClass? _selectedClass;

    [ObservableProperty]
    private string _newClassName = string.Empty;

    //Свойства учеников
    [ObservableProperty]
    private Student? _selectedStudent;

    [ObservableProperty]
    private string _newStudentName = string.Empty;

    //Свойства оверлея
    [ObservableProperty]
    private bool _isConfirmDeleteVisible;

    [ObservableProperty]
    private string _confirmMessage = string.Empty;

    #endregion

    public MainWindowViewModel(DatabaseService dbService)
    {
        _dbService = dbService;

        Profile = new ProfileViewModel(dbService);
        Profile.LogoutRequested += HandleLogout;

        Admin = new AdminViewModel(dbService);
        Admin.UserDeleted += HandleAdminDeletedUser;

        StudentProfile = new StudentProfileViewModel();
        StudentProfile.BackRequested += HandleStudentProfileBack;

        FilterClasses();
    }

    //Авторизация

    [RelayCommand]
    private void Login()//Вход
    {
        LoginError = string.Empty;

        var user = _dbService.Authenticate(LoginInput, PasswordInput);
        if (user == null)
        {
            LoginError = "Неверный логин или пароль";
            return;
        }

        CurrentUser = user;
        Profile.CurrentUser = user;
        IsLoginVisible = false;
        
        //Сброс всех экранов перед входом
        IsClassesViewVisible = false;
        IsStudentsViewVisible = false;
        IsAdminViewVisible = false;
        IsStudentProfileViewVisible = false;
        IsTeacherViewVisible = false;

        if (user.Role == UserRole.Admin)//Если роль админ
        {
            IsAdminViewVisible = true;
            IsTeacherViewVisible = false;
            IsClassesViewVisible = false;
            IsStudentsViewVisible = false;

            Admin.CurrentUser = user;
            Admin.LoadUsers();
        }
        else if (user.Role == UserRole.Teacher)//Если учитель
        {
            IsAdminViewVisible = false;
            IsTeacherViewVisible = true;
            IsClassesViewVisible = true;
            IsStudentsViewVisible = false;
        }
        else if (user.Role == UserRole.Student)//Если ученик
        {
            IsTeacherViewVisible = false;
            IsClassesViewVisible = false;
            IsStudentsViewVisible = false;
            IsAdminViewVisible = false;

            Student? loggedInStudent = null;
            SchoolClass? studentClass = null;

            foreach (var c in _allClasses)
            {
                var s = c.Students.FirstOrDefault(x => x.Id == user.StudentId);
                if (s != null)
                {
                    loggedInStudent = s;
                    studentClass = c;
                    break;
                }
            }

            if (loggedInStudent != null && studentClass != null)
            {
                StudentProfile.CurrentStudent = loggedInStudent;
                StudentProfile.CurrentClass = studentClass;
                StudentProfile.CanGoBack = false;
                IsStudentProfileViewVisible = true;
            }
            else
            {
                LoginError = "Ученик не найден в базе данных классов.";
                IsLoginVisible = true;
            }
        }
    }
    private void HandleLogout()//Выход
    {
        CurrentUser = null;
        LoginInput = string.Empty;
        PasswordInput = string.Empty;
        LoginError = string.Empty;

        IsTeacherViewVisible = false;
        IsAdminViewVisible = false;
        IsClassesViewVisible = true;
        IsStudentsViewVisible = false;
        IsStudentProfileViewVisible = false;

        IsLoginVisible = true;
    }
    private void HandleStudentProfileBack()
    {
        IsStudentProfileViewVisible = false;
        IsStudentsViewVisible = true;
    }

    //Поиск

    private void FilterClasses() //Поиск класса
    {
        Classes.Clear();
        var search = NewClassName?.Trim().ToLower() ?? "";

        foreach (var c in _allClasses)
        {
            if (string.IsNullOrWhiteSpace(search) || c.Name.ToLower().Contains(search))
                Classes.Add(c);
        }
    }
    partial void OnNewClassNameChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FilterClasses();
        }
    }
    private void FilterStudents() //Поиск ученика
    {
        FilteredStudents.Clear();
        if (SelectedClass == null) return;

        var search = NewStudentName?.Trim().ToLower() ?? "";

        foreach (var s in SelectedClass.Students)
        {
            if (string.IsNullOrWhiteSpace(search) || s.Name.ToLower().Contains(search))
                FilteredStudents.Add(s);
        }
    }
    partial void OnNewStudentNameChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FilterStudents();
        }
    }

    [RelayCommand]
    private void SearchClasses() => FilterClasses();

    [RelayCommand]
    private void SearchStudents() => FilterStudents();

    //Классы

    [RelayCommand] //Добавление класса
    private void AddClass()
    {
        var name = NewClassName?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        var newClass = new SchoolClass { Id = _nextClassId++, Name = name };
        _allClasses.Add(newClass);

        NewClassName = string.Empty;
        FilterClasses();
    }

    [RelayCommand] //Открытие класса
    private void OpenClass(SchoolClass? schoolClass)
    {
        if (schoolClass == null) return;
        SelectedClass = schoolClass;

        NewStudentName = string.Empty;
        FilterStudents();              

        IsClassesViewVisible = false;
        IsStudentsViewVisible = true;
    }

    [RelayCommand] //Подтверждение удаления класса
    private void DeleteClass(SchoolClass? schoolClass)
    {
        if (schoolClass is null) return;
        _classToDelete = schoolClass;
        _studentToDelete = null;
        ConfirmMessage = $"Удалить класс «{schoolClass.Name}» и всех его учеников?";
        IsConfirmDeleteVisible = true;
    }

    [RelayCommand] //Возвращение к классам
    private void GoBackToClasses()
    {
        IsStudentsViewVisible = false;
        IsClassesViewVisible = true;
        SelectedClass = null;
        SelectedStudent = null;

        NewClassName = string.Empty;
        FilterClasses();            
    }

    //Ученики

    [RelayCommand] //Добавить ученика
    private void AddStudent()
    {
        var name = NewStudentName?.Trim();
        if (string.IsNullOrWhiteSpace(name) || SelectedClass == null) return;

        var newStudent = new Student { Id = _nextStudentId++, Name = name };
        SelectedClass.Students.Add(newStudent);

        //Логин и пароль генерируются автоматически, пароль по умолчанию 1234
        _dbService.RegisterStudentUser(newStudent.Name, newStudent.Id, out string generatedLogin, out string generatedPassword);
        //Вывод в консоль для проверки
        System.Diagnostics.Debug.WriteLine($"Создан ученик: {name}. Логин: {generatedLogin}, Пароль: {generatedPassword}");

        NewStudentName = string.Empty;
        FilterStudents();
    }

    [RelayCommand] //Открыть ученика
    private void OpenStudent(Student? student)
    {
        if (student == null) return;

        StudentProfile.CurrentStudent = student;
        StudentProfile.CurrentClass = SelectedClass;
        StudentProfile.CanGoBack = true;

        IsStudentsViewVisible = false;
        IsStudentProfileViewVisible = true;
    }

    [RelayCommand] //Удалить ученика
    private void DeleteStudent(Student? student)
    {
        if (student is null) return;
        _studentToDelete = student;
        _classToDelete = null;
        ConfirmMessage = $"Удалить ученика «{student.Name}»?";
        IsConfirmDeleteVisible = true;
    }

    //Оверлей

    [RelayCommand] //Подтверждение удаления
    private void ConfirmDelete()
    {
        if (_classToDelete != null)//Удаление класса
        {
            var studentIds = _classToDelete.Students.Select(s => s.Id).ToList();
            _dbService.DeleteUsersByStudentIds(studentIds);

            _allClasses.Remove(_classToDelete);
            FilterClasses();

            if (SelectedClass == _classToDelete) GoBackToClasses();
            _classToDelete = null;
        }
        else if (_studentToDelete != null && SelectedClass != null)//Удаление конкретного ученика
        {
            _dbService.DeleteStudentUser(_studentToDelete.Id);

            SelectedClass.Students.Remove(_studentToDelete);
            FilterStudents();

            if (SelectedStudent == _studentToDelete) SelectedStudent = null;
            _studentToDelete = null;
        }

        IsConfirmDeleteVisible = false;
    }

    [RelayCommand] //Отмена удаления
    private void CancelDelete()
    {
        _classToDelete = null;
        _studentToDelete = null;
        IsConfirmDeleteVisible = false;
    }
    //Вызывается, когда Админ удаляет пользователя
    private void HandleAdminDeletedUser(User user)
    {
        if (user.StudentId == null) return;

        foreach (var schoolClass in _allClasses)
        {
            var student = schoolClass.Students.FirstOrDefault(s => s.Id == user.StudentId);
            if (student != null)
            {
                schoolClass.Students.Remove(student);

                if (SelectedClass == schoolClass)
                {
                    FilterStudents();
                    if (SelectedStudent == student) SelectedStudent = null;
                }
                break;
            }
        }
    }
}