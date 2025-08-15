using System;
using System.Collections.ObjectModel;
using System.Globalization;
using FuelDispenserController.Models;
using FuelDispenserController.Services;
using FuelDispenserController.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FuelDispenserController.Views;

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class DataGridPage : Page
{
    private static readonly string DbFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FuelDispenserController");
    private static readonly string DbPath = Path.Combine(DbFolder, "FuelDispenserManagement.db");
    private static readonly string ConnectionString = $"Data Source={DbPath}";

    //public ObservableCollection<DailyReport> ReportsUnit1 { get; set; } = new();
    //public ObservableCollection<DailyReport> ReportsUnit2 { get; set; } = new();
    //public ObservableCollection<DailyReport> ReportsUnit3 { get; set; } = new();
    //public ObservableCollection<DailyReport> ReportsUnit4 { get; set; } = new();
    public ObservableCollection<DailyReport> Reports { get; set; } = new();



    public DataGridPage()
    {
        this.InitializeComponent();
        LoadReport("1");
        UnitOptions = new List<string> { "Unit 1", "Unit 2", "Unit 3", "Unit 4" };
    }



    private string CurrentUserType()
    {
        string currentUserName = App.CurrentUserName;
        string currentUserType = " ";

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
        connection.Open();

        string query = "SELECT UserType FROM Users WHERE Username = @Username";

        using var cmd = new Microsoft.Data.Sqlite.SqliteCommand(query,connection);
        cmd.Parameters.AddWithValue("@Username", currentUserName);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            currentUserType = reader.GetString(0);
        }
        else
        {
            // Handle case where user is not found
            currentUserType = "Unknown";
        }


        return currentUserType;
    }

    private void LoadReport(string unitNo)
    {
        
        string currentUserType = CurrentUserType();
        string currentUserName = App.CurrentUserName;


        Reports.Clear();

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();

        if(currentUserType == "Admin")
        {
            cmd.CommandText = $"SELECT * FROM DailyReport_Unit_{unitNo} ";
        }else if(currentUserType == "Manager") 
        { 
            cmd.CommandText = $"SELECT * FROM DailyReport_Unit_{unitNo} WHERE User = @Username "; 
            cmd.Parameters.AddWithValue("@Username", currentUserName);

        }


        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Reports.Add(new DailyReport
            {
                Token = reader.GetString(0),
                OperatorName = reader.GetString(1),
                Quantity = double.Parse(reader.GetString(2)),
                Rate = double.Parse(reader.GetString(3)),
                TotalAmount = double.Parse(reader.GetString(4)),
                //Date_Time = DateTime.Parse(reader.GetString(5)),

                Date_Time = DateTime.ParseExact(reader.GetString(5), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                //Date_Time = DateTime.TryParse(reader.GetString(5), out DateTime dateTime) ? dateTime : DateTime.MinValue,

                User = reader.IsDBNull(6) ? "none" : reader.GetString(6)
            });
        }


    }

    public List<string> UnitOptions
    {
        get; set;
    }

    int selectedUnit;
    private void UnitSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedUnit = UnitSelector.SelectedIndex+1;
        string unitNo = selectedUnit.ToString();
        LoadReport(unitNo);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {

        if (DatabaseTable.SelectedItem is DailyReport selectedReport)
        {
            var unitNo = selectedUnit.ToString();

            var dialog = new ContentDialog
            {
                Title = "Delete Report",
                Content = $"Are you sure you want to delete the report for {selectedReport.Token} From Unit 0{selectedUnit}?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot

            };

            var item = selectedReport.Token;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                
                DailyReportService.DeleteItem(unitNo, item);
                LoadReport(unitNo);
            }

        }
        else
        {

        }
            
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (DatabaseTable.SelectedItem is DailyReport selectedReport)
        {
           

            var dialog = new ContentDialog()
            {

                Title = "Update Report",
                Content = $"Are you sure you want to Update the report for {selectedReport.Token} From Unit 0{selectedUnit}?",
                PrimaryButtonText = "Update",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot

            };

            var result = await dialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {
                DailyReportService.UpdateItem(selectedUnit, selectedReport);
                LoadReport(selectedUnit.ToString());
            }

        }
    }
}
