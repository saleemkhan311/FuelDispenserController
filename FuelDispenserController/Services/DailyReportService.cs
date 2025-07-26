using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuelDispenserController.Models;
using SQLite;

namespace FuelDispenserController.Services;

class DailyReportService
{
    static readonly string DatabaseFile = "Data Source=C:\\Database\\FuelDispenserManagement.db;";
    public static void DeleteItem(string UnitNo,string item)
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseFile);
        connection.Open();
        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = $"DELETE From DailyReport_Unit_{UnitNo} Where Token = @Token";
        deleteCmd.Parameters.AddWithValue("@Token",item);
        deleteCmd.ExecuteNonQuery();
        connection.Close();

    }
}

