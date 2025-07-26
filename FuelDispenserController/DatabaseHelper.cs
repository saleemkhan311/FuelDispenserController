using System;
using System.Collections.ObjectModel;
using System.IO;
using FuelDispenserController.Models;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using Windows.Storage;
public static class DatabaseHelper
{
    private const string DatabaseFileName = "Data Source=C:\\Database\\FuelDispenserManagement.db;";

    public static void InitializeDatabase()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseFileName);


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
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseFileName);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @$"
        INSERT INTO DailyReport_Unit_{unitNo} 
        (Token, OperatorName, Quantity, Rate, TotalAmount, Date_Time) 
        VALUES (@Token, @OperatorName, @Quantity, @Rate, @TotalAmount, @Date_Time)";

        cmd.Parameters.AddWithValue("@Token", report.Token);
        cmd.Parameters.AddWithValue("@OperatorName", report.OperatorName);
        cmd.Parameters.AddWithValue("@Quantity", report.Quantity);
        cmd.Parameters.AddWithValue("@Rate", report.Rate);
        cmd.Parameters.AddWithValue("@TotalAmount", report.TotalAmount);
        cmd.Parameters.AddWithValue("@Date_Time", report.Date_Time.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }


    public static void InitializeUserDatabase(string dbPath)
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(dbPath);
        connection.Open();

        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText =
        @"
        CREATE TABLE IF NOT EXISTS Users (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Username TEXT NOT NULL UNIQUE,
        Password TEXT NOT NULL,
        RegistrationDate TEXT,
        UserType TEXT NOT NULL DEFAULT 'User' -- Added UserType column
        );
        ";
        tableCmd.ExecuteNonQuery();
    }


}


