using App.Services;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace App.PageModels;

public partial class AdminAllAbsencesPageModel :ObservableObject, INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private bool _isRefreshing;

    public event PropertyChangedEventHandler PropertyChanged;
    [ObservableProperty]
    public ObservableCollection<AbsenceViewModel> absences = new();

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
    public ICommand FilterCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }

    public AdminAllAbsencesPageModel(HttpService httpService)
    {
        _httpService = httpService;
        
        ApproveAbsenceCommand = new Command<AbsenceViewModel>(async (absence) => await ApproveAbsenceAsync(absence));
        RejectAbsenceCommand = new Command<AbsenceViewModel>(async (absence) => await RejectAbsenceAsync(absence));
        FilterCommand = new Command(async () => await FilterAbsencesAsync());
        CancelCommand = new Command(async () => await CancelAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());

        _ = LoadAbsencesAsync();
    }

    private async Task LoadAbsencesAsync()
    {
        try
        {
            IsBusy = true;

            var absences = await _httpService.GetAllAbsencesAsync();

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
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load absences: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApproveAbsenceAsync(AbsenceViewModel absence)
    {
        if (absence == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Confirm Approval", 
            $"Are you sure you want to approve this absence request?", 
            "Approve", 
            "Cancel");

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
                    await Application.Current.MainPage.DisplayAlert("Success", "Absence approved successfully", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to approve absence", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to approve absence: {ex.Message}", "OK");
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
            "Confirm Rejection", 
            $"Are you sure you want to reject this absence request?", 
            "Reject", 
            "Cancel");

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
                    await Application.Current.MainPage.DisplayAlert("Success", "Absence rejected successfully", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to reject absence", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to reject absence: {ex.Message}", "OK");
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

    private async Task FilterAbsencesAsync()
    {
        // Implement filtering functionality
        await LoadAbsencesAsync();
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