using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicJournal.Models
{
    public static class SubjectCatalog
    {
        //Список предметов
        public static readonly List<string> AllSubjects = new()
    {
        "Математика", "Русский язык", "Литература",
        "Физика", "Химия", "История", "Английский язык",
        "Информатика", "Биология", "География"
    };
    }
}
