using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FuelDispenserController.Models;
using FuelDispenserController.Services;
using FuelDispenserController.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SQLite;
using Microsoft.Data.Sqlite;
using System.Runtime.Serialization.Formatters;


namespace FuelDispenserController.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    readonly string connectionString = "Data Source=C:\\Database\\DailyReport_Unit_1.db;";
    readonly string connectionString2 = "Data Source=C:\\Database\\DailyReport_Unit_2.db;";
    readonly string connectionString3 = "Data Source=C:\\Database\\DailyReport_Unit_3.db;";
    readonly string connectionString4 = "Data Source=C:\\Database\\DailyReport_Unit_4.db;";
    public ObservableCollection<DailyReport> Reports { get; set; } = new();



    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        SetBaseToken(connectionString, TokenTextBox1);

        DatabaseHelper.InitializeDatabase();

    }

    void AddReport()
    {
        var report = new DailyReport
        {
            Token = TokenTextBox1.Text,
            OperatorName = OperatorNameTextBox1.Text,
            Quantity = decimal.Parse(QuantityTextBox1.Text),
            Rate = decimal.Parse(RateTextBox1.Text),
            TotalAmount = decimal.Parse(AmountTextBox1.Text),
            Date_Time = DateTime.Now

        };

        DatabaseHelper.AddReport(report);


        TokenTextBox1.Text = string.Empty;
        QuantityTextBox1.Text = string.Empty;
        RateTextBox1.Text = string.Empty;
        AmountTextBox1.Text = string.Empty;


    }



    private async void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        AddReport();
        SetBaseToken(connectionString, TokenTextBox1);
    }

    private async void CaptureButton_Click(object sender, RoutedEventArgs e)
    {


    }

   


   

    public static void SetBaseToken(string connectionString, TextBox tokenTextBox)
    {
        string lastToken = null;

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Token FROM DailyReport_Unit_1 ORDER BY ROWID DESC LIMIT 1";
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            lastToken = reader.GetString(0);
        }

        string numericPart = new string(lastToken?.TakeWhile(char.IsDigit).ToArray() ?? Array.Empty<char>());

        if (string.IsNullOrEmpty(numericPart) || !int.TryParse(numericPart, out int numericToken) || numericToken < 1000)
        {
            tokenTextBox.Text = "1000";
        }
        else
        {
            tokenTextBox.Text = (numericToken + 1).ToString();
        }
    }





    private void AddButton_Click4(object sender, RoutedEventArgs e) => throw new NotImplementedException();
    private void PrintButton_Click4(object sender, RoutedEventArgs e) => throw new NotImplementedException();
    private void PrintButton_Click2(object sender, RoutedEventArgs e) => throw new NotImplementedException();
    private void PrintButton_Click3(object sender, RoutedEventArgs e) => throw new NotImplementedException();
    private void AddButton_Click3(object sender, RoutedEventArgs e) => throw new NotImplementedException();
    private void AddButton_Click2(object sender, RoutedEventArgs e) => throw new NotImplementedException();


    /* public static void SetNextToken(string connectionString, TextBox tokenTextBox)
    {
        string lastToken = null;

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Token FROM DailyReport_Unit_1 ORDER BY ROWID DESC LIMIT 1";
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            lastToken = reader.GetString(0);
        }

        if (string.IsNullOrEmpty(lastToken))
        {
            tokenTextBox.Text = "1000";
            return;
        }

        // Check if last token ends with a letter (suffix like A, B, C...)
        if (char.IsLetter(lastToken[^1]))
        {
            // Extract numeric part and letter suffix
            string numericPart = new string(lastToken.TakeWhile(char.IsDigit).ToArray());
            char lastChar = lastToken[^1];

            if (char.ToUpper(lastChar) < 'Z')
            {
                // Increment letter (A -> B, B -> C, ...)
                char nextSuffix = (char)(char.ToUpper(lastChar) + 1);
                tokenTextBox.Text = numericPart + nextSuffix;
            }
            else
            {
                // Reset back to base token if Z is reached (optional logic)
                tokenTextBox.Text = numericPart;
            }
        }
        else
        {
            // No suffix → start suffix with 'A'
            tokenTextBox.Text = lastToken + "A";
        }
    }*/



}
