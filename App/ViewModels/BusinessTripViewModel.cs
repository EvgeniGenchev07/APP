using BusinessLayer;
using Microsoft.Maui.Graphics;
using System;

namespace App.ViewModels
{
    public class BusinessTripViewModel
    {
        private readonly BusinessTrip _trip;

        public BusinessTripViewModel(BusinessTrip trip)
        {
            _trip = trip;
        }

        public int Id => _trip.Id;
        public BusinessTripStatus Status
        {
            get => _trip.Status;
            set => _trip.Status = value; // Preserved from first version
        }

        public DateTime StartDate => _trip.StartDate;
        public DateTime EndDate => _trip.EndDate;
        public string UserId => _trip.UserId;
        public string UserName => _trip.UserFullName ?? "Неизвестен потребител";
        public string UserFullName => _trip.UserFullName;

        public string ProjectName => _trip.ProjectName;
        public string CarTripDestination => _trip.CarTripDestination;
        public string Destination => _trip.CarTripDestination;
        public string Task => _trip.Task ?? "Няма посочена задача";

        public decimal Wage => _trip.Wage;
        public decimal AccommodationMoney => _trip.AccommodationMoney;
        public string CreatedDate => _trip.Created.ToString("MM/dd/yyyy");

        public int Days => (int)(_trip.EndDate - _trip.StartDate).TotalDays + 1;

        public string DateRange => $"{_trip.StartDate:MM/dd/yyyy} - {_trip.EndDate:MM/dd/yyyy} ({Days} ден{(Days == 1 ? "" : "'/'дни")})";

        public string DurationText => $"{Days} ден{(Days == 1 ? "" : "'/'дни")}";
        public string CreatedText => $"Заявено на {_trip.Created:MM/dd/yyyy}";

        public string CarModel => _trip.CarModel ?? "Не е посочен модел на автомобила";

        public string CarBrand => _trip.CarBrand ?? "Не е посочена марка на автомобила";

        public string CarRegistrationNumber => _trip.CarRegistrationNumber ?? "Не е посочен регистрационен номер";

        public string CarOwnership => _trip.CarOwnership switch
        {
            BusinessLayer.CarOwnership.Personal => "Личен автомобил",
            BusinessLayer.CarOwnership.Company => "Фирмен автомобил",
            BusinessLayer.CarOwnership.Rental => "Нает автомобил",
            _ => "Неизвестно"
        };

        public string CarUsagePerHundredKm => _trip.CarUsagePerHundredKm.ToString("F2") + " л/100км";

        public string PricePerLiter => _trip.PricePerLiter.ToString("F2") + " лв/л";

        public string ExpensesResponsibility => _trip.ExpensesResponsibility ?? "Не е посочено";
        public bool CanChangeStatus => Status == BusinessTripStatus.Pending;

        public string StatusText => _trip.Status switch
        {
            BusinessTripStatus.Pending => "В очакване",
            BusinessTripStatus.Approved => "Одобрен",
            BusinessTripStatus.Rejected => "Отхвърлен",
            BusinessTripStatus.Cancelled => "Отказан",
            BusinessTripStatus.Completed => "Изпълнен",
            _ => "Неизвестен"
        };

        public Color StatusColor => _trip.Status switch
        {
            BusinessTripStatus.Pending => Colors.Orange,
            BusinessTripStatus.Approved => Colors.Green,
            BusinessTripStatus.Rejected => Colors.Red,
            BusinessTripStatus.Cancelled => Colors.Gray,
            BusinessTripStatus.Completed => Colors.Blue,
            _ => Colors.Gray
        };
    }
}
