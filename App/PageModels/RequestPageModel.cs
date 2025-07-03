using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace App.PageModels
{
    public partial class RequestPageModel : ObservableObject
    {
        
        [ObservableProperty]
        string _organization = "ЕНЕРГИЙНА АГЕНЦИЯ-ПЛОВДИВ";

        [ObservableProperty]
        string _documentNumber = "16.0";

        [ObservableProperty]
        DateTime _documentDate = DateTime.Now;

        [ObservableProperty]
        string _project = "Стопанска дейност";

        [ObservableProperty]
        string _employeeName = "Десислава Николова";

        [ObservableProperty]
        string _destinationCity = "София";

        [ObservableProperty]
        string _returnCity = "Пловдив";

        [ObservableProperty]
        int _durationDays = 1;

        [ObservableProperty]
        DateTime _tripStartDate = DateTime.Now;

        [ObservableProperty]
        DateTime _tripEndDate = DateTime.Now;

        [ObservableProperty]
        string _task = "участие в работни срещи";

        [ObservableProperty]
        decimal _dailyAllowanceRate = 40.00m;

        [ObservableProperty]
        decimal _accommodationAllowanceRate = 0.00m;

        [ObservableProperty]
        string _vehicleType = "Opel";

        [ObservableProperty]
        string _vehicleModel = "Zafira";

        [ObservableProperty]
        string _fuelType = "D";

        [ObservableProperty]
        decimal _fuelConsumption = 10.00m;

        [ObservableProperty]
        decimal _totalExpenses = 40.00m;

        // Dropdown options
        [ObservableProperty]
        ObservableCollection<string> _cities = new()
        {
            "София", "Пловдив", "Варна", "Бургас", "Русе",
            "Стара Загора", "Плевен", "Велико Търново", "Сливен"
        };

        [ObservableProperty]
        ObservableCollection<string> _fuelTypes = new()
        {
            "D", "A95", "A98", "LPG", "Electric"
        };

        [ObservableProperty]
        ObservableCollection<string> _filteredCities = new();

        // Commands
        [RelayCommand]
        private void FilterCities(string searchText)
        {


            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var city in Cities)
                {
                    FilteredCities.Add(city);
                }
            }
            else
            {
                foreach (var city in Cities.Where(c =>
                    c.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                {
                    FilteredCities.Add(city);
                }
            }
        }

        [RelayCommand]
        private void CalculateTotalExpenses()
        {
            TotalExpenses = DailyAllowanceRate * DurationDays + AccommodationAllowanceRate * DurationDays;
        }

        [RelayCommand]
        private async Task SubmitRequest()
        {
            // Validate form
            if (string.IsNullOrWhiteSpace(EmployeeName) ||
                string.IsNullOrWhiteSpace(DestinationCity))
            {
                await Shell.Current.DisplayAlert("Грешка", "Моля, попълнете всички задължителни полета", "OK");
                return;
            }

            // Save logic would go here
            await Shell.Current.DisplayAlert("Успех", "Командировката е запазена успешно", "OK");

            // Could navigate back or clear form
        }

        partial void OnTripStartDateChanged(DateTime value)
        {
            TripStartDate = value;
            TripEndDate = value.AddDays(DurationDays);
            CalculateTotalExpenses();
        }

        partial void OnDurationDaysChanged(int value)
        {
            TripEndDate = TripStartDate.AddDays(value);
            CalculateTotalExpenses();
        }

        partial void OnDailyAllowanceRateChanged(decimal value)
        {
            CalculateTotalExpenses();
        }

        partial void OnAccommodationAllowanceRateChanged(decimal value)
        {
            CalculateTotalExpenses();
        }
    }
}