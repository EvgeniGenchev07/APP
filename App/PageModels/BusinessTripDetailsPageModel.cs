using App.Pages;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace App.PageModels
{
    public partial class BusinessTripDetailsPageModel : ObservableObject
    {
        [ObservableProperty]
        private BusinessTripViewModel _businessTrip;

        [ObservableProperty]
        private decimal _totalExpenses;

        [ObservableProperty]
        private bool _canEdit;

        [ObservableProperty]
        private bool _isEditing;

        private BusinessTrip _originalBusinessTrip;

        public ICommand CancelCommand { get; }

        public BusinessTripDetailsPageModel()
        {
       
        }
        public BusinessTripDetailsPageModel(BusinessTripViewModel businessTrip)
        {
            BusinessTrip = businessTrip;
            _originalBusinessTrip = new BusinessTrip()
            {
                AccommodationMoney = businessTrip.AccommodationMoney,
                CarBrand = businessTrip.CarBrand,
                CarModel = businessTrip.CarModel,
            };
            CancelCommand = new Command(async () => await CancelAsync());
            CalculateTotalExpenses();
            UpdateCanEdit();
        }

        public BusinessTripDetailsPageModel(BusinessTrip businessTrip)
        {
            BusinessTrip = new BusinessTripViewModel(businessTrip);
            _originalBusinessTrip = CloneBusinessTrip(businessTrip);
            CancelCommand = new Command(async () => await CancelAsync());
            CalculateTotalExpenses();
            UpdateCanEdit();
        }

        partial void OnBusinessTripChanged(BusinessTripViewModel value)
        {
            CalculateTotalExpenses();
            UpdateCanEdit();
        }

        partial void OnIsEditingChanged(bool value)
        {
            if (!value)
            {
                CalculateTotalExpenses();
            }
        }

        private void CalculateTotalExpenses()
        {
            if (BusinessTrip != null)
            {
                TotalExpenses = BusinessTrip.Wage * BusinessTrip.Days + BusinessTrip.AccommodationMoney * BusinessTrip.Days;
            }
        }

        private void UpdateCanEdit()
        {
            CanEdit = BusinessTrip?.Status == BusinessTripStatus.Pending;
        }

        private BusinessTrip CloneBusinessTrip(BusinessTrip original)
        {
            return new BusinessTrip
            {
                Id = original.Id,
                Status = original.Status,
                IssueDate = original.IssueDate,
                ProjectName = original.ProjectName,
                UserFullName = original.UserFullName,
                Task = original.Task,
                StartDate = original.StartDate,
                EndDate = original.EndDate,
                TotalDays = original.TotalDays,
                CarOwnership = original.CarOwnership,
                Wage = original.Wage,
                AccommodationMoney = original.AccommodationMoney,
                CarBrand = original.CarBrand,
                CarRegistrationNumber = original.CarRegistrationNumber,
                CarTripDestination = original.CarTripDestination,
                DateOfArrival = original.DateOfArrival,
                CarModel = original.CarModel,
                CarUsagePerHundredKm = original.CarUsagePerHundredKm,
                PricePerLiter = original.PricePerLiter,
                DepartureDate = original.DepartureDate,
                ExpensesResponsibility = original.ExpensesResponsibility,
                Created = original.Created
            };
        }

        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        [RelayCommand]
        private void ToggleEdit()
        {
            IsEditing = !IsEditing;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (IsEditing)
            {
                var result = await Shell.Current.DisplayAlert("Потвърждение", 
                    "Искате ли да запазите промените?", "Да", "Не");
                
                if (result)
                {
                    CalculateTotalExpenses();
                    IsEditing = false;
                    
                    await Shell.Current.DisplayAlert("Успех", 
                        "Промените са запазени успешно!", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task CancelEdit()
        {
            if (IsEditing)
            {
                var result = await Shell.Current.DisplayAlert("Потвърждение", 
                    "Искате ли да отмените промените?", "Да", "Не");
                
                if (result)
                {
                    BusinessTrip = new BusinessTripViewModel(CloneBusinessTrip(_originalBusinessTrip));
                    IsEditing = false;
                }
            }
        }


        [RelayCommand]
        private async Task GoBack()
        {
            if (IsEditing)
            {
                var result = await Shell.Current.DisplayAlert("Предупреждение", 
                    "Имате незапазени промени. Искате ли да излезете?", "Да", "Не");
                
                if (!result)
                {
                    return;
                }
            }
            
            await Shell.Current.GoToAsync("..");
        }
    }
} 