using CommunityToolkit.Mvvm.ComponentModel;

namespace FuelDispenserController.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private string _username = string.Empty; // Initialize the field to avoid nullability issues
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }
    public MainViewModel()
    {
        Username = App.CurrentUserName;
    }
}
