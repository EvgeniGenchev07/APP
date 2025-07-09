using App.Pages;
using App.Services;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace App.PageModels;

public partial class AdminAllBusinessTripsPageModel : ObservableObject, INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private bool _isRefreshing;

    public event PropertyChangedEventHandler PropertyChanged;
    [ObservableProperty]
    public ObservableCollection<BusinessTripViewModel> businessTrips = new();

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

    public int TotalTrips => BusinessTrips.Count;
    public int PendingTrips => BusinessTrips.Count(t => t.Status ==  BusinessTripStatus.Pending);
    public int ApprovedTrips => BusinessTrips.Count(t => t.Status == BusinessTripStatus.Approved);
    public int RejectedTrips => BusinessTrips.Count(t => t.Status == BusinessTripStatus.Rejected);

    public ICommand ApproveTripCommand { get; }
    public ICommand RejectTripCommand { get; }
    public ICommand SummaryCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }

    public AdminAllBusinessTripsPageModel(HttpService httpService)
    {
        _httpService = httpService;
        
        ApproveTripCommand = new Command<BusinessTripViewModel>(async (trip) => await ApproveTripAsync(trip));
        RejectTripCommand = new Command<BusinessTripViewModel>(async (trip) => await RejectTripAsync(trip));
        SummaryCommand = new Command(async () => await SummaryAsync());
        CancelCommand = new Command(async () => await CancelAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());

        _ = LoadBusinessTripsAsync();
    }

    private async Task LoadBusinessTripsAsync()
    {
        try
        {
            IsBusy = true;

            var trips = await _httpService.GetAllBusinessTripsAsync();

            BusinessTrips.Clear();
            foreach (var trip in trips)
            {
                BusinessTrips.Add(new BusinessTripViewModel(trip));
            }

            OnPropertyChanged(nameof(TotalTrips));
            OnPropertyChanged(nameof(PendingTrips));
            OnPropertyChanged(nameof(ApprovedTrips));
            OnPropertyChanged(nameof(RejectedTrips));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно зареждане на командировки: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
    [RelayCommand]
    private async Task ItemTapped(BusinessTripViewModel businessTrip)
    {
        if (businessTrip != null)
        {
            //BusinessTripDetailsPage.SelectedBusinessTrip = businessTrip;
            await Shell.Current.GoToAsync("//businesstripdetails");
        }
    }
    private async Task ApproveTripAsync(BusinessTripViewModel trip)
    {
        if (trip == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Потвърдете одобрение", 
            $"Искате ли да одобрите молбата за командировка?", 
            "Одобри", 
            "Отказ");

        if (confirm)
        {
            try
            {
                IsBusy = true;
                
                // Call API to approve business trip
                var success = await _httpService.ApproveBusinessTripAsync(trip.Id);
                if (success)
                {
                    trip.Status = BusinessTripStatus.Approved;
                    int index = BusinessTrips.IndexOf(trip);
                    if (index >= 0)
                    {
                        BusinessTrips[index] = trip;
                    }
                    OnPropertyChanged(nameof(PendingTrips));
                    OnPropertyChanged(nameof(ApprovedTrips));
                    await Application.Current.MainPage.DisplayAlert("Успех", "Командировката бе одобрена успешно", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Грешка", "Неуспешно одобряване на командировка", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно одобряване на командировка: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task RejectTripAsync(BusinessTripViewModel trip)
    {
        if (trip == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Потвърдете отхвърляне", 
            $"Искате ли да отхвърлите молбата за командировка?", 
            "Отхвърли", 
            "Отказ");

        if (confirm)
        {
            try
            {
                IsBusy = true;
                
                var success = await _httpService.RejectBusinessTripAsync(trip.Id);
                if (success)
                {
                    trip.Status = BusinessTripStatus.Rejected;
                    int index = BusinessTrips.IndexOf(trip);
                    if (index >= 0)
                    {
                        BusinessTrips[index] = trip;
                    }
                    OnPropertyChanged(nameof(PendingTrips));
                    OnPropertyChanged(nameof(RejectedTrips));
                    await Application.Current.MainPage.DisplayAlert("Успех", "Командировката бе откзана успешно", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Грешка", "Неуспешно отхвърляне на командировка", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно отхвърляне на командировка: {ex.Message}", "OK");
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

    private async Task SummaryAsync()
    {
        await Shell.Current.GoToAsync("//BusinessTripsSummaryPage");
    }

    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadBusinessTripsAsync();
        IsRefreshing = false;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 