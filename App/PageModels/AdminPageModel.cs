using App.Pages;
using App.Services;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace App.PageModels;

public partial class AdminPageModel : ObservableObject, INotifyPropertyChanged
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
    private ObservableCollection<HolidayDay> _holidayDays;
    private List<DateTime> _customHolidays = new();

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<CalendarDay> CalendarDays { get; } = new();
    public ObservableCollection<BusinessTripViewModel> SelectedDayTrips { get; } = new();
    public ObservableCollection<AbsenceViewModel> SelectedDayAbsences { get; } = new();

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
    private List<Absence> _allAbsences = new();

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

    }

   
    [RelayCommand]
    private async Task ItemTapped(BusinessTripViewModel businessTrip)
    {
        if (businessTrip != null)
        {
            BusinessTripDetailsPage.SelectedBusinessTrip = businessTrip;
            await Shell.Current.GoToAsync("//businesstripdetails");
        }
    }
    [RelayCommand]
    public async void SelectAbsence(AbsenceViewModel absence)
    {
        if (absence != null)
        {
            AbsenceDetailsPage.SelectedAbsence = absence;
            await Shell.Current.GoToAsync("AbsenceDetailsPage");
        }
    }
   

    internal async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            _allBusinessTrips = await _httpService.GetAllBusinessTripsAsync();
            _allAbsences = await _httpService.GetAllAbsencesAsync();
            _holidayDays = new ObservableCollection<HolidayDay>( await _httpService.GetAllHolidayDaysAsync());
            _customHolidays = _holidayDays.Select(h => h.Date.Date).ToList();
            GenerateCalendar();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно зареждане на данните: {ex.Message}", "OK");
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

            var dayAbsences = _allAbsences.Where(a =>
                date >= a.StartDate.Date && date <= a.StartDate.AddDays(a.DaysCount - 1).Date).ToList();
            var holidayDay = _holidayDays.FirstOrDefault(h => h.Date.Date == date.Date);

            var calendarDay = new CalendarDay
            {
                Date = date,
                DayNumber = day.ToString(),
                IsCurrentMonth = true,
                IsToday = date.Date == DateTime.Today,
                IsHoliday = holidayDay is not null,
                HolidayName = holidayDay?.Name ?? string.Empty,
                IsOfficialHoliday = holidayDay is not null && !holidayDay.IsCustom,
                IsCustomHoliday = holidayDay is not null && holidayDay.IsCustom,
                HasBusinessTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Approved),
                HasPendingTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Pending),
                HasCompletedTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Completed),
                BusinessTrips = dayTrips,
                HasApprovedAbsences = dayAbsences.Any(a => a.Status == AbsenceStatus.Approved),
                HasPendingAbsences = dayAbsences.Any(a => a.Status == AbsenceStatus.Pending),
                HasRejectedAbsences = dayAbsences.Any(a => a.Status == AbsenceStatus.Rejected),
                Absences = dayAbsences,
            };

            CalendarDays.Add(calendarDay);
        }

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
            await Application.Current.MainPage.DisplayAlert("Грешка", "Моля въведете име на почивния ден!", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            _customHolidays.Add(SelectedHolidayDate.Date);
            await _httpService.CreateHolidayDayAsync(new HolidayDay()
            {
                Name = HolidayName,
                Date = SelectedHolidayDate.Date,
                IsCustom = true
            });
            _holidayDays = new ObservableCollection<HolidayDay>(await _httpService.GetAllHolidayDaysAsync());
            GenerateCalendar();
            IsHolidayDialogVisible = false;
            HolidayName = string.Empty;
            await Application.Current.MainPage.DisplayAlert("Успех", "Почивния ден бе успешно добавен!", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно добавяне на почивен ден: {ex.Message}", "OK");
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
            await Application.Current.MainPage.DisplayAlert("Success", "Почивният ден бе успешно изтрит!", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно изтриване: {ex.Message}", "OK");
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
        if(SelectedDay is not null)
        {
            SelectedDay.IsSelected = false; 
            int index = CalendarDays.IndexOf(SelectedDay);
            if (index >= 0)
            {
                CalendarDays[index] = SelectedDay;
            }
        }
        {
            day.IsSelected = true;
            int index = CalendarDays.IndexOf(day);
            if (index >= 0)
            {
                CalendarDays[index] = day;
            }
        }
        SelectedDay = day;
        IsDaySelected = true;
        SelectedDayTitle = day.Date.ToString("dddd, MMMM dd, yyyy");

        // Update business trips for selected day
        SelectedDayTrips.Clear();
        foreach (var trip in day.BusinessTrips)
        {
            SelectedDayTrips.Add(new BusinessTripViewModel(trip));
        }

        // Update absences for selected day
        SelectedDayAbsences.Clear();
        foreach (var absence in day.Absences)
        {
            SelectedDayAbsences.Add(new AbsenceViewModel(absence));
        }
    }

    private async Task NavigateToUsersAsync()
    {
        await Shell.Current.GoToAsync("//AdminUsersPage");
    }

    private async Task NavigateToAbsencesAsync()
    {
        await Shell.Current.GoToAsync("//AdminAllAbsencesPage");
    }

    private async Task NavigateToTripsAsync()
    {
        await Shell.Current.GoToAsync("//AdminAllBusinessTripsPage");
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
    public bool IsSelected { get; set; }
    public bool IsHoliday { get; set; }
    public bool IsOfficialHoliday { get; set; }
    public bool IsCustomHoliday { get; set; }
    public bool HasBusinessTrips { get; set; }
    public bool HasPendingTrips { get; set; }
    public bool HasCompletedTrips { get; set; }
    public bool HasApprovedAbsences { get; set; }
    public bool HasPendingAbsences { get; set; }
    public bool HasRejectedAbsences { get; set; }
    public List<BusinessTrip> BusinessTrips { get; set; } = new();
    public List<Absence> Absences { get; set; } = new();

    public bool HasAnyTrips => HasBusinessTrips || HasPendingTrips || HasCompletedTrips;
    public bool HasNoTrips => !HasAnyTrips;
    public bool HasAnyAbsences => HasApprovedAbsences || HasPendingAbsences || HasRejectedAbsences;
    public bool HasNoAbsences => !HasAnyAbsences;

    public string HolidayName { get; set; }

    public Color HolidayColor => IsOfficialHoliday ? Colors.LightPink :
                               IsCustomHoliday ? Colors.LightBlue :
                               Colors.Transparent;

    public Color BackgroundColor
    {
        get
        {
            if (IsSelected) return Color.FromArgb("#4169E1"); 
            if (IsHoliday) return IsOfficialHoliday ? Colors.LightPink : Colors.LightBlue;
            return Colors.Transparent;
        }
    }

    public Color TextColor
    {
        get
        {
            if (IsSelected) return Colors.Black; 
            if (!IsCurrentMonth) return Colors.Gray;
            if (IsHoliday) return Colors.DarkRed;
            return Colors.Black;
        }
    }
}