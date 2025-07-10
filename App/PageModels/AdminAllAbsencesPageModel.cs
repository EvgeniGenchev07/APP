using App.Pages;
using App.Services;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace App.PageModels;

public partial class AdminAllAbsencesPageModel :ObservableObject, INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private bool _isRefreshing;
    [ObservableProperty]
    private string _search;
    [ObservableProperty]
    private ObservableCollection<int> availableYears;
    [ObservableProperty]
    private bool _hasNoResults;
    [ObservableProperty]
    private int selectedYear;

    [ObservableProperty]
    private ObservableCollection<string> availableMonths;

    [ObservableProperty]
    private string selectedMonth;
    public event PropertyChangedEventHandler PropertyChanged;
    [ObservableProperty]
    public ObservableCollection<AbsenceViewModel> absences = new();
    private List<AbsenceViewModel> _originalAbsences = new();

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

    public int TotalAbsences => Absences.Count;
    public int PendingAbsences => Absences.Count(a => a.Status == AbsenceStatus.Pending);
    public int ApprovedAbsences => Absences.Count(a => a.Status == AbsenceStatus.Approved);
    public int RejectedAbsences => Absences.Count(a => a.Status == AbsenceStatus.Rejected);

    public ICommand ApproveAbsenceCommand { get; }
    public ICommand RejectAbsenceCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }

    public AdminAllAbsencesPageModel(HttpService httpService)
    {
        _httpService = httpService;
        AvailableYears = new ObservableCollection<int>(Enumerable.Range(DateTime.Now.Year - 5, 10));
        SelectedYear = DateTime.Now.Year;

        AvailableMonths = new ObservableCollection<string>(
            CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12));
        AvailableMonths.Add("Всички месеци");
        SelectedMonth = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[DateTime.Now.Month - 1];

        ApproveAbsenceCommand = new Command<AbsenceViewModel>(async (absence) => await ApproveAbsenceAsync(absence));
        RejectAbsenceCommand = new Command<AbsenceViewModel>(async (absence) => await RejectAbsenceAsync(absence));
        CancelCommand = new Command(async () => await CancelAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());

    }
    [RelayCommand]
    private void FilterAbsence()
    {
        try
        {
            IsBusy = true;
            ObservableCollection<AbsenceViewModel> filtered = new ObservableCollection<AbsenceViewModel>(_originalAbsences);

            // Filter by year
            if (SelectedYear > 0)
            {
                filtered = new ObservableCollection<AbsenceViewModel>(
                    filtered.Where(t => t.StartDate.Year == SelectedYear || t.EndDate.Year == SelectedYear));
            }

            // Filter by month
            if (!string.IsNullOrEmpty(SelectedMonth) && selectedMonth != "Всички месеци")
            {
                var monthIndex = Array.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, SelectedMonth) + 1;
                filtered = new ObservableCollection<AbsenceViewModel>(
                    filtered.Where(t => t.StartDate.Month == monthIndex || t.EndDate.Month == monthIndex));
            }

            if (!string.IsNullOrEmpty(Search))
            {
                filtered = new ObservableCollection<AbsenceViewModel>(filtered.Where(t => t.UserName.Contains(Search, StringComparison.OrdinalIgnoreCase)));
            }

            Absences = filtered;
            
        }
        finally
        {
            IsBusy = false;
            HasNoResults = !Absences.Any();
        }
    }

    internal async Task LoadAbsencesAsync()
    {
        try
        {
            IsBusy = true;
            
            var absences = await _httpService.GetAllAbsencesAsync();
            _originalAbsences = absences.Select(a => new AbsenceViewModel(a)).ToList();
            Absences.Clear();
            foreach (var absence in absences)
            {
                Absences.Add(new AbsenceViewModel(absence));
            }

            OnPropertyChanged(nameof(TotalAbsences));
            OnPropertyChanged(nameof(PendingAbsences));
            OnPropertyChanged(nameof(ApprovedAbsences));
            OnPropertyChanged(nameof(RejectedAbsences));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Ãðåøêà", $"Íåóñïåøíî çàðåæäàíå íà îòñúñòâèÿ: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
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
    private async Task ApproveAbsenceAsync(AbsenceViewModel absence)
    {
        if (absence == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Ïîòâúðäåòå îäîáðåíèå",
            $"Èñêàòå ëè äà îäîáðèòå ìîëáàòà çà êîìàíäèðîâêà?",
            "Îäîáðè",
            "Îòêàç");

        if (confirm)
        {
            try
            {
                IsBusy = true;
                
                // Call API to approve absence
                var success = await _httpService.ApproveAbsenceAsync(absence.Id);
                if (success)
                {
                    absence.Status = AbsenceStatus.Approved;
                    int index = Absences.IndexOf(absence);
                    if (index >= 0)
                    {
                        Absences[index] = absence;
                    }
                    OnPropertyChanged(nameof(PendingAbsences));
                    OnPropertyChanged(nameof(ApprovedAbsences));
                    await Application.Current.MainPage.DisplayAlert("Óñïåõ", "Îòñúñòâèåòî áå îäîáðåíî óñïåøíî", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ãðåøêà", "Íåóñïåøíî îäîáðåíèå íà îòñúñòâèå", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ãðåøêà", $"Íåóñïåøíî îäîáðåíèå íà îòñúñòâèå: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task RejectAbsenceAsync(AbsenceViewModel absence)
    {
        if (absence == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Ïîòâúðäåòå îòõâúðëÿíå",
            $"Èñêàòå ëè äà îòõâúðëèòå ìîëáàòà çà êîìàíäèðîâêà?",
            "Îòõâúðëè",
            "Îòêàç");

        if (confirm)
        {
            try
            {
                IsBusy = true;
                
                var success = await _httpService.RejectAbsenceAsync(absence.Id);
                if (success)
                {
                    absence.Status = AbsenceStatus.Rejected;
                    int index = Absences.IndexOf(absence);
                    if (index >= 0)
                    {
                        Absences[index] = absence;
                    }
                    OnPropertyChanged(nameof(PendingAbsences));
                    OnPropertyChanged(nameof(RejectedAbsences));
                    await Application.Current.MainPage.DisplayAlert("Óñïåõ", "Îòñúñòâèåòî áå îòõâúðëåíî óñïåøíî", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ãðåøêà", "Íåóñïåøíî îòõâúðëÿíå íà îòñúñòâèå", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ãðåøêà", $"Íåóñïåøíî îòõâúðëÿíå íà îòñúñòâèå: {ex.Message}", "OK");
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
    [RelayCommand]
    private async Task Export()
    {
        
    }

    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadAbsencesAsync();
        IsRefreshing = false;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 