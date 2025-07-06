using App.Services;
using App.ViewModels;
using BusinessLayer;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using App.Pages;

namespace App.PageModels;

public class AllAbsencesPageModel : INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private bool _isRefreshing;
    private int _totalAbsences;
    private int _pendingAbsences;
    private int _approvedAbsences;
    private int _rejectedAbsences;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<AbsenceViewModel> AllAbsences { get; } = new();

    public int TotalAbsences
    {
        get => _totalAbsences;
        set
        {
            _totalAbsences = value;
            OnPropertyChanged();
        }
    }

    public int PendingAbsences
    {
        get => _pendingAbsences;
        set
        {
            _pendingAbsences = value;
            OnPropertyChanged();
        }
    }

    public int ApprovedAbsences
    {
        get => _approvedAbsences;
        set
        {
            _approvedAbsences = value;
            OnPropertyChanged();
        }
    }

    public int RejectedAbsences
    {
        get => _rejectedAbsences;
        set
        {
            _rejectedAbsences = value;
            OnPropertyChanged();
        }
    }

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

    public ICommand BackCommand { get; }
    public ICommand RefreshCommand { get; }

    public AllAbsencesPageModel(HttpService httpService)
    {
        _httpService = httpService;
        
        BackCommand = new Command(async () => await BackAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;

            if (App.User != null)
            {
                var absences = await _httpService.GetUserAbsencesAsync(App.User.Id);
                var sortedAbsences = absences.OrderByDescending(a => a.Created).ToList();

                AllAbsences.Clear();
                foreach (var absence in sortedAbsences)
                {
                    AllAbsences.Add(new AbsenceViewModel(absence));
                }

                // Calculate statistics
                TotalAbsences = absences.Count;
                PendingAbsences = absences.Count(a => a.Status == BusinessLayer.AbsenceStatus.Pending);
                ApprovedAbsences = absences.Count(a => a.Status == BusinessLayer.AbsenceStatus.Approved);
                RejectedAbsences = absences.Count(a => a.Status == BusinessLayer.AbsenceStatus.Rejected);
            }
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

    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadDataAsync();
        IsRefreshing = false;
    }

    private async Task BackAsync()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    public async void SelectAbsence(AbsenceViewModel absence)
    {
        if (absence != null)
        {
            AbsenceDetailsPage.SelectedAbsence = absence;
            await Shell.Current.GoToAsync("AbsenceDetailsPage");
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 