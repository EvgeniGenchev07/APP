using BusinessLayer;
using Microsoft.Maui.Graphics;

namespace App.ViewModels;

public class AbsenceViewModel
{
    private readonly Absence _absence;

    public AbsenceViewModel(Absence absence)
    {
        _absence = absence;
    }

    public int Id => _absence.Id;
    public AbsenceStatus Status => _absence.Status;
    public DateTime StartDate => _absence.StartDate;
    public DateTime EndDate => _absence.StartDate.AddDays(_absence.DaysCount - 1);
    public int DaysCount => _absence.DaysCount;
    public int Days => _absence.DaysCount;
    public DateTime Created => _absence.Created;
    public DateTime CreatedDate => _absence.Created;
    public string UserId => _absence.UserId;
    public string UserName => _absence.User?.Name ?? "Unknown User";

    public string TypeText => _absence.Type switch
    {
        AbsenceType.Vacation => "Vacation",
        AbsenceType.SickLeave => "Sick Leave",
        AbsenceType.PersonalLeave => "Personal Leave",
        AbsenceType.Other => "Other",
        _ => "Unknown"
    };

    public string StatusText => _absence.Status switch
    {
        AbsenceStatus.Pending => "Pending",
        AbsenceStatus.Approved => "Approved",
        AbsenceStatus.Rejected => "Rejected",
        AbsenceStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };

    public Color StatusColor => _absence.Status switch
    {
        AbsenceStatus.Pending => Colors.Orange,
        AbsenceStatus.Approved => Colors.Green,
        AbsenceStatus.Rejected => Colors.Red,
        AbsenceStatus.Cancelled => Colors.Gray,
        _ => Colors.Gray
    };

    public Color TypeColor => _absence.Type switch
    {
        AbsenceType.Vacation => Colors.Blue,
        AbsenceType.SickLeave => Colors.Red,
        AbsenceType.PersonalLeave => Colors.Purple,
        AbsenceType.Other => Colors.Gray,
        _ => Colors.Gray
    };

    public string DateRange => $"{_absence.StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy} ({_absence.DaysCount} days)";
    
    public string DurationText => $"{_absence.DaysCount} day{(_absence.DaysCount == 1 ? "" : "s")}";
    
    public string CreatedText => $"Requested on {_absence.Created:dd/MM/yyyy}";

    public bool CanChangeStatus => _absence.Status == AbsenceStatus.Pending;
} 