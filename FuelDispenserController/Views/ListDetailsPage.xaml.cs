using CommunityToolkit.WinUI.UI.Controls;

using FuelDispenserController.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace FuelDispenserController.Views;

public sealed partial class ListDetailsPage : Page
{
    private static readonly string DbFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FuelDispenserController");
    private static readonly string DbPath = Path.Combine(DbFolder, "FuelDispenserManagement.db");
    private static readonly string ConnectionString = $"Data Source={DbPath}";

    public ListDetailsPage()
    {

        InitializeComponent();
       
    }



    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {

    }

    public static bool UsernameExists(string username, string dbPath)
    {
        using var connection = new Microsoft.Data.Sqlite. SqliteConnection(dbPath);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @username";
        command.Parameters.AddWithValue("@username", username);

        var count = (long)(command.ExecuteScalar() ?? 0);
        return count > 0;
    }

    
    private void RegisterButton_Click(object sender, RoutedEventArgs e )
    {
        string username = UsernameTextBox.Text;
        string password = PasswordBox.Password;
        string confirmPassword = ConfirmPasswordBox.Password;
        string userType = RoleComboBox.SelectedIndex == 0 ? "Admin" : RoleComboBox.SelectedIndex == 1 ? "Manager" : "None";

        if (UsernameTextBox.Text == string.Empty)
        {
            FormMessage.Text = "Please Fill out the form Completely";
            return;
        }

        if (password != confirmPassword )
        {
            FormMessage.Text = "Passwords do not match!";
            FormMessage.Foreground = new SolidColorBrush(Colors.Red);
            return;
        }

        //try
        //{
            var newUser = new User
            {
                Username = username,
                Password = password,
                RegistrationDate =  DateTime.Now.ToString(),

                UserType = userType

            };

            if (UsernameExists(username, ConnectionString))
            {

            }
            else { UserService.AddUser(ConnectionString, newUser); LoadUsers(); }
                

            FormMessage.Text = "User Registered Successfully!";
            FormMessage.Foreground = new SolidColorBrush(Colors.LightBlue);
            UsernameTextBox.Text = string.Empty;
        //}
        //catch (Exception ex)
        //{
        //    FormMessage.Text = "Error: " + ex.Message;
        //    FormMessage.Foreground = new SolidColorBrush(Colors.Red);
        //}
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        LoadUsers();
    }

    private void LoadUsers()
    {
        var dbPath = ConnectionString; // or relative path
        var users = UserService.GetAllUsers(dbPath);
        UserListView.ItemsSource = users;
    }

    private User _selectedUser;

    private void Selection_Changed_Click(object sender, SelectionChangedEventArgs e)
    {
        if (UserListView.SelectedItem is User selected)
        {
            _selectedUser = selected;
            UsernameTextBox.Text = selected.Username;
            PasswordBox.Password = selected.Password;
            ConfirmPasswordBox.Password = selected.Password;
        }
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedUser == null)
        {
            FormMessage.Text = "Passwords do not match!"; return;
        }

        if (PasswordBox.Password != ConfirmPasswordBox.Password)
        {
            FormMessage.Text = "Passwords do not match!";
            return;
        }

        _selectedUser.Username = UsernameTextBox.Text;
        _selectedUser.Password = PasswordBox.Password;

        UserService.UpdateUser(_selectedUser, ConnectionString);

        // Refresh list or show success message

        LoadUsers();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        
       

        if (sender is Button button && button.Tag is User userToDelete)
        {

            //ContentDialog dialog = new ContentDialog();

            //// XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            //dialog.XamlRoot = this.XamlRoot;
            //dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            //dialog.Title = "Save your work?";
            //dialog.PrimaryButtonText = "Delete";
            //dialog.CloseButtonText = "Cancel";
            //dialog.DefaultButton = ContentDialogButton.Primary;

            //var result = await dialog.ShowAsync();


            // Show confirmation dialog
            var dialog2 = new ContentDialog
            {
                XamlRoot = this.XamlRoot, // required in WinUI 3
                Title = "Confirm Delete",
                Content = $"Are you sure you want to delete user '{userToDelete.Username}'?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,

            };

            var result2 = await dialog2.ShowAsync();

            if (result2 == ContentDialogResult.Primary)
            {
                // Remove from database
              
                    UserService.DeleteUser(userToDelete.Id, ConnectionString);
                    LoadUsers();
                    FormMessage.Text = "User Deleted Successfully!";
                    FormMessage.Foreground = new SolidColorBrush(Colors.LightBlue);
                
                // Remove from ObservableCollection

            }
            else
            {
                FormMessage.Text = "No user selected!";
                FormMessage.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
    }


}
