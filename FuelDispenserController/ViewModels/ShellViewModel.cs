using CommunityToolkit.Mvvm.ComponentModel;

using FuelDispenserController.Contracts.Services;
using FuelDispenserController.Views;

using Microsoft.UI.Xaml.Navigation;

namespace FuelDispenserController.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    private string _username = string.Empty; // Initialize the field to avoid nullability issues
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        Username = App.CurrentUserName; // Initialize Username with the current user's name
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;
        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
