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
                StudentId INTEGER
            );";
        command.ExecuteNonQuery();

        // Проверяем, есть ли админ, если нет - создаем дефолтного
        var checkAdminCmd = connection.CreateCommand();
        checkAdminCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Role = 0"; // 0 = Admin
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
        cmd.CommandText = "SELECT Id, Login, Role, StudentId FROM Users WHERE Login = $login AND PasswordHash = $hash";
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
                StudentId = reader.IsDBNull(3) ? null : reader.GetInt32(3)
            };
        }
        return null; //Неверный логин или пароль
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
        cmd.CommandText = "INSERT INTO Users (Login, PasswordHash, Role, StudentId) VALUES ($login, $hash, 2, $sid)";
        cmd.Parameters.AddWithValue("$login", generatedLogin);
        cmd.Parameters.AddWithValue("$hash", PasswordHasher.Hash(generatedPassword));
        cmd.Parameters.AddWithValue("$sid", studentId);
        cmd.ExecuteNonQuery();
    }

    //Ну и для создания учителя в БД
    public void RegisterTeacher(string login, string password)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO Users (Login, PasswordHash, Role) VALUES ($login, $hash, 1)";
        cmd.Parameters.AddWithValue("$login", login);
        cmd.Parameters.AddWithValue("$hash", PasswordHasher.Hash(password));

        cmd.ExecuteNonQuery(); // Если логин занят - исключение
    }

    //Изменение логина и пароля после авторизации
    public bool UpdateUser(int userId, string newLogin, string newPasswordHash)
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
        cmd.CommandText = "SELECT Id, Login, Role, StudentId FROM Users";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                Role = (UserRole)reader.GetInt32(2),
                StudentId = reader.IsDBNull(3) ? null : reader.GetInt32(3)
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
}