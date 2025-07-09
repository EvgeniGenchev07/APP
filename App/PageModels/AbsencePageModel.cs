using App.Services;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace App.PageModels;

public partial class AbsencePageModel : ObservableObject
{
    private readonly HttpService _httpService;

    [ObservableProperty]
    private string _employeeName;

    [ObservableProperty]
    private int _availableDays;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasValidationErrors;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _isFormValid = true;

    [ObservableProperty]
    private AbsenceTypeOption _selectedAbsenceType;

    public ObservableCollection<AbsenceTypeOption> AbsenceTypes { get; } = new()
    {
        new AbsenceTypeOption { Value = BusinessLayer.AbsenceType.Vacation, DisplayName = "Ваканция" },
        new AbsenceTypeOption { Value = BusinessLayer.AbsenceType.SickLeave, DisplayName = "Болнични" },
        new AbsenceTypeOption { Value = BusinessLayer.AbsenceType.PersonalLeave, DisplayName = "Отпуск" },
        new AbsenceTypeOption { Value = BusinessLayer.AbsenceType.Other, DisplayName = "Други" }
    };

    public DateTime MinimumDate => DateTime.Today;

    public int DurationDays => (EndDate - StartDate).Days + 1;

    public AbsencePageModel(HttpService httpService)
    {
        _httpService = httpService;
        LoadUserData();
        ValidateForm();
    }

    private void LoadUserData()
    {
        if (App.User != null)
        {
            EmployeeName = App.User.Name;
            AvailableDays = App.User.AbsenceDays;
        }
    }

    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    [RelayCommand]
    private async Task SubmitRequest()
    {
        if (!ValidateForm())
        {
            return;
        }

        try
        {
            IsBusy = true;

            var absence = new Absence
            {
                Type = SelectedAbsenceType.Value,
                DaysCount = DurationDays,
                StartDate = StartDate,
                Status = BusinessLayer.AbsenceStatus.Pending,
                Created = DateTime.Now,
                UserId = App.User?.Id ?? string.Empty
            };

            var success = await _httpService.CreateAbsenceAsync(absence);
            
            if (success)
            {
<<<<<<< HEAD
                await Shell.Current.DisplayAlert("Success", "Absence request submitted successfully", "OK");
=======
                await Shell.Current.DisplayAlert("Успех", "Молбата за отсъствие е изпратена успешно", "OK");
           
>>>>>>> 161cdccfa121aca2aa5c30c2621aaf502d2e368f
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Грешка", "Неуспешно изпращане на молба за отсъствие", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Грешка", $"Възникна грешка: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateForm()
    {
        var errors = new List<string>();

        if (SelectedAbsenceType == null)
        {
            errors.Add("Моля избери причина за отсъствието");
        }

        if (StartDate < DateTime.Today)
        {
            errors.Add("Началната дата не може да бъде в миналото");
        }

        if (EndDate < StartDate)
        {
            errors.Add("Крайната дата не може да бъде преди началната");
        }

        if (DurationDays > AvailableDays)
        {
            errors.Add($"Имаш още само {AvailableDays} свободни дни");
        }

        HasValidationErrors = errors.Any();
        ValidationMessage = string.Join("\n", errors);
        IsFormValid = !HasValidationErrors;

        return IsFormValid;
    }

    partial void OnStartDateChanged(DateTime value)
    {
        if (EndDate < value)
        {
            EndDate = value;
        }
        ValidateForm();
        OnPropertyChanged(nameof(DurationDays));
    }

    partial void OnEndDateChanged(DateTime value)
    {
        ValidateForm();
        OnPropertyChanged(nameof(DurationDays));
    }

    partial void OnSelectedAbsenceTypeChanged(AbsenceTypeOption value)
    {
        ValidateForm();
    }
}

public class AbsenceTypeOption
{
    public BusinessLayer.AbsenceType Value { get; set; }
    public string DisplayName { get; set; } = string.Empty;
} 