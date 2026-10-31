using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using ElectronicJournal.ViewModels;

namespace ElectronicJournal.Behaviors;

public static class StudentJournalGridBehavior
{
    public static readonly AttachedProperty<StudentProfileViewModel?> JournalViewModelProperty =
        AvaloniaProperty.RegisterAttached<DataGrid, StudentProfileViewModel?>("JournalViewModel", typeof(StudentJournalGridBehavior));

    public static void SetJournalViewModel(DataGrid element, StudentProfileViewModel? value) => element.SetValue(JournalViewModelProperty, value);
    public static StudentProfileViewModel? GetJournalViewModel(DataGrid element) => element.GetValue(JournalViewModelProperty);

    static StudentJournalGridBehavior()
    {
        JournalViewModelProperty.Changed.AddClassHandler<DataGrid>((grid, e) =>
        {
            if (e.NewValue is StudentProfileViewModel vm)
            {
                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(StudentProfileViewModel.Rows))
                    {
                        GenerateColumns(grid, vm);
                    }
                };
                GenerateColumns(grid, vm);
            }
        });
    }

    private static void GenerateColumns(DataGrid grid, StudentProfileViewModel vm)
    {
        grid.Columns.Clear();

        //Первая колонка - Предмет
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Предмет",
            Binding = new Binding("Subject"),
            Width = new DataGridLength(120)
        });
        grid.FrozenColumnCount = 1;

        //Динамические колонки с датами
        foreach (var date in vm.Dates)
        {
            var index = vm.Dates.IndexOf(date);

            var cellTemplate = new FuncDataTemplate<object>((item, _) =>
            {
                var tb = new TextBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = Avalonia.Media.TextAlignment.Center,
                    Background = Avalonia.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    IsReadOnly = true // Ученик не может редактировать оценки
                };

                tb.Bind(TextBox.TextProperty, new Binding($"Cells[{index}].Value"));
                return tb;
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
    }
}