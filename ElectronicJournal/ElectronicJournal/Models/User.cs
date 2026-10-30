using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicJournal.Models;
public enum UserRole //Роли пользователей
{
    Admin,
    Teacher,
    Student
}
public class User //Пользователь
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    //Для учеников - ссылка на их запись в таблице учеников
    public int? StudentId { get; set; }
}
