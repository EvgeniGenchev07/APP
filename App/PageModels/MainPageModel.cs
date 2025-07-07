using App.Services;
using App.ViewModels;
using BusinessLayer;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace App.PageModels;

public class MainPageModel : INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private string _userName;
    private int _absenceDays;
    private int _pendingTripsCount;
    private int _completedTripsCount;
    private bool _noAbsences;
    private bool _noBusinessTrips;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<AbsenceViewModel> RecentAbsences { get; } = new();
    public ObservableCollection<BusinessTripViewModel> RecentBusinessTrips { get; } = new();

    public string UserName
    {
        get => _userName;
        set
        {
            _userName = value;
            OnPropertyChanged();
        }
    }

    public int AbsenceDays
    {
        get => _absenceDays;
        set
        {
            _absenceDays = value;
            OnPropertyChanged();
        }
    }

    public int PendingTripsCount
    {
        get => _pendingTripsCount;
        set
        {
            _pendingTripsCount = value;
            OnPropertyChanged();
        }
    }

    public int CompletedTripsCount
    {
        get => _completedTripsCount;
        set
        {
            _completedTripsCount = value;
            OnPropertyChanged();
        }
    }

    public bool NoAbsences
    {
        get => _noAbsences;
        set
        {
            _noAbsences = value;
            OnPropertyChanged();
        }
    }

    public bool NoBusinessTrips
    {
        get => _noBusinessTrips;
        set
        {
            _noBusinessTrips = value;
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

    public ICommand RequestAbsenceCommand { get; }
    public ICommand RequestBusinessTripCommand { get; }
    public ICommand ViewAllAbsencesCommand { get; }
    public ICommand ViewAllBusinessTripsCommand { get; }
    public ICommand LogoutCommand { get; }

    public MainPageModel(HttpService httpService)
    {
        _httpService = httpService;
        
        RequestAbsenceCommand = new Command(async () => await RequestAbsenceAsync());
        RequestBusinessTripCommand = new Command(async () => await RequestBusinessTripAsync());
        ViewAllAbsencesCommand = new Command(async () => await ViewAllAbsencesAsync());
        ViewAllBusinessTripsCommand = new Command(async () => await ViewAllBusinessTripsAsync());
        LogoutCommand = new Command(async () => await LogoutAsync());
        MessagingCenter.Subscribe<RequestPageModel>(this, "BusinessTripCreated", async (sender) =>
        {
            await LoadDataAsync();
        });
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;

            if (App.User != null)
            {
                UserName = App.User.Name;
                AbsenceDays = App.User.AbsenceDays;

                // Load user's business trips
                var businessTrips = await _httpService.GetUserBusinessTripsAsync(App.User.Id);
                var recentTrips = businessTrips.OrderByDescending(t => t.Created).Take(5).ToList();
                
                PendingTripsCount = businessTrips.Count(t => t.Status == BusinessTripStatus.Pending);
                CompletedTripsCount = businessTrips.Count(t => t.Status == BusinessTripStatus.Completed);

                RecentBusinessTrips.Clear();
                foreach (var trip in recentTrips)
                {
                    RecentBusinessTrips.Add(new BusinessTripViewModel(trip));
                }

                NoBusinessTrips = !recentTrips.Any();

                // Load user's absences
                var absences = await _httpService.GetUserAbsencesAsync(App.User.Id);
                var recentAbsences = absences.OrderByDescending(a => a.Created).Take(5).ToList();

                RecentAbsences.Clear();
                foreach (var absence in recentAbsences)
                {
                    RecentAbsences.Add(new AbsenceViewModel(absence));
                }

                NoAbsences = !recentAbsences.Any();
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RequestAbsenceAsync()
    {
        await Shell.Current.GoToAsync("//AbsencePage");
    }

    private async Task RequestBusinessTripAsync()
    {
        await Shell.Current.GoToAsync("//request");
    }

    private async Task ViewAllAbsencesAsync()
    {
        await Shell.Current.GoToAsync("//AllAbsencesPage");
    }

    private async Task ViewAllBusinessTripsAsync()
    {
        await Shell.Current.GoToAsync("//businesstrips");
    }

    private async Task LogoutAsync()
    {
        try
        {
            // Clear user data
            App.User = null;
            
            // Navigate to register page
            await Shell.Current.GoToAsync("//register");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Logout failed: {ex.Message}", "OK");
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

 