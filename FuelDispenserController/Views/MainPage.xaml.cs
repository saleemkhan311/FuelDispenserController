using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.Serialization.Formatters;
using System.Threading;       // For CancellationTokenSource
using System.Threading.Tasks; // For Task and async/await
using CommunityToolkit.WinUI;
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

    static readonly string connectionString = "Data Source=C:\\Database\\FuelDispenserManagement.db;";
    public ObservableCollection<DailyReport> Reports { get; set; } = new();

    

    public MainPage()
    {
        DatabaseHelper.InitializeDatabase();
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        SetBaseToken(TokenTextBox1, "1");
        SetBaseToken(TokenTextBox2, "2");
        SetBaseToken(TokenTextBox3, "3");
        SetBaseToken(TokenTextBox4, "4");

        DatabaseHelper.InitializeDatabase();
        //InitSerialPort("COM3");
        ConnectToSerialPort();

    }

    private string token;
    private string operatorName;
    private double quantity;
    private double rate;
    private double totalAmount;


   
    private CancellationTokenSource _readCancellationTokenSource;
    public ObservableCollection<string> AvailableComPorts { get; } = new ObservableCollection<string>();
    private SerialDevice _serialPort;
    private DataWriter _dataWriter;

    // Define your known COM port and baud rate here
    private const string TARGET_COM_PORT_NAME = "COM3"; // <<< CHANGE THIS IF YOUR PORT IS DIFFERENT
    private const uint BAUD_RATE = 115200;
    private async void ConnectToSerialPort()
    {
        StatusTextBlock.Text = $"Attempting to connect to {TARGET_COM_PORT_NAME} at {BAUD_RATE} baud...";

        try
        {
            string aqs = SerialDevice.GetDeviceSelector(); // Get selector for ALL serial devices
            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(aqs);

            if (devices.Any())
            {
                // Find the device where the name or Id contains the target COM port name
                var targetDevice = devices.FirstOrDefault(d => d.Name.Contains(TARGET_COM_PORT_NAME, StringComparison.OrdinalIgnoreCase) ||
                                                               d.Id.Contains(TARGET_COM_PORT_NAME, StringComparison.OrdinalIgnoreCase));

                if (targetDevice != null)
                {
                    _serialPort = await SerialDevice.FromIdAsync(targetDevice.Id);

                    if (_serialPort != null)
                    {
                        // ... (rest of your configuration code remains the same) ...
                        _serialPort.BaudRate = BAUD_RATE;
                        _serialPort.DataBits = 8;
                        _serialPort.StopBits = SerialStopBitCount.One;
                        _serialPort.Parity = SerialParity.None;
                        _serialPort.Handshake = SerialHandshake.None;
                        _serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                        _serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);

                        _dataWriter = new DataWriter(_serialPort.OutputStream);

                        StatusTextBlock.Text = $"Successfully connected to {_serialPort.PortName} ({targetDevice.Name})."; // Show actual port name
                        UnitOnButton1.IsEnabled = true;
                        ListenForSerialData();
                    }
                    else
                    {
                        StatusTextBlock.Text = $"Could not open {TARGET_COM_PORT_NAME}. It might be in use or not accessible.";
                        UnitOnButton1.IsEnabled = false;
                    }
                }
                else
                {
                    // This block will now tell you what names were actually found
                    string foundNames = string.Join(", ", devices.Select(d => d.Name));
                    StatusTextBlock.Text = $"No device found matching '{TARGET_COM_PORT_NAME}'. Found devices: {foundNames}.";
                    UnitOnButton1.IsEnabled = false;
                }
            }
            else
            {
                StatusTextBlock.Text = "No serial devices found at all. Check connections and drivers.";
                UnitOnButton1.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Error connecting to serial port: {ex.Message}";
            UnitOnButton1.IsEnabled = false;
        }
    }

    private async void UnitControl_Click1(object sender, RoutedEventArgs e)
    {
        if (_serialPort != null && _dataWriter != null)
        {
            try
            {
                string command = "ON\n"; // Send "ON" followed by a newline character
                _dataWriter.WriteString(command);
                await _dataWriter.StoreAsync(); // Ensure data is sent

                StatusTextBlock.Text = $"Sent: '{command.Trim()}' to {TARGET_COM_PORT_NAME}";
                UnitOnButton1.IsEnabled = false;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error sending command: {ex.Message}";
            }
        }
        else
        {
            StatusTextBlock.Text = "Serial port not connected. Attempting to reconnect...";
            UnitOnButton1.IsEnabled = false; // Disable temporarily
            ConnectToSerialPort(); // Try to reconnect
        }
    }


    private async void ListenForSerialData()
    {
        // Ensure any previous read operation is cancelled before starting a new one
        _readCancellationTokenSource?.Cancel();
        _readCancellationTokenSource?.Dispose();
        _readCancellationTokenSource = new CancellationTokenSource();

        DataReader dataReader = new DataReader(_serialPort.InputStream);

        while (true) // Continuous loop to read data
        {
            try
            {
                // Check if cancellation has been requested
                _readCancellationTokenSource.Token.ThrowIfCancellationRequested();

                // Try to load more bytes into the buffer.
                // LoadAsync returns the number of bytes read.
                // We use a small buffer size (e.g., 1024) or just 1 byte to check for new data.
                uint bytesToRead = await dataReader.LoadAsync(1024).AsTask(_readCancellationTokenSource.Token);

                if (bytesToRead > 0)
                {
                    // Read the string from the buffer
                    string receivedText = dataReader.ReadString(bytesToRead);

                    // Update the UI on the UI thread using DispatcherQueue
                    // This is crucial because you cannot directly update UI from a background thread
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        // You can parse the receivedText here based on your ESP32's output
                        // For example, if ESP32 sends "OFF" when LED is turned off by touch:
                        if (receivedText.Contains("OFF", StringComparison.OrdinalIgnoreCase))
                        {
                            StatusTextBlock.Text = $"Received from ESP32: RELAY is OFF.";
                            // You could potentially disable the "ON" button here, or enable an "OFF" button
                            UnitOnButton1.IsEnabled = true;
                        }
                        else
                        {
                            StatusTextBlock.Text = $"Received: RELAY {receivedText.Trim()}";
                        }
                    });
                }
                // Add a small delay to prevent busy-waiting and consuming too much CPU
                await Task.Delay(50);
            }
            catch (OperationCanceledException)
            {
                // This exception is thrown when _readCancellationTokenSource.Cancel() is called.
                // It means the reading task was intentionally stopped.
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    StatusTextBlock.Text = "Serial port listening stopped.";
                });
                break; // Exit the loop
            }
            catch (Exception ex)
            {
                // Handle other exceptions, e.g., serial port disconnected unexpectedly
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    StatusTextBlock.Text = $"Error during serial read: {ex.Message}";
                });
                break; // Exit the loop on error
            }
        }

        // Clean up DataReader when the loop exits
        dataReader.Dispose();
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
            Date_Time = DateTime.Now

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

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"{connectionString}");
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
