using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicJournal.Models;
using ElectronicJournal.Services;

namespace ElectronicJournal.ViewModels;

public partial class StudentProfileViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    public event Action? BackRequested;

    public User? CurrentUser { get; set; }

    [ObservableProperty] private Student? _currentStudent;
    [ObservableProperty] private SchoolClass? _currentClass;
    [ObservableProperty] private bool _canGoBack = true;
    [ObservableProperty] private DateTime? _startDate = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime? _endDate = DateTime.Today.AddDays(7);

    //Данные для матрицы
    public List<DateTime> Dates { get; } = new();
    public ObservableCollection<SubjectJournalRow> Rows { get; } = new();

    public StudentProfileViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public void LoadGrades()
    {
        Rows.Clear();
        Dates.Clear();

        if (CurrentStudent == null) return;

        var start = StartDate ?? DateTime.Today.AddDays(-7);
        var end = EndDate ?? DateTime.Today.AddDays(7);

        if ((end - start).TotalDays > 60) end = start.AddDays(60);
        if (end < start) end = start;

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            Dates.Add(date);
        }

        //Получеие оценок ученика
        var studentGrades = _dbService.GetGradesForStudent(CurrentStudent.Id);

        foreach (var subject in SubjectCatalog.AllSubjects)
        {
            var row = new SubjectJournalRow { Subject = subject };

            //Оценки только по текущему предмету
            var subjectGrades = studentGrades.Where(g => g.Subject == subject).ToList();

            //Заполнение
            foreach (var date in Dates)
            {
                var grade = subjectGrades.FirstOrDefault(g => g.Date.Date == date.Date);
                row.Cells.Add(new JournalCell
                {
                    Date = date,
                    Value = grade?.Value.ToString() ?? ""
                });
            }
            Rows.Add(row);
        }

        OnPropertyChanged(nameof(Rows));
        OnPropertyChanged(nameof(Dates));
    }

    [RelayCommand]
    private void RefreshGrades()
    {
        LoadGrades();
    }
    //Автоматическое обновление
    partial void OnStartDateChanged(DateTime? value) => RefreshGrades();
    partial void OnEndDateChanged(DateTime? value) => RefreshGrades();

    [RelayCommand]
    private void GoBack()
    {
        BackRequested?.Invoke();
    }
}