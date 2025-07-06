using App.Services;
using App.ViewModels;
using BusinessLayer;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using App.Pages;

namespace App.PageModels;

public class AbsenceDetailsPageModel : INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private AbsenceViewModel _absence;

    public event PropertyChangedEventHandler PropertyChanged;

    public AbsenceViewModel Absence
    {
        get => _absence;
        set
        {
            _absence = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TypeText));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusIcon));
            OnPropertyChanged(nameof(StatusDescription));
            OnPropertyChanged(nameof(DurationText));
            OnPropertyChanged(nameof(StartDateText));
            OnPropertyChanged(nameof(EndDateText));
            OnPropertyChanged(nameof(CreatedText));
            OnPropertyChanged(nameof(IsApproved));
            OnPropertyChanged(nameof(IsRejected));
            OnPropertyChanged(nameof(CanEdit));
        }
    }

    public string TypeText => Absence?.TypeText ?? string.Empty;
    public string StatusText => Absence?.StatusText ?? string.Empty;
    public Color StatusColor => Absence?.StatusColor ?? Colors.Gray;
    
    public string StatusIcon => Absence?.Status switch
    {
        BusinessLayer.AbsenceStatus.Pending => "⏳", // Pending
        BusinessLayer.AbsenceStatus.Approved => "✅", // Approved
        BusinessLayer.AbsenceStatus.Rejected => "❌", // Rejected
        BusinessLayer.AbsenceStatus.Cancelled => "🚫", // Cancelled
        _ => "❓"
    };

    public string StatusDescription => Absence?.Status switch
    {
        BusinessLayer.AbsenceStatus.Pending => "Your request is being reviewed by management",
        BusinessLayer.AbsenceStatus.Approved => "Your request has been approved",
        BusinessLayer.AbsenceStatus.Rejected => "Your request has been rejected",
        BusinessLayer.AbsenceStatus.Cancelled => "Your request has been cancelled",
        _ => "Unknown status"
    };

    public string DurationText => Absence?.DurationText ?? string.Empty;
    public string StartDateText => Absence != null ? $"{Absence.StartDate:dd/MM/yyyy}" : string.Empty;
    public string EndDateText => Absence != null ? $"{Absence.StartDate.AddDays(Absence.DaysCount - 1):dd/MM/yyyy}" : string.Empty;
    public string CreatedText => Absence?.CreatedText ?? string.Empty;

    public bool IsApproved => Absence?.Status == BusinessLayer.AbsenceStatus.Approved;
    public bool IsRejected => Absence?.Status == BusinessLayer.AbsenceStatus.Rejected;
    public bool CanEdit => Absence?.Status == BusinessLayer.AbsenceStatus.Pending; // Only pending requests can be edited

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public ICommand BackCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand CancelCommand { get; }

    public AbsenceDetailsPageModel(HttpService httpService)
    {
        _httpService = httpService;
        
        BackCommand = new Command(async () => await BackAsync());
        EditCommand = new Command(async () => await EditAsync());
        CancelCommand = new Command(async () => await CancelAsync());

        LoadAbsenceDetails();
    }

    private void LoadAbsenceDetails()
    {
        // Get the selected absence from the static property
        if (AbsenceDetailsPage.SelectedAbsence != null)
        {
            Absence = AbsenceDetailsPage.SelectedAbsence;
        }
    }

    private async Task BackAsync()
    {
        await Shell.Current.GoToAsync("//AllAbsencesPage");
    }

    private async Task EditAsync()
    {
        if (Absence == null) return;

        try
        {
            IsBusy = true;
            
            // Navigate to edit page (could be the same as AbsencePage with pre-filled data)
            await Shell.Current.DisplayAlert("Edit", "Edit functionality will be implemented in the future", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to edit absence: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        if (Absence == null) return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Cancel Request", 
            "Are you sure you want to cancel this absence request? This action cannot be undone.", 
            "Cancel Request", 
            "Keep Request");

        if (!confirmed) return;

        try
        {
            IsBusy = true;

            // Call API to cancel the absence request
            var success = await _httpService.CancelAbsenceAsync(Absence.Id);
            
            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Absence request has been cancelled", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to cancel absence request", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to cancel absence: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 