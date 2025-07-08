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
