using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using FuelDispenserController.Models;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using OfficeOpenXml.Packaging.Ionic.Zlib;
using Windows.Storage;
using Windows.System;
using static SQLite.TableMapping;
public static class DatabaseHelper
{
    // private const string ConnectionString = "Data Source=C:\\Database\\FuelDispenserManagement.db;";

    private static readonly string DbFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FuelDispenserController");
    private static readonly string DbPath = Path.Combine(DbFolder, "FuelDispenserManagement.db");
    private static readonly string ConnectionString = $"Data Source={DbPath}";

    public static void InitializeDatabase()
    {


        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
        connection.Open();


        var command = connection.CreateCommand();

        for (var i = 1; i <= 4; i++)
        {

            command.CommandText = @$"
                         CREATE TABLE IF NOT EXISTS DailyReport_Unit_{i} (
                             Token TEXT PRIMARY KEY,
                             OperatorName TEXT,
                             Quantity TEXT,
                             Rate TEXT,
                             TotalAmount TEXT,
                             Date_Time TEXT,
                             User TEXT
                         )";
            command.ExecuteNonQuery();

        }

    }

    public static void AddReport(DailyReport report, string unitNo)
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @$"
        INSERT INTO DailyReport_Unit_{unitNo} 
        (Token, OperatorName, Quantity, Rate, TotalAmount, Date_Time, User) 
        VALUES (@Token, @OperatorName, @Quantity, @Rate, @TotalAmount, @Date_Time, @User)";

        cmd.Parameters.AddWithValue("@Token", report.Token);
        cmd.Parameters.AddWithValue("@OperatorName", report.OperatorName);
        cmd.Parameters.AddWithValue("@Quantity", report.Quantity);
        cmd.Parameters.AddWithValue("@Rate", report.Rate);
        cmd.Parameters.AddWithValue("@TotalAmount", report.TotalAmount);
        cmd.Parameters.AddWithValue("@Date_Time", report.Date_Time.ToString("dd-MM-yyyy HH:mm:ss"));
        cmd.Parameters.AddWithValue("@User", report.User);
        cmd.ExecuteNonQuery();
    }


    public static bool InitializeUserDatabase()
    {
        try
        {
            // Ensure folder exists
            Directory.CreateDirectory(DbFolder);

            // If DB file exists, we consider initialization complete (but still ensure tables)
            bool fileExists = File.Exists(DbPath);

            using var connection = new Microsoft.Data.Sqlite. SqliteConnection(ConnectionString);
            connection.Open(); // this will create the file if it doesn't exist

            // Create tables (IF NOT EXISTS) — safe to run multiple times
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                RegistrationDate TEXT,
                UserType TEXT NOT NULL DEFAULT 'User' -- Added UserType column
                );

                CREATE TABLE IF NOT EXISTS DailyReport_Unit_1 (
                    Token TEXT,
                    OperatorName TEXT,
                    Quantity REAL,
                    Rate REAL,
                    TotalAmount REAL,
                    Date_Time TEXT,
                    User TEXT
                );";
            cmd.ExecuteNonQuery();
            string date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            // Insert default admin only if DB was just created (optional)
            if (!fileExists)
            {
                using var insert = connection.CreateCommand();
                insert.CommandText = $"INSERT INTO users (Username, Password, RegistrationDate, UserType) VALUES (@u, @p, @r, @t);";
                insert.Parameters.AddWithValue("@u", "admin");
                insert.Parameters.AddWithValue("@p", "admin"); // hash in real app!
                insert.Parameters.AddWithValue("@r", date);
                insert.Parameters.AddWithValue("@t", "Admin");
                insert.ExecuteNonQuery();
            }

            Debug.WriteLine($"Database initialized at {DbPath}");
            return true;
        }
        catch (Exception ex)
        {
            // log to Debug and optionally to a file
            Debug.WriteLine("Database init error: " + ex);
            try
            {
                // Append simple log so you can inspect later
                Directory.CreateDirectory(Path.GetDirectoryName(DbPath) ?? ".");
                File.AppendAllText(Path.Combine(DbFolder, "db_init_error.txt"),
                    $"{DateTime.Now}: {ex}\n");
            }
            catch { /* swallowing secondary logging errors */ }

            return false;
        }
    }




   
  

}


