using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ElectronicJournal.Models;

public partial class SubjectJournalRow : ObservableObject
{
    public string Subject { get; set; } = string.Empty;
    public ObservableCollection<JournalCell> Cells { get; set; } = new();

    [ObservableProperty]
    private string _averageGrade = "";
}
