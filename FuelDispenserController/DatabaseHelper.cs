using System;
using System.Collections.ObjectModel;
using System.IO;
using FuelDispenserController.Models;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using Windows.Storage;
public static class DatabaseHelper
{
    private const string DatabaseFileName = "Data Source=C:\\Database\\DailyReport_Unit_1.db;";

    public static void InitializeDatabase()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseFileName);


        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @$"
                CREATE TABLE IF NOT EXISTS DailyReport_Unit_1 (
                    Token TEXT PRIMARY KEY,
                    OperatorName TEXT,
                    Quantity TEXT,
                    Rate TEXT,
                    TotalAmount TEXT,
                    Date_Time TEXT
                )";
        command.ExecuteNonQuery();
    }

    public static void AddReport(DailyReport report)
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=C:\\Database\\DailyReport_Unit_1.db;");
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO DailyReport_Unit_1 
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



}
