using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicJournal.Models;

namespace ElectronicJournal.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private int _nextClassId = 1;
    private int _nextStudentId = 1;

    private SchoolClass? _classToDelete;
    private Student? _studentToDelete;

    private readonly List<SchoolClass> _allClasses = new();

    public ObservableCollection<SchoolClass> Classes { get; } = new();
    public ObservableCollection<Student> FilteredStudents { get; } = new();

    //Свойства навигации
    [ObservableProperty]
    private bool _isClassesViewVisible = true;

    [ObservableProperty]
    private bool _isStudentsViewVisible = false;

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

    public MainWindowViewModel()
    {
        // Демо-данные
        var class5A = new SchoolClass { Id = _nextClassId++, Name = "5А" };
        class5A.Students.Add(new Student { Id = _nextStudentId++, Name = "Иванов Иван" });
        class5A.Students.Add(new Student { Id = _nextStudentId++, Name = "Петров Петр" });

        var class6B = new SchoolClass { Id = _nextClassId++, Name = "6Б" };
        class6B.Students.Add(new Student { Id = _nextStudentId++, Name = "Сидоров Сидор" });

        _allClasses.Add(class5A);
        _allClasses.Add(class6B);

        FilterClasses();
    }

    //Поиск

    private void FilterClasses()
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
    private void FilterStudents()
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

        SelectedClass.Students.Add(new Student { Id = _nextStudentId++, Name = name });

        NewStudentName = string.Empty;
        FilterStudents();
    }

    [RelayCommand] //Открыть ученика
    private void OpenStudent(Student? student)
    {

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
        if (_classToDelete != null)
        {
            // Каскадное удаление
            _allClasses.Remove(_classToDelete);
            FilterClasses();                  

            if (SelectedClass == _classToDelete) GoBackToClasses();
            _classToDelete = null;
        }
        else if (_studentToDelete != null && SelectedClass != null)
        {
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
}