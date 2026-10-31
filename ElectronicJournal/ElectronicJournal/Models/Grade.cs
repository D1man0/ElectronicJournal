using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicJournal.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Value { get; set; } //оценка (2, 3, 4, 5)
    }
}
