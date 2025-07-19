using System;
using System.Collections.ObjectModel;
using FuelDispenserController.Models;
using FuelDispenserController.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Data.Sqlite;

namespace FuelDispenserController.Views;

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class DataGridPage : Page
{
    private const string DatabaseFilePath = "Data Source=C:\\Database\\DailyReport_Unit_1.db;";

    public ObservableCollection<DailyReport> ReportsUnit1 { get; set; } = new();
    public ObservableCollection<DailyReport> ReportsUnit2 { get; set; } = new();

    public DataGridPage()
    {
        this.InitializeComponent();
        LoadReport();
    }

    

    private void LoadReport()
    {
        ReportsUnit1.Clear();

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseFilePath);
        connection.Open();

        using var cmd = connection.CreateCommand();

        cmd.CommandText = "SELECT * FROM DailyReport_Unit_1";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            ReportsUnit1.Add(new DailyReport
            {
                Token = reader.GetString(0),
                OperatorName = reader.GetString(1),
                Quantity = decimal.Parse(reader.GetString(2)),
                Rate = decimal.Parse(reader.GetString(3)),
                TotalAmount = decimal.Parse(reader.GetString(4)),
                Date_Time = DateTime.Parse(reader.GetString(5))
            });
        }


    }

}
