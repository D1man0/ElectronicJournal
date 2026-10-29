using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ElectronicJournal.Models;

public partial class SchoolClass : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _name = string.Empty;
    public ObservableCollection<Student> Students { get; } = new(); // Список учеников связанных с классом
}
