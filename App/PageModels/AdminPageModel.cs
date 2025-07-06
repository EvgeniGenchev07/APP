using App.Services;
using BusinessLayer;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace App.PageModels;

public class AdminPageModel : INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private DateTime _currentDate;
    private bool _isBusy;
    private bool _isDaySelected;
    private string _selectedDayTitle;
    private CalendarDay _selectedDay;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<CalendarDay> CalendarDays { get; } = new();
    public ObservableCollection<BusinessTripViewModel> SelectedDayTrips { get; } = new();

    public string CurrentMonthYear => _currentDate.ToString("MMMM yyyy");
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public bool IsDaySelected
    {
        get => _isDaySelected;
        set
        {
            _isDaySelected = value;
            OnPropertyChanged();
        }
    }

    public string SelectedDayTitle
    {
        get => _selectedDayTitle;
        set
        {
            _selectedDayTitle = value;
            OnPropertyChanged();
        }
    }

    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand NavigateToUsersCommand { get; }
    public ICommand NavigateToAbsencesCommand { get; }
    public ICommand NavigateToTripsCommand { get; }
    public ICommand LogoutCommand { get; }

    private List<BusinessTrip> _allBusinessTrips = new();

    public AdminPageModel(HttpService httpService)
    {
        _httpService = httpService;
        _currentDate = DateTime.Now;

        PreviousMonthCommand = new Command(async () => await NavigateMonth(-1));
        NextMonthCommand = new Command(async () => await NavigateMonth(1));
        NavigateToUsersCommand = new Command(async () => await NavigateToUsersAsync());
        NavigateToAbsencesCommand = new Command(async () => await NavigateToAbsencesAsync());
        NavigateToTripsCommand = new Command(async () => await NavigateToTripsAsync());
        LogoutCommand = new Command(async () => await LogoutAsync());

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            _allBusinessTrips = await _httpService.GetAllBusinessTripsAsync();
            GenerateCalendar();
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

    private async Task NavigateMonth(int direction)
    {
        _currentDate = _currentDate.AddMonths(direction);
        OnPropertyChanged(nameof(CurrentMonthYear));
        GenerateCalendar();
    }

    private void GenerateCalendar()
    {
        CalendarDays.Clear();

        var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

        // Add empty days for the beginning of the month
        for (int i = 0; i < firstDayOfWeek; i++)
        {
            CalendarDays.Add(new CalendarDay { IsEmpty = true });
        }

        // Add days of the month
        for (int day = 1; day <= lastDayOfMonth.Day; day++)
        {
            var date = new DateTime(_currentDate.Year, _currentDate.Month, day);
            var dayTrips = _allBusinessTrips.Where(t => 
                date >= t.StartDate.Date && date <= t.EndDate.Date).ToList();

            var calendarDay = new CalendarDay
            {
                Date = date,
                DayNumber = day.ToString(),
                IsCurrentMonth = true,
                IsToday = date.Date == DateTime.Today,
                HasBusinessTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Approved), // Approved
                HasPendingTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Pending), // Pending
                HasCompletedTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Completed), // Completed
                BusinessTrips = dayTrips
            };

            CalendarDays.Add(calendarDay);
        }

        // Fill remaining cells to complete the grid
        var remainingCells = 42 - CalendarDays.Count; // 6 rows * 7 columns
        for (int i = 0; i < remainingCells; i++)
        {
            CalendarDays.Add(new CalendarDay { IsEmpty = true });
        }
    }

    public void SelectDay(CalendarDay day)
    {
        if (day == null || day.IsEmpty || !day.IsCurrentMonth)
            return;

        _selectedDay = day;
        IsDaySelected = true;
        SelectedDayTitle = day.Date.ToString("dddd, MMMM dd, yyyy");

        SelectedDayTrips.Clear();
        foreach (var trip in day.BusinessTrips)
        {
            SelectedDayTrips.Add(new BusinessTripViewModel(trip));
        }
    }

    private async Task NavigateToUsersAsync()
    {
        await Shell.Current.GoToAsync("AdminUsersPage");
    }

    private async Task NavigateToAbsencesAsync()
    {
        await Shell.Current.GoToAsync("AdminAllAbsencesPage");
    }

    private async Task NavigateToTripsAsync()
    {
        await Shell.Current.GoToAsync("AdminAllBusinessTripsPage");
    }

    private async Task LogoutAsync()
    {
        // Clear user session
        Preferences.Default.Remove("UserId");
        Preferences.Default.Remove("UserName");
        Preferences.Default.Remove("UserRole");
        
        await Shell.Current.GoToAsync("//RegisterPage");
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class CalendarDay
{
    public DateTime Date { get; set; }
    public string DayNumber { get; set; }
    public bool IsEmpty { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public bool HasBusinessTrips { get; set; }
    public bool HasPendingTrips { get; set; }
    public bool HasCompletedTrips { get; set; }
    public List<BusinessTrip> BusinessTrips { get; set; } = new();

    public Color BackgroundColor => IsToday ? Colors.LightBlue : Colors.Transparent;
    public Color TextColor => IsCurrentMonth ? Colors.Black : Colors.Gray;
}

public class BusinessTripViewModel
{
    private readonly BusinessTrip _trip;

    public BusinessTripViewModel(BusinessTrip trip)
    {
        _trip = trip;
    }

    public string UserFullName => _trip.UserFullName;
    public string ProjectName => _trip.ProjectName;
    public string CarTripDestination => _trip.CarTripDestination;
    public string Task => _trip.Task ?? "No task specified";
    public string DateRange => $"{_trip.StartDate:dd/MM/yyyy} - {_trip.EndDate:dd/MM/yyyy}";
    public string Destination => _trip.CarTripDestination;
    
    public string StatusText => _trip.Status switch
    {
        BusinessTripStatus.Pending => "Pending",
        BusinessTripStatus.Approved => "Approved",
        BusinessTripStatus.Rejected => "Rejected",
        BusinessTripStatus.Cancelled => "Cancelled",
        BusinessTripStatus.Completed => "Completed",
        _ => "Unknown"
    };

    public Color StatusColor => _trip.Status switch
    {
        BusinessTripStatus.Pending => Colors.Orange,
        BusinessTripStatus.Approved => Colors.Green,
        BusinessTripStatus.Rejected => Colors.Red,
        BusinessTripStatus.Cancelled => Colors.Gray,
        BusinessTripStatus.Completed => Colors.Blue,
        _ => Colors.Gray
    };
} 