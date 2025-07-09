using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class BusinessTrip
    {
        public int Id { get; set; }
        public BusinessTripStatus Status { get; set; }
        public DateTime IssueDate { get; set; }
        public string ProjectName { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public string? Task { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public byte TotalDays { get; set; }
        public CarOwnerShip CarOwnership { get; set; }
        public decimal Wage { get; set; }
        public decimal AccommodationMoney { get; set; }
        public string? CarBrand { get; set; }
        public string? CarRegistrationNumber { get; set; }
        public string CarTripDestination { get; set; } = null!;
        public DateTime DateOfArrival { get; set; }
        public string CarModel { get; set; } = null!;
        public float CarUsagePerHundredKm { get; set; }
        public double PricePerLiter { get; set; }
        public DateTime DepartureDate { get; set; }
        public string? ExpensesResponsibility { get; set; }
        public DateTime Created { get; set; }
        public string UserId { get; set; }

        public User? User { get; set; }
    }
}
