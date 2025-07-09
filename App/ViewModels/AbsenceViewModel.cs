using BusinessLayer;
using Microsoft.Maui.Graphics;

namespace App.ViewModels;

public class AbsenceViewModel
{
    private Absence _absence;

    public AbsenceViewModel(Absence absence)
    {
        _absence = absence;
    }

    public int Id => _absence.Id;
    public AbsenceStatus Status {get => _absence.Status;set => _absence.Status = value; }
    public DateTime StartDate => _absence.StartDate;
    public DateTime EndDate => _absence.StartDate.AddDays(_absence.DaysCount - 1);
    public int DaysCount => _absence.DaysCount;
    public int Days => _absence.DaysCount;
    public string CreatedDate => _absence.Created.ToString("MM/dd/yyyy");
    public string UserId => _absence.UserId;
    public string UserName => _absence.UserName ?? "Неизвестен потребител";

    public string TypeText => _absence.Type switch
    {
        AbsenceType.Vacation => "Ваканция",
        AbsenceType.SickLeave => "Болнични",
        AbsenceType.PersonalLeave => "Отпуск",
        AbsenceType.Other => "Други",
        _ => "Неизвестен"
    };
        
    public string StatusText => _absence.Status switch
    {
        AbsenceStatus.Pending => "В очакване",
        AbsenceStatus.Approved => "Одобрен",
        AbsenceStatus.Rejected => "Отхвърлен",
        AbsenceStatus.Cancelled => "Отказан",
        _ => "Неизвестен"
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

    public string DateRange => $"{_absence.StartDate:MM/dd/yyyy} - {EndDate:MM/dd/yyyy} ({_absence.DaysCount} дни)";
    
    public string DurationText => $"{_absence.DaysCount} ден{(_absence.DaysCount == 1 ? "" : "'/'дни")}";
    
    public string CreatedText => $"Заявено на {_absence.Created:MM/dd/yyyy}";

    public bool CanChangeStatus => Status == AbsenceStatus.Pending;
} 