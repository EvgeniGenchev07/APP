using App.Pages;
using App.Services;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace App.PageModels;

public partial class MainPageModel : ObservableObject, INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private string _userName;
    private int _absenceDays;
    private int _pendingTripsCount;
    private int _pendingAbsencesCount;
    private int _approvedAbsencesCount;
    private int _approvedTripsCount;
    private bool _noAbsences;
    private bool _noBusinessTrips;

    public event PropertyChangedEventHandler PropertyChanged;

    [ObservableProperty]
    public ObservableCollection<AbsenceViewModel> recentAbsences = new();

    [ObservableProperty]
    public ObservableCollection<BusinessTripViewModel> recentBusinessTrips = new();

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

    public int ApprovedTripsCount
    {
        get => _approvedTripsCount;
        set
        {
            _approvedTripsCount = value;
            OnPropertyChanged();
        }
    }
    public int PendingAbsencesCount
    {
        get => _pendingAbsencesCount;
        set
        {
            _pendingAbsencesCount = value;
            OnPropertyChanged();
        }
    }
    public int ApprovedAbsencesCount
    {
        get => _approvedAbsencesCount;
        set
        {
            _approvedAbsencesCount = value;
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
   
        _ = LoadDataAsync();
    }

    internal async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;

            if (App.User != null)
            {
                UserName = App.User.Name;
                AbsenceDays = App.User.AbsenceDays;

                var businessTrips = await _httpService.GetUserBusinessTripsAsync(App.User.Id);
                var recentTrips = businessTrips.OrderByDescending(t => t.Created).Take(5).ToList();
                
                PendingTripsCount = businessTrips.Count(t => t.Status == BusinessTripStatus.Pending);
                ApprovedTripsCount = businessTrips.Count(t => t.Status == BusinessTripStatus.Approved);
                
                RecentBusinessTrips.Clear();
                foreach (var trip in recentTrips)
                {
                    RecentBusinessTrips.Add(new BusinessTripViewModel(trip));
                }

                NoBusinessTrips = !recentTrips.Any();

                var absences = await _httpService.GetUserAbsencesAsync(App.User.Id);
                var recentAbsences = absences.OrderByDescending(a => a.Created).Take(5).ToList();

                RecentAbsences.Clear();
                foreach (var absence in recentAbsences)
                {
                    RecentAbsences.Add(new AbsenceViewModel(absence));
                }
                ApprovedAbsencesCount = RecentAbsences.Count(t => t.Status == AbsenceStatus.Approved);
                PendingAbsencesCount = RecentAbsences.Count(t => t.Status == AbsenceStatus.Pending);
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
    [RelayCommand]
    private async Task AbsenceTapped(AbsenceViewModel absence)
    {
        if (absence != null)
        {
            AbsenceDetailsPage.SelectedAbsence =  absence;
            await Shell.Current.GoToAsync("AbsenceDetailsPage");
        }
    }
    [RelayCommand]
    private async Task BusinessTripTapped(BusinessTripViewModel businessTrip)
    {
        if (businessTrip != null)
        {
            BusinessTripDetailsPage.SelectedBusinessTrip = App.User?.BusinessTrips?.FirstOrDefault(t => t.Id == businessTrip.Id);
            await Shell.Current.GoToAsync("//businesstripdetails");
        }
    }
    private async Task LogoutAsync()
    {
        try
        {
            App.User = null;
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

 