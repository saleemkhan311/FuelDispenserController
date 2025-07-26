using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelDispenserController;

public static class UserService
{
    public static void AddUser(string dbPath, User user)
    {

        if (string.IsNullOrWhiteSpace(user.Username) ||
    string.IsNullOrWhiteSpace(user.Password) ||
    string.IsNullOrWhiteSpace(user.UserType))
        {
            throw new ArgumentException("All fields (Username, Password, UserType) must be provided.");
        }

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(dbPath);
        connection.Open();

        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Users (Username, Password, RegistrationDate,UserType) VALUES (@username, @password, @registrationDate, @UserType);";
        insertCmd.Parameters.AddWithValue("@username", user.Username);
        insertCmd.Parameters.AddWithValue("@password", user.Password); // Optionally ha
        insertCmd.Parameters.AddWithValue("@registrationDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        insertCmd.Parameters.AddWithValue("@UserType", user.UserType);
        insertCmd.ExecuteNonQuery();
    }

    public static List<User> GetAllUsers(string dbPath)
    {
        var users = new List<User>();

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(dbPath);
        connection.Open();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"SELECT Id, Username, RegistrationDate, UserType FROM Users ORDER BY Id DESC";

        using var reader = selectCmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                RegistrationDate = reader.IsDBNull(2) ? "" : reader.GetString(2),
                UserType = reader.IsDBNull(3) ? "" : reader.GetString(3)
            });
        }

        return users;
    }


    public static void UpdateUser(User user, string dbPath)
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(dbPath);
        connection.Open();

        var updateCmd = connection.CreateCommand();
        updateCmd.CommandText = @"
        UPDATE Users 
        SET Username = $username, Password = $password 
        WHERE Id = $id";

        updateCmd.Parameters.AddWithValue("$username", user.Username);
        updateCmd.Parameters.AddWithValue("$password", user.Password);
        updateCmd.Parameters.AddWithValue("$id", user.Id);

        updateCmd.ExecuteNonQuery();
    }

    public static void DeleteUser(int userId, string dbPath)
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(dbPath);
        connection.Open();
        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Users WHERE Id = $id";
        deleteCmd.Parameters.AddWithValue("$id", userId);
        deleteCmd.ExecuteNonQuery();
    }
}
