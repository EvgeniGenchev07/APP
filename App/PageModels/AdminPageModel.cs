using App.Services;
using BusinessLayer;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using App.ViewModels;

namespace App.PageModels;

public class AdminPageModel : INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private DateTime _currentDate;
    private bool _isBusy;
    private bool _isDaySelected;
    private string _selectedDayTitle;
    private CalendarDay _selectedDay;
    private DateTime _selectedHolidayDate = DateTime.Today;
    private string _holidayName;
    private bool _isHolidayDialogVisible;
    private List<DateTime> _officialHolidays = new();
    private List<DateTime> _customHolidays = new();

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

    public CalendarDay SelectedDay
    {
        get => _selectedDay;
        set
        {
            _selectedDay = value;
            OnPropertyChanged();
        }
    }

    public DateTime SelectedHolidayDate
    {
        get => _selectedHolidayDate;
        set
        {
            _selectedHolidayDate = value;
            OnPropertyChanged();
        }
    }

    public string HolidayName
    {
        get => _holidayName;
        set
        {
            _holidayName = value;
            OnPropertyChanged();
        }
    }

    public bool IsHolidayDialogVisible
    {
        get => _isHolidayDialogVisible;
        set
        {
            _isHolidayDialogVisible = value;
            OnPropertyChanged();
        }
    }

    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand NavigateToUsersCommand { get; }
    public ICommand NavigateToAbsencesCommand { get; }
    public ICommand NavigateToTripsCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand AddHolidayCommand { get; }
    public ICommand DeleteCustomHolidayCommand { get; }
    public ICommand ShowAddHolidayDialogCommand { get; }
    public ICommand HideAddHolidayDialogCommand { get; }

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
        AddHolidayCommand = new Command(async () => await AddHolidayAsync());
        DeleteCustomHolidayCommand = new Command(async () => await DeleteCustomHolidayAsync());
        ShowAddHolidayDialogCommand = new Command(() => IsHolidayDialogVisible = true);
        HideAddHolidayDialogCommand = new Command(() => IsHolidayDialogVisible = false);

        InitializeOfficialHolidays(_currentDate.Year);
        _ = LoadDataAsync();
    }

    private void InitializeOfficialHolidays(int year)
    {
        _officialHolidays.Clear();

        // Fixed date holidays
        var fixedHolidays = new List<DateTime>
        {
            new DateTime(year, 1, 1),   // New Year
            new DateTime(year, 3, 3),   // Liberation Day
            new DateTime(year, 5, 1),   // Labor Day
            new DateTime(year, 5, 6),   // St. George's Day
            new DateTime(year, 5, 24),  // Bulgarian Education and Culture Day
            new DateTime(year, 9, 6),   // Unification Day
            new DateTime(year, 9, 22),  // Independence Day
            new DateTime(year, 12, 24), // Christmas Eve
            new DateTime(year, 12, 25), // Christmas Day
            new DateTime(year, 12, 26)  // Second Day of Christmas
        };

        // Calculate Easter and related holidays
        var easter = CalculateOrthodoxEaster(year);
        var easterHolidays = new List<DateTime>
        {
            easter.AddDays(-2), // Good Friday
            easter.AddDays(-1), // Holy Saturday
            easter,             // Easter Sunday
            easter.AddDays(1)   // Easter Monday
        };

        // Add all holidays to the official list
        _officialHolidays.AddRange(fixedHolidays);
        _officialHolidays.AddRange(easterHolidays);

        // Adjust for weekends - move to Monday if holiday falls on Saturday or Sunday
        for (int i = 0; i < _officialHolidays.Count; i++)
        {
            var holiday = _officialHolidays[i];
            if (holiday.DayOfWeek == DayOfWeek.Saturday || holiday.DayOfWeek == DayOfWeek.Sunday)
            {
                // Find next Monday that's not already a holiday
                DateTime monday = holiday;
                while (monday.DayOfWeek != DayOfWeek.Monday)
                {
                    monday = monday.AddDays(1);
                }

                // Only add if not already in the list
                if (!_officialHolidays.Contains(monday))
                {
                    _officialHolidays.Add(monday);
                }
            }
        }
    }

    private DateTime CalculateOrthodoxEaster(int year)
    {
        // Gauss algorithm for Orthodox Easter date calculation
        int a = year % 4;
        int b = year % 7;
        int c = year % 19;
        int d = (19 * c + 15) % 30;
        int e = (2 * a + 4 * b - d + 34) % 7;
        int month = (int)Math.Floor((d + e + 114) / 31M);
        int day = ((d + e + 114) % 31) + 1;

        DateTime easter = new DateTime(year, month, day);
        return easter.AddDays(13); // Convert to Gregorian calendar
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

        // Reinitialize holidays if year changed
        if (direction != 0 && _currentDate.Year != _officialHolidays.FirstOrDefault().Year)
        {
            InitializeOfficialHolidays(_currentDate.Year);
        }

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

            var isOfficialHoliday = _officialHolidays.Contains(date.Date);
            var isCustomHoliday = _customHolidays.Contains(date.Date);
            var isHoliday = isOfficialHoliday || isCustomHoliday;

            var calendarDay = new CalendarDay
            {
                Date = date,
                DayNumber = day.ToString(),
                IsCurrentMonth = true,
                IsToday = date.Date == DateTime.Today,
                IsHoliday = isHoliday,
                IsOfficialHoliday = isOfficialHoliday,
                IsCustomHoliday = isCustomHoliday,
                HasBusinessTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Approved),
                HasPendingTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Pending),
                HasCompletedTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Completed),
                BusinessTrips = dayTrips
            };

            CalendarDays.Add(calendarDay);
        }

        // Fill remaining cells to complete the grid
        var remainingCells = 42 - CalendarDays.Count;
        for (int i = 0; i < remainingCells; i++)
        {
            CalendarDays.Add(new CalendarDay { IsEmpty = true });
        }
    }

    private async Task AddHolidayAsync()
    {
        if (string.IsNullOrWhiteSpace(HolidayName))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Please enter a holiday name", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            _customHolidays.Add(SelectedHolidayDate.Date);
            GenerateCalendar();
            IsHolidayDialogVisible = false;
            HolidayName = string.Empty;
            await Application.Current.MainPage.DisplayAlert("Success", "Holiday added successfully", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to add holiday: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteCustomHolidayAsync()
    {
        if (SelectedDay == null || !SelectedDay.IsCustomHoliday)
            return;

        try
        {
            IsBusy = true;
            _customHolidays.Remove(SelectedDay.Date.Date);
            GenerateCalendar();
            IsDaySelected = false;
            await Application.Current.MainPage.DisplayAlert("Success", "Custom holiday deleted successfully", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete holiday: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void SelectDay(CalendarDay day)
    {
        if (day == null || day.IsEmpty || !day.IsCurrentMonth)
            return;

        SelectedDay = day;
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
        App.User = null;
        await Shell.Current.GoToAsync("//register");
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
    public bool IsHoliday { get; set; }
    public bool IsOfficialHoliday { get; set; }
    public bool IsCustomHoliday { get; set; }
    public bool HasBusinessTrips { get; set; }
    public bool HasPendingTrips { get; set; }
    public bool HasCompletedTrips { get; set; }
    public List<BusinessTrip> BusinessTrips { get; set; } = new();

    public bool HasAnyTrips => HasBusinessTrips || HasPendingTrips || HasCompletedTrips;
    public bool HasNoTrips => !HasAnyTrips;

    public string HolidayTypeText => IsOfficialHoliday ? "Official Holiday" :
                                   IsCustomHoliday ? "Custom Holiday" :
                                   string.Empty;

    public Color HolidayColor => IsOfficialHoliday ? Colors.LightPink :
                               IsCustomHoliday ? Colors.LightBlue :
                               Colors.Transparent;

    public Color BackgroundColor => IsToday ? Colors.LightBlue :
                                 IsHoliday ? (IsOfficialHoliday ? Colors.LightPink : Colors.LightBlue) :
                                 Colors.Transparent;

    public Color TextColor => IsCurrentMonth ?
                            (IsHoliday ? Colors.Red : Colors.Black) :
                            Colors.Gray;
}