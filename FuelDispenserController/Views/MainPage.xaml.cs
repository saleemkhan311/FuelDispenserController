using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.Serialization.Formatters;
using System.Threading;       // For CancellationTokenSource
using System.Threading.Tasks; // For Task and async/await
using CommunityToolkit.WinUI;
using FuelDispenserController.Helpers;
using FuelDispenserController.Models;
using FuelDispenserController.Services;
using FuelDispenserController.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Dispatching; // For DispatcherQueue to update UI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SQLite;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams; // For DataReader

namespace FuelDispenserController.Views;

public sealed partial class MainPage : Page
{
   

    public MainViewModel ViewModel
    {
        get;
    }

    private static readonly string DbFolder = Path.Combine(
         Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
         "FuelDispenserController");
    private static readonly string DbPath = Path.Combine(DbFolder, "FuelDispenserManagement.db");
    private static readonly string ConnectionString = $"Data Source={DbPath}";
    public ObservableCollection<DailyReport> Reports { get; set; } = new();

    

    public MainPage()
    {
        _espControllers = new List<SerialCommunicationHelper>();

        DatabaseHelper.InitializeDatabase();
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        SetBaseToken(TokenTextBox1, "1");
        SetBaseToken(TokenTextBox2, "2");
        SetBaseToken(TokenTextBox3, "3");
        SetBaseToken(TokenTextBox4, "4");

        DatabaseHelper.InitializeDatabase();

         _espControllers.Add(new SerialCommunicationHelper("COM4", 115200, UnitOnButton1, StatusTextBlock, this.DispatcherQueue));
         _espControllers.Add(new SerialCommunicationHelper("COM3", 115200, UnitOnButton2, StatusTextBlock2, this.DispatcherQueue));

        ConnectAllControllers();
        //InitSerialPort("COM3");
        //ComPortBox.Text = "COM4"; // Replace with your default COM port
        //BaudRateBox.Text = "115200"; // Replace with your default baud rate

    }

    private string token;
    private string operatorName;
    private double quantity;
    private double rate;
    private double totalAmount;

    private List<SerialCommunicationHelper> _espControllers;


    private async void ConnectAllControllers()
    {
        //GeneralAppStatus.Text = "Starting connection for all ESP32 units...";
        foreach (var controller in _espControllers) // Go through each manager in our list
        {
            await controller.ConnectAsync(); // Tell each manager to connect to its ESP32
        }
        //GeneralAppStatus.Text = "All connection attempts finished.";
    }

    // Event handler for when the "Connect & Send ON" button is clicked.
    //private SerialCommunicationHelper? _espControllers;
    //private async void UnitControl_Click1(object sender, RoutedEventArgs e)
    //{
    //    // Read the user input from the text boxes.
    //    string comPortName = ComPortBox.Text.Trim();
    //    string baudRateText = BaudRateBox.Text.Trim();

    //    // Perform basic validation on the user input.
    //    if (string.IsNullOrEmpty(comPortName) || string.IsNullOrEmpty(baudRateText))
    //    {
    //        StatusTextBlock.Text = "Error: Please enter both a COM port and a baud rate.";
    //        return;
    //    }

    //    // Try to convert the baud rate text into a number.
    //    if (!uint.TryParse(baudRateText, out uint baudRate))
    //    {
    //        StatusTextBlock.Text = "Error: Baud rate must be a valid number.";
    //        return;
    //    }

    //    // If a controller already exists, we need to clean it up before creating a new one.
    //    if (_espControllers != null)
    //    {
    //        _espControllers.Dispose();
    //        _espControllers = null;
    //    }

    //    // Create a new controller with the user's input.
    //    // We pass the new status text block and the current window's dispatcher queue.
    //    _espControllers = new SerialCommunicationHelper(comPortName, baudRate, PrintButton, StatusTextBlock, this.DispatcherQueue);

    //    // Connect to the ESP32 and wait for the connection to be established.
    //    await _espControllers.ConnectAsync();
    //    // After connecting, if the connection was successful, send the "ON" command.
    //    if (_espControllers.IsConnected)
    //    {
    //        await _espControllers.SendCommandAsync("ON\n");
    //    }
    //}

    private async void UnitControl_Click1(object sender, RoutedEventArgs e)
    {
        await _espControllers[0].SendCommandAsync("ON\n");
    }
    private async void UnitControl_Click2(object sender, RoutedEventArgs e)
    {
        await _espControllers[1].SendCommandAsync("ON\n");
    }


    void AddReport(string UnitNo)
    {
       


        var report = new DailyReport
        {
            Token = token,
            OperatorName = operatorName,
            Quantity = quantity,
            Rate = rate,
            TotalAmount = totalAmount,
            Date_Time = DateTime.Now,
            User = App.CurrentUserName

        };

        DatabaseHelper.AddReport(report, UnitNo);
    }








    private async void CaptureButton_Click(object sender, RoutedEventArgs e)
    {


    }

    private void EmptyBox(params TextBox[] textBoxes)
    {
        foreach(var box in textBoxes)
        {
            box.Text = string.Empty;
        }
    }

    

    private bool CheckValid(params TextBox[] textBoxes)
    {
        var check = false;

        //TextBox[] textBoxes = { tokenBox, helperBox, qunatityBox, rateBox, amountBox };

        foreach (TextBox textBox in textBoxes)
        {
            if (textBox.Text != string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return check;
    }






    public static void SetBaseToken(TextBox tokenTextBox, string UnitNo)
    {
        string lastToken = null;

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"{ConnectionString}");
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT Token FROM DailyReport_Unit_{UnitNo} ORDER BY ROWID DESC LIMIT 1";
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            lastToken = reader.GetString(0);
        }

        string numericPart = new string(lastToken?.TakeWhile(char.IsDigit).ToArray() ?? Array.Empty<char>());

        int.TryParse(UnitNo, out int unitNumber);
        int baseToken = unitNumber * 1000;

        if (string.IsNullOrEmpty(numericPart) || !int.TryParse(numericPart, out int numericToken) || numericToken < baseToken)
        {
            tokenTextBox.Text = baseToken.ToString();
        }
        else
        {
            tokenTextBox.Text = (numericToken + 1).ToString();
        }
    }


    private async void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        if (CheckValid(TokenTextBox1, OperatorNameTextBox1, QuantityTextBox1, RateTextBox1, AmountTextBox1))
        {
            token = TokenTextBox1.Text;
            operatorName = OperatorNameTextBox1.Text;
            quantity = double.Parse(QuantityTextBox1.Text);
            rate
                = double.Parse(RateTextBox1.Text);
            totalAmount = double.Parse(AmountTextBox1.Text);

            AddReport("1");
            SetBaseToken(TokenTextBox1, "1");
            EmptyBox(QuantityTextBox1, RateTextBox1, AmountTextBox1);

        }
        else
        {
            var dialog = new ContentDialog
            {
                Title = "Invalid Input Unit 01",
                Content = "Please fill in all fields before printing.",
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
        }
    }

    private void PrintButton_Click2(object sender, RoutedEventArgs e)
    {
        if (CheckValid(TokenTextBox2, OperatorNameTextBox2, QuantityTextBox2, RateTextBox2, AmountTextBox2))
        {
            token = TokenTextBox2.Text;
            operatorName = OperatorNameTextBox2.Text;
            quantity = double.Parse(QuantityTextBox2.Text);
            rate = double.Parse(RateTextBox2.Text);
            totalAmount = double.Parse(AmountTextBox2.Text);


            AddReport("2");
            SetBaseToken(TokenTextBox2, "2");
            EmptyBox(QuantityTextBox2, RateTextBox2, AmountTextBox2);
        }
        else
        {
            var dialog = new ContentDialog
            {
                Title = "Invalid Input Unit 02",
                Content = "Please fill in all fields before printing.",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }
    }

    private void PrintButton_Click3(object sender, RoutedEventArgs e)
    {
        if (CheckValid(TokenTextBox3, OperatorNameTextBox3, QuantityTextBox3, RateTextBox3, AmountTextBox3))
        {
            token = TokenTextBox3.Text;
            operatorName = OperatorNameTextBox3.Text;
            
            quantity = double.Parse(QuantityTextBox3.Text);

            rate = double.Parse(RateTextBox3.Text);
            totalAmount = double.Parse(AmountTextBox3.Text);


            AddReport("3");
            SetBaseToken(TokenTextBox3, "3");
            EmptyBox(QuantityTextBox3, RateTextBox3, AmountTextBox3);
        }
        else
        {
            var dialog = new ContentDialog
            {
                Title = "Invalid Input Unit 03",
                Content = "Please fill in all fields before printing.",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }
    }

    private void PrintButton_Click4(object sender, RoutedEventArgs e)
    {
        if (CheckValid(TokenTextBox4, OperatorNameTextBox4, QuantityTextBox4, RateTextBox4, AmountTextBox4))
        {

            token = TokenTextBox4.Text;
            operatorName = OperatorNameTextBox4.Text;
            quantity = double.Parse(QuantityTextBox4.Text);
            rate = double.Parse(RateTextBox4.Text);
            totalAmount = double.Parse(AmountTextBox4.Text);



            AddReport("4");
            SetBaseToken(TokenTextBox4, "4");
            EmptyBox(QuantityTextBox4, RateTextBox4, AmountTextBox4);
        }
        else
        {
            var dialog = new ContentDialog
            {
                Title = "Invalid Input Unit 04",
                Content = "Please fill in all fields before printing.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot, 
            };
            dialog.ShowAsync();
        }
    }


    

}
