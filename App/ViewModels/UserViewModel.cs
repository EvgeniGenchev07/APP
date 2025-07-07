using BusinessLayer;
using Microsoft.Maui.Graphics;

namespace App.ViewModels;

public class UserViewModel
{
    private readonly User _user;

    public UserViewModel(User user)
    {
        _user = user;
    }

    public string Id => _user.Id;
    public string Name => _user.Name;
    public string Email => _user.Email;
    public string Password => _user.Password;
    public string UserName => _user.Name;
    public Role Role => _user.Role;
    public int AbsenceDays => _user.AbsenceDays;
    public int AbsencesCount => _user.Absences?.Count ?? 0;

    public string RoleText => _user.Role switch
    {
        Role.Admin => "Administrator",
        Role.Employee => "Employee",
        _ => "Unknown"
    };

    public Color RoleColor => _user.Role switch
    {
        Role.Admin => Colors.Red,
        Role.Employee => Colors.Blue,
        _ => Colors.Gray
    };
} 