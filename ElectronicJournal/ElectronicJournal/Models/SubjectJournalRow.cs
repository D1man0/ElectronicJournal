using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace ElectronicJournal.Models;

public class SubjectJournalRow
{
    public string Subject { get; set; } = string.Empty;
    public ObservableCollection<JournalCell> Cells { get; set; } = new();
}
