using App.Services;
using App.ViewModels;
using BusinessLayer;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using App.Pages;
using System.Linq;

namespace App.PageModels;

public class AdminUsersPageModel : INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private bool _isRefreshing;
    private string _searchText = string.Empty;
    private List<UserViewModel> _allUsers = new(); 

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<UserViewModel> Users { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                Task.Run(() => SearchUsersAsync());
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (_isRefreshing != value)
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand AddUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand RefreshCommand { get; }

    public AdminUsersPageModel(HttpService httpService)
    {
        _httpService = httpService;

        AddUserCommand = new Command(async () => await AddUserAsync());
        EditUserCommand = new Command<UserViewModel>(async (user) => await EditUserAsync(user));
        DeleteUserCommand = new Command<UserViewModel>(async (user) => await DeleteUserAsync(user));
        SearchCommand = new Command(async () => await SearchUsersAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());

        _ = LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            IsBusy = true;
            Users.Clear();
            _allUsers.Clear();

            var users = await _httpService.GetAllUsersAsync();
            _allUsers = users.Select(u => new UserViewModel(u)).ToList();

            Device.BeginInvokeOnMainThread(() =>
            {
                foreach (var user in _allUsers)
                {
                    Users.Add(user);
                }
            });
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

    private async Task SearchUsersAsync()
    {
        try
        {
            IsBusy = true;

            var filteredUsers = await Task.Run(() =>
                string.IsNullOrWhiteSpace(SearchText)
                    ? _allUsers
                    : _allUsers.Where(u =>
                        u.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList());

            Device.BeginInvokeOnMainThread(() =>
            {
                Users.Clear();
                foreach (var user in filteredUsers)
                {
                    Users.Add(user);
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to search users: {ex.Message}", "OK");
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
                var success = await _httpService.DeleteUserAsync(user.Id);

                if (success)
                {
                    _allUsers.Remove(user);
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