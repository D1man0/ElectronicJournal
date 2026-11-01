using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ElectronicJournal.Models;

public partial class StudentJournalRow : ObservableObject
{
    public Student Student { get; set; } = null!;
    public ObservableCollection<JournalCell> Cells { get; set; } = new();

    [ObservableProperty]
    private string _averageGrade = "";
}
