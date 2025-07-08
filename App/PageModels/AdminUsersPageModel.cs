using App.Services;
using App.ViewModels;
using BusinessLayer;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using App.Pages;

namespace App.PageModels;

public class AdminUsersPageModel : INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private bool _isRefreshing;
    private string _searchText = string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<UserViewModel> Users { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }

    public AdminUsersPageModel(HttpService httpService)
    {
        _httpService = httpService;
        
        AddUserCommand = new Command(async () => await AddUserAsync());
        EditUserCommand = new Command<UserViewModel>(async (user) => await EditUserAsync(user));
        DeleteUserCommand = new Command<UserViewModel>(async (user) => await DeleteUserAsync(user));
        SearchCommand = new Command(async () => await SearchUsersAsync());
        CancelCommand = new Command(async () => await CancelAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());

        _ = LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            IsBusy = true;

            var users = await _httpService.GetAllUsersAsync();

            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(new UserViewModel(user));
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddUserAsync()
    {
        await Shell.Current.GoToAsync("AddUserPage");
    }

    private async Task EditUserAsync(UserViewModel user)
    {
        if (user != null)
        {
            // Pass the user to the edit page
            EditUserPage.SelectedUser = user;
            await Shell.Current.GoToAsync("EditUserPage");
        }
    }

    private async Task DeleteUserAsync(UserViewModel user)
    {
        if (user == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete user '{user.Name}'?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            try
            {
                IsBusy = true;

                // Call API to delete user
                var success = await _httpService.DeleteUserAsync(user.Id);
                if (success)
                {
                    Users.Remove(user);
                    await Application.Current.MainPage.DisplayAlert("Success", "User deleted successfully", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete user", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete user: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("//AdminPage");
    }

    private async Task SearchUsersAsync()
    {
        // Implement search functionality
        await LoadUsersAsync();
    }

    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadUsersAsync();
        IsRefreshing = false;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 