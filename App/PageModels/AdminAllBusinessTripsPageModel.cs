using App.Services;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
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
    public int PendingTrips => BusinessTrips.Count(t => t.StatusText == BusinessTripStatus.Pending.ToString());
    public int ApprovedTrips => BusinessTrips.Count(t => t.StatusText == BusinessTripStatus.Approved.ToString());
    public int RejectedTrips => BusinessTrips.Count(t => t.StatusText == BusinessTripStatus.Rejected.ToString());

    public ICommand ApproveTripCommand { get; }
    public ICommand RejectTripCommand { get; }
    public ICommand FilterCommand { get; }
    public ICommand RefreshCommand { get; }

    public AdminAllBusinessTripsPageModel(HttpService httpService)
    {
        _httpService = httpService;
        
        ApproveTripCommand = new Command<BusinessTripViewModel>(async (trip) => await ApproveTripAsync(trip));
        RejectTripCommand = new Command<BusinessTripViewModel>(async (trip) => await RejectTripAsync(trip));
        FilterCommand = new Command(async () => await FilterTripsAsync());
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
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load business trips: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApproveTripAsync(BusinessTripViewModel trip)
    {
        if (trip == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Confirm Approval", 
            $"Are you sure you want to approve this business trip request?", 
            "Approve", 
            "Cancel");

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
                    await Application.Current.MainPage.DisplayAlert("Success", "Business trip approved successfully", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to approve business trip", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to approve business trip: {ex.Message}", "OK");
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
            "Confirm Rejection", 
            $"Are you sure you want to reject this business trip request?", 
            "Reject", 
            "Cancel");

        if (confirm)
        {
            try
            {
                IsBusy = true;
                
                // Call API to reject business trip
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
                    await Application.Current.MainPage.DisplayAlert("Success", "Business trip rejected successfully", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to reject business trip", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to reject business trip: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task FilterTripsAsync()
    {
        // Implement filtering functionality
        await LoadBusinessTripsAsync();
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