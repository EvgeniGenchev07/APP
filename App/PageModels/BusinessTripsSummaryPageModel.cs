using App.Services;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using ClosedXML.Excel;
using Microsoft.Maui.Storage;
using System.IO;
namespace App.PageModels;

public partial class BusinessTripsSummaryPageModel : ObservableObject
{
    private readonly HttpService _httpService;
    public BusinessTripsSummaryPageModel(HttpService httpService)
    {
        _httpService = httpService;

        // Initialize filters
        AvailableYears = new ObservableCollection<int>(Enumerable.Range(DateTime.Now.Year - 5, 10));
        SelectedYear = DateTime.Now.Year;

        AvailableMonths = new ObservableCollection<string>(
            CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12));
        AvailableMonths.Add("Всички месеци");
        SelectedMonth = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[DateTime.Now.Month - 1];

        LoadProjects();
    }

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool hasNoResults;
    [ObservableProperty]
    private decimal summary;

    // Filter properties
    [ObservableProperty]
    private ObservableCollection<int> availableYears;

    [ObservableProperty]
    private int selectedYear;

    [ObservableProperty]
    private ObservableCollection<string> availableMonths;

    [ObservableProperty]
    private string selectedMonth;

    [ObservableProperty]
    private ObservableCollection<string> availableProjects = new();

    [ObservableProperty]
    private string selectedProject;

    [ObservableProperty]
    private StatusFilter selectedStatus;
    private List<BusinessTripViewModel> _originalTrips = new();
    // Trips collection
    [ObservableProperty]
    private ObservableCollection<BusinessTripViewModel> trips = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    partial void OnSelectedStatusChanged(StatusFilter value)
    {
        if (value != null)
            FilterTrips();
    }

    [RelayCommand]
    private async Task LoadTrips()
    {
        try
        {
            IsLoading = true;
            HasNoResults = false;

            var trips = await _httpService.GetAllBusinessTripsAsync();
            var viewModels = trips.Select(t => new BusinessTripViewModel(t)).ToList();
            _originalTrips = viewModels;
            Trips = new ObservableCollection<BusinessTripViewModel>(viewModels);
            HasNoResults = !Trips.Any();
            Summary = Trips.Sum(t => t.Wage * t.Days + t.AccommodationMoney * t.Days);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Грешка", $"Възникна грешка при зареждане: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }
    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.GoToAsync("//AdminAllBusinessTripsPage");
    }
   


[RelayCommand]
    private async Task Export() 
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Командировки");
        worksheet.Cell(1, 1).Value = "Номер";
        worksheet.Cell(1, 2).Value = "Проект";
        worksheet.Cell(1, 3).Value = "Място";
        worksheet.Cell(1, 4).Value = "Дата";
        worksheet.Cell(1, 5).Value = "Дни";
        worksheet.Cell(1, 6).Value = "Общо дневни";
        worksheet.Cell(1, 7).Value = "Пътни";
        worksheet.Cell(1, 8).Value = "Хотел";
        worksheet.Cell(1, 9).Value = "Други";
        worksheet.Cell(1, 10).Value = "Цел";
        worksheet.Cell(1, 11).Value = "Общо";
        int currentIndex = 2;
      foreach (var trip in Trips)
        {
            worksheet.Cell(currentIndex, 1).Value = trip.Id;
            worksheet.Cell(currentIndex, 2).Value = trip.ProjectName;
            worksheet.Cell(currentIndex, 3).Value = trip.CarTripDestination;
            worksheet.Cell(currentIndex, 4).Value = trip.DateRange;
            worksheet.Cell(currentIndex, 5).Value = trip.Days;
            worksheet.Cell(currentIndex, 6).Value = trip.Wage * trip.Days;
            worksheet.Cell(currentIndex, 7).Value = trip.Wage;
            worksheet.Cell(currentIndex, 8).Value = trip.AccommodationMoney;
            worksheet.Cell(currentIndex, 9).Value = "0";
            worksheet.Cell(currentIndex, 10).Value = trip.Task;
            worksheet.Cell(currentIndex, 11).FormulaA1 = $"F{currentIndex} + G{currentIndex} + H{currentIndex} + I{currentIndex}";
            currentIndex++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var result = await FileSaver.Default.SaveAsync("BusinessTrips.xlsx", stream);
    }
    [RelayCommand]
    private async Task LoadProjects()
    {
        try
        {
            var projects = await _httpService.GetAllBusinessTripsAsync();
            AvailableProjects = new ObservableCollection<string>(projects.Select(bt=>bt.ProjectName).Distinct().ToList());
            AvailableProjects.Insert(0, "Всички проекти");
            SelectedProject = "Всички проекти";
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Грешка", $"Възникна грешка при зареждане на проекти: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void FilterTrips()
    {
        try
        {
            IsLoading = true;

            ObservableCollection<BusinessTripViewModel> filtered = new ObservableCollection<BusinessTripViewModel>(_originalTrips);

            // Filter by year
            if (SelectedYear > 0)
            {
                filtered = new ObservableCollection<BusinessTripViewModel>(
                    filtered.Where(t => t.StartDate.Year == SelectedYear || t.EndDate.Year == SelectedYear));
            }

            // Filter by month
            if (!string.IsNullOrEmpty(SelectedMonth))
            {
                var monthIndex = Array.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, SelectedMonth) + 1;
                filtered = new ObservableCollection<BusinessTripViewModel>(
                    filtered.Where(t => t.StartDate.Month == monthIndex || t.EndDate.Month == monthIndex));
            }

            // Filter by project
            if (!string.IsNullOrEmpty(SelectedProject) && SelectedProject != "Всички проекти")
            {
                filtered = new ObservableCollection<BusinessTripViewModel>(
                    filtered.Where(t => t.ProjectName == SelectedProject));
            }

            Trips = filtered;
            Summary = Trips.Sum(t => t.Wage * t.Days + t.AccommodationMoney * t.Days);
            HasNoResults = !Trips.Any();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadTrips();
        await LoadProjects();
    }
}

public class StatusFilter
{
    public string Name { get; }
    public Color Color { get; }

    public StatusFilter(string name, Color color)
    {
        Name = name;
        Color = color;
    }
}

