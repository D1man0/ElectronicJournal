using ElectronicJournal.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace ElectronicJournal.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        //Файл базы будет создан рядом с приложением
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "journal.db");
        _connectionString = $"Data Source={dbPath}";
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Login TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Role INTEGER NOT NULL,
                StudentId INTEGER,
                Subject TEXT,
                ManagedClassId INTEGER
            );
            CREATE TABLE IF NOT EXISTS Grades (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                StudentId INTEGER NOT NULL,
                Subject TEXT NOT NULL,
                Date TEXT NOT NULL,
                Value INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Classes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Students (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ClassId INTEGER NOT NULL,
                Name TEXT NOT NULL
            );";
        command.ExecuteNonQuery();

        // Проверяем, есть ли админ, если нет - создаем дефолтного
        var checkAdminCmd = connection.CreateCommand();
        checkAdminCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Role = 0";// 0 = Admin
        var count = Convert.ToInt32(checkAdminCmd.ExecuteScalar());

        if (count == 0)
        {
            var createAdminCmd = connection.CreateCommand();
            createAdminCmd.CommandText = "INSERT INTO Users (Login, PasswordHash, Role) VALUES ('admin', $hash, 0)";
            // Пароль: admin
            createAdminCmd.Parameters.AddWithValue("$hash", PasswordHasher.Hash("admin"));
            createAdminCmd.ExecuteNonQuery();
        }
    }

    public User? Authenticate(string login, string password)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Login, Role, StudentId, Subject, ManagedClassId FROM Users WHERE Login = $login AND PasswordHash = $hash";
        cmd.Parameters.AddWithValue("$login", login);
        cmd.Parameters.AddWithValue("$hash", PasswordHasher.Hash(password));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                Role = (UserRole)reader.GetInt32(2),
                StudentId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                Subject = reader.IsDBNull(4) ? null : reader.GetString(4),
                ManagedClassId = reader.IsDBNull(5) ? null : reader.GetInt32(5)
            };
        }
        return null; //Неверный логин или пароль
    }

    public List<SchoolClass> LoadClasses()//Загрузка классов
    {
        var classes = new List<SchoolClass>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM Classes";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            classes.Add(new SchoolClass { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        }
        reader.Close();

        //Загрузка учеников для классов
        foreach (var cls in classes)
        {
            var cmdStudents = connection.CreateCommand();
            cmdStudents.CommandText = "SELECT Id, Name FROM Students WHERE ClassId = $cid";
            cmdStudents.Parameters.AddWithValue("$cid", cls.Id);

            using var readerStudents = cmdStudents.ExecuteReader();
            while (readerStudents.Read())
            {
                cls.Students.Add(new Student { Id = readerStudents.GetInt32(0), Name = readerStudents.GetString(1) });
            }
        }
        return classes;
    }

    public int AddClass(string name)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO Classes (Name) VALUES ($name); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$name", name);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void DeleteClass(int classId)//Удаление класса
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd1 = connection.CreateCommand();
        cmd1.CommandText = "DELETE FROM Students WHERE ClassId = $cid";
        cmd1.Parameters.AddWithValue("$cid", classId);
        cmd1.ExecuteNonQuery();

        var cmd2 = connection.CreateCommand();
        cmd2.CommandText = "DELETE FROM Classes WHERE Id = $cid";
        cmd2.Parameters.AddWithValue("$cid", classId);
        cmd2.ExecuteNonQuery();
    }
    public int AddStudent(int classId, string name)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO Students (ClassId, Name) VALUES ($cid, $name); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$cid", classId);
        cmd.Parameters.AddWithValue("$name", name);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
    public void DeleteStudent(int studentId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Students WHERE Id = $sid";
        cmd.Parameters.AddWithValue("$sid", studentId);
        cmd.ExecuteNonQuery();
    }

    //Метод для создания ученика в БД
    public void RegisterStudentUser(string studentName, int studentId, out string generatedLogin, out string generatedPassword)
    {
        // Генерируем логин и пароль
        generatedLogin = studentName.Replace(" ", "") + studentId;
        generatedPassword = "1234";

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO Users (Login, PasswordHash, Role, StudentId) VALUES ($login, $hash, 3, $sid)";
        cmd.Parameters.AddWithValue("$login", generatedLogin);
        cmd.Parameters.AddWithValue("$hash", PasswordHasher.Hash(generatedPassword));
        cmd.Parameters.AddWithValue("$sid", studentId);
        cmd.ExecuteNonQuery();
    }

    //Ну и для создания учителя в БД
    public void RegisterStaff(string login, string password, UserRole role, string? subject, int? managedClassId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO Users (Login, PasswordHash, Role, Subject, ManagedClassId) VALUES ($login, $hash, $role, $subject, $managedClassId)";
        cmd.Parameters.AddWithValue("$login", login);
        cmd.Parameters.AddWithValue("$hash", PasswordHasher.Hash(password));
        cmd.Parameters.AddWithValue("$role", (int)role);
        cmd.Parameters.AddWithValue("$subject", subject ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$managedClassId", managedClassId ?? (object)DBNull.Value);

        cmd.ExecuteNonQuery();// Если логин занят - исключение
    }

    //Изменение логина и пароля после авторизации
    public bool UpdateUser(int userId, string newLogin, string? newPasswordHash)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        //Обновление логина и хеша пароля, если пароль пуст то старый пароль
        if (string.IsNullOrEmpty(newPasswordHash))
        {
            cmd.CommandText = "UPDATE Users SET Login = $login WHERE Id = $id";
        }
        else
        {
            cmd.CommandText = "UPDATE Users SET Login = $login, PasswordHash = $hash WHERE Id = $id";
            cmd.Parameters.AddWithValue("$hash", newPasswordHash);
        }

        cmd.Parameters.AddWithValue("$login", newLogin);
        cmd.Parameters.AddWithValue("$id", userId);

        try
        {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (SqliteException)
        {
            return false;
        }
    }
    public List<User> GetAllUsers()//Считывание базы пользователей
    {
        var users = new List<User>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Login, Role, StudentId, Subject, ManagedClassId FROM Users";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                Role = (UserRole)reader.GetInt32(2),
                StudentId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                Subject = reader.IsDBNull(4) ? null : reader.GetString(4),
                ManagedClassId = reader.IsDBNull(5) ? null : reader.GetInt32(5)
            });
        }
        return users;
    }
    public void DeleteUser(int id)//Удаление пользователя
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
    public void DeleteStudentUser(int studentId)//Удалить конкретного ученика
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE StudentId = $sid";
        cmd.Parameters.AddWithValue("$sid", studentId);
        cmd.ExecuteNonQuery();
    }

    public void DeleteUsersByStudentIds(List<int> studentIds)//Удалить весь класс
    {
        if (studentIds.Count == 0) return;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var ids = string.Join(",", studentIds);
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DELETE FROM Users WHERE StudentId IN ({ids})";
        cmd.ExecuteNonQuery();
    }

    //Получить все оценки ученика
    public List<Grade> GetGradesForStudent(int studentId)
    {
        var grades = new List<Grade>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, StudentId, Subject, Date, Value FROM Grades WHERE StudentId = $sid ORDER BY Date";
        cmd.Parameters.AddWithValue("$sid", studentId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            grades.Add(new Grade
            {
                Id = reader.GetInt32(0),
                StudentId = reader.GetInt32(1),
                Subject = reader.GetString(2),
                Date = DateTime.Parse(reader.GetString(3)),
                Value = reader.GetInt32(4)
            });
        }
        return grades;
    }
    public void UpsertGrade(int studentId, string subject, DateTime date, string valueStr)//Обновить оценку
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var dateStr = date.ToString("yyyy-MM-dd");

        //Если строка пустая
        if (string.IsNullOrWhiteSpace(valueStr))
        {
            var delCmd = connection.CreateCommand();
            delCmd.CommandText = "DELETE FROM Grades WHERE StudentId = $sid AND Subject = $subject AND Date = $date";
            delCmd.Parameters.AddWithValue("$sid", studentId);
            delCmd.Parameters.AddWithValue("$subject", subject);
            delCmd.Parameters.AddWithValue("$date", dateStr);
            delCmd.ExecuteNonQuery();
            return;
        }

        if (!int.TryParse(valueStr, out int val)) return;

        //Есть ли уже оценка на эту дату
        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Grades WHERE StudentId = $sid AND Subject = $subject AND Date = $date";
        checkCmd.Parameters.AddWithValue("$sid", studentId);
        checkCmd.Parameters.AddWithValue("$subject", subject);
        checkCmd.Parameters.AddWithValue("$date", dateStr);
        var count = Convert.ToInt32(checkCmd.ExecuteScalar());

        var cmd = connection.CreateCommand();
        if (count > 0)
        {
            //Обновление
            cmd.CommandText = "UPDATE Grades SET Value = $val WHERE StudentId = $sid AND Subject = $subject AND Date = $date";
        }
        else
        {
            //Вставка новой
            cmd.CommandText = "INSERT INTO Grades (StudentId, Subject, Date, Value) VALUES ($sid, $subject, $date, $val)";
        }

        cmd.Parameters.AddWithValue("$sid", studentId);
        cmd.Parameters.AddWithValue("$subject", subject);
        cmd.Parameters.AddWithValue("$date", dateStr);
        cmd.Parameters.AddWithValue("$val", val);
        cmd.ExecuteNonQuery();
    }
}