using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicJournal.Models;
using ElectronicJournal.Services;

namespace ElectronicJournal.ViewModels;

public partial class ClassJournalViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    public event Action? BackRequested;

    public User? CurrentTeacher { get; set; }
    public SchoolClass? CurrentClass { get; set; }

    //Список дат
    public List<DateTime> Dates { get; } = new();

    //Свойства для выбора диапозона дат
    [ObservableProperty] private DateTime? _startDate = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime? _endDate = DateTime.Today.AddDays(7);

    //Строки таблицы
    public ObservableCollection<StudentJournalRow> Rows { get; } = new();

    public ClassJournalViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public void LoadData()
    {
        Rows.Clear();
        Dates.Clear();

        if (CurrentClass == null || CurrentTeacher?.Subject == null)
        {
            OnPropertyChanged(nameof(Rows));
            return;
        }

        //Если даты не выбраны
        var start = StartDate ?? DateTime.Today.AddDays(-7);
        var end = EndDate ?? DateTime.Today.AddDays(7);

        //Защита от слишком большого периода 
        if ((end - start).TotalDays > 60) end = start.AddDays(60);
        if (end < start) end = start;

        //Генерируем даты
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            Dates.Add(date);
        }

        //Проходимся по каждому ученику
        foreach (var student in CurrentClass.Students)
        {
            var row = new StudentJournalRow { Student = student };

            //Получаем оценки ученика по предмету учителя
            var studentGrades = _dbService.GetGradesForStudent(student.Id)
                .Where(g => g.Subject == CurrentTeacher.Subject)
                .ToList();

            //Заполняем ячейки для каждой даты
            foreach (var date in Dates)
            {
                var grade = studentGrades.FirstOrDefault(g => g.Date.Date == date.Date);
                row.Cells.Add(new JournalCell
                {
                    Date = date,
                    Value = grade?.Value.ToString() ?? ""
                });
            }
            //Расчет среднего балла
            var validGrades = row.Cells
                .Where(c => !string.IsNullOrWhiteSpace(c.Value) && int.TryParse(c.Value, out _))
                .Select(c => int.Parse(c.Value))
                .ToList();

            if (validGrades.Any())
            {
                row.AverageGrade = validGrades.Average().ToString("F2"); //Формат: 4.50
            }

            Rows.Add(row);
        }
        OnPropertyChanged(nameof(Rows));
        OnPropertyChanged(nameof(Dates));
    }

    [RelayCommand]
    private void RefreshJournal()
    {
        LoadData();
    }
    //Автомаическое обновление
    partial void OnStartDateChanged(DateTime? value) => RefreshJournal();
    partial void OnEndDateChanged(DateTime? value) => RefreshJournal();

    [RelayCommand]
    private void GoBack()
    {
        BackRequested?.Invoke();
    }

    [RelayCommand]
    private void SaveGrade(StudentJournalRow row)
    {
        if (CurrentTeacher?.Subject == null) return;

        // Проходим по всем ячейкам в строке и сохраняем/обновляем
        for (int i = 0; i < row.Cells.Count; i++)
        {
            var cell = row.Cells[i];
            _dbService.UpsertGrade(row.Student.Id, CurrentTeacher.Subject, cell.Date, cell.Value);
        }
        var validGrades = row.Cells
            .Where(c => !string.IsNullOrWhiteSpace(c.Value) && int.TryParse(c.Value, out _))
            .Select(c => int.Parse(c.Value))
            .ToList();

        if (validGrades.Any())
        {
            row.AverageGrade = validGrades.Average().ToString("F2");//Формат: 4.50
        }
        else
        {
            row.AverageGrade = ""; //Если все оценки стерли
        }
    }
}