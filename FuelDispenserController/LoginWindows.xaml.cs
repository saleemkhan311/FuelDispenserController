using FuelDispenserController.Contracts.Services;
using FuelDispenserController.Core.Helpers;
using FuelDispenserController.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using WinRT.Interop;

namespace FuelDispenserController;

public sealed partial class LoginWindows : Window
{
    private static readonly string DbFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FuelDispenserController");
    private static readonly string DbPath = Path.Combine(DbFolder, "FuelDispenserManagement.db");
    private static readonly string ConnectionString = $"Data Source={DbPath}";

    public LoginWindows()
    {
        InitializeComponent();

       

        Title = "Login";

        // Set LoginPage as content
        //Content = App.GetService<LoginPage>();

        // Get AppWindow
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        // Disable maximize & resize
        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMaximizable = false;
            presenter.IsResizable = false;
            presenter.PreferredMaximumWidth = 550;
        }
        // Center the window on the screen
        CenterWindow();



    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        string username = UsernameTextBox.Text.Trim();
        string password = PasswordTextBox.Password.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            StatusTextBlock.Text = "Please enter both username and password.";
            return;
        }

        var authResult = AuthenticateUser(username, password);

        if (authResult == "success")
        {


            // Store username in a global/static property so MainViewModel can use it
            App.CurrentUserName = username;

            // Activate the main window using ActivationService
            await App.GetService<IActivationService>().ActivateAsync(null);

            // Close the login window
            this.Close();
        }
        else
        {
            StatusTextBlock.Text = authResult;
        }
    }

    private string AuthenticateUser(string username, string password)
    {
        try
        {
            

            using (var connection = new Microsoft.Data.Sqlite. SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Step 1: Check if username exists
                string userCheckQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                using (var userCheckCmd = new Microsoft.Data.Sqlite.SqliteCommand(userCheckQuery, connection))
                {
                    userCheckCmd.Parameters.AddWithValue("@username", username);
                    long userExists = (long)userCheckCmd.ExecuteScalar();

                    if (userExists == 0)
                        return "Username not found.";
                }

                // Step 2: Check if password matches for this username
                string passCheckQuery = "SELECT COUNT(*) FROM users WHERE username = @username AND password = @password";
                using (var passCheckCmd = new Microsoft.Data.Sqlite.SqliteCommand(passCheckQuery, connection))
                {
                    passCheckCmd.Parameters.AddWithValue("@username", username);
                    passCheckCmd.Parameters.AddWithValue("@password", password); // Hash in real apps

                    long validLogin = (long)passCheckCmd.ExecuteScalar();

                    if (validLogin == 0)
                        return "Incorrect password.";
                }

                return "success";
            }
        }
        catch (Exception ex)
        {
            return "Error: " + ex.Message;
        }
    }






    private void CenterWindow()
    {
        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId winId = Win32Interop.GetWindowIdFromWindow(hwnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(winId);

        // Get the display area (work area excludes taskbar)
        var displayArea = DisplayArea.GetFromWindowId(winId, DisplayAreaFallback.Primary);

        int screenWidth = displayArea.WorkArea.Width;
        int screenHeight = displayArea.WorkArea.Height;

        int windowWidth = appWindow.Size.Width;
        int windowHeight = appWindow.Size.Height;

        // Calculate centered position
        int x = (screenWidth - windowWidth) / 2;
        int y = (screenHeight - windowHeight) / 2;

        appWindow.Move(new PointInt32(x, y));
    }
}
