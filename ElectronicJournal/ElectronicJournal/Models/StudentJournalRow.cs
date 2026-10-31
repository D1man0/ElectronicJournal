using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace ElectronicJournal.Models;

public class StudentJournalRow
{
    public Student Student { get; set; } = null!;
    public ObservableCollection<JournalCell> Cells { get; set; } = new();
}
