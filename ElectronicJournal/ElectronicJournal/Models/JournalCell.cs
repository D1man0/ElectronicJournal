using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectronicJournal.Models;

public partial class JournalCell : ObservableObject
{
    public DateTime Date { get; set; }

    //Оценка (если null, значит оценки нет)
    [ObservableProperty] private string _value = string.Empty;
}
