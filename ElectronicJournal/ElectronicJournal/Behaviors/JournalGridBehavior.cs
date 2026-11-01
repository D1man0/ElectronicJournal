using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates; // <--- Добавили для FuncDataTemplate
using Avalonia.Data;
using Avalonia.Layout;
using ElectronicJournal.Models;
using ElectronicJournal.ViewModels;
using System;

namespace ElectronicJournal.Behaviors;

public static class JournalGridBehavior
{
    public static readonly AttachedProperty<ClassJournalViewModel?> JournalViewModelProperty =
        AvaloniaProperty.RegisterAttached<DataGrid, ClassJournalViewModel?>("JournalViewModel", typeof(JournalGridBehavior));

    public static void SetJournalViewModel(DataGrid element, ClassJournalViewModel? value) => element.SetValue(JournalViewModelProperty, value);
    public static ClassJournalViewModel? GetJournalViewModel(DataGrid element) => element.GetValue(JournalViewModelProperty);

    static JournalGridBehavior()
    {
        JournalViewModelProperty.Changed.AddClassHandler<DataGrid>((grid, e) =>
        {
            if (e.NewValue is ClassJournalViewModel vm)
            {
                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(ClassJournalViewModel.Rows))
                    {
                        GenerateColumns(grid, vm);
                    }
                };

                GenerateColumns(grid, vm);
            }
        });
    }

    private static void GenerateColumns(DataGrid grid, ClassJournalViewModel vm)
    {
        grid.Columns.Clear();

        //Первая колонка - ФИО ученика
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Ученик",
            Binding = new Binding("Student.Name"),
            Width = new DataGridLength(150)
        });
        grid.FrozenColumnCount = 1;

        //Динамические колонки с датами
        foreach (var date in vm.Dates)
        {
            var index = vm.Dates.IndexOf(date);

            var cellTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                var cb = new ComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = Avalonia.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    ItemsSource = new[] { "", "2", "3", "4", "5" }, //Доступные оценки
                    Padding = new Thickness(4, 0),
                    MinHeight = 30
                };
                //Привязка выбранного к ячейке
                cb.Bind(ComboBox.SelectedItemProperty, new Binding($"Cells[{index}].Value"));

                //Сохранение в БД
                cb.SelectionChanged += (s, e) =>
                {
                    if (item is StudentJournalRow row)
                    {
                        vm.SaveGradeCommand.Execute(row);
                    }
                };

                return cb;
            });

            //Месяц сверху, День снизу
            var headerStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            headerStack.Children.Add(new TextBlock { Text = date.ToString("MMM"), FontSize = 10, Foreground = Avalonia.Media.Brushes.Gray });
            headerStack.Children.Add(new TextBlock { Text = date.ToString("dd"), FontWeight = Avalonia.Media.FontWeight.Bold });

            var col = new DataGridTemplateColumn
            {
                Header = headerStack,
                Width = new DataGridLength(60),
                CellTemplate = cellTemplate
            };

            grid.Columns.Add(col);
        }
        //Последняя колонка - Средний балл
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Ср. балл",
            Binding = new Binding("AverageGrade"),
            Width = new DataGridLength(80),
            IsReadOnly = true
        });
    }
}