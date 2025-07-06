using App.Pages;
using App.Services;
using App.ViewModels;
using BusinessLayer;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace App.PageModels;

public class EditUserPageModel : INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _selectedRole = "Employee";
    private string _absenceDays = "0";
    private string _userId = string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
            ValidateName();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            ValidateEmail();
        }
    }

    public string SelectedRole
    {
        get => _selectedRole;
        set
        {
            _selectedRole = value;
            OnPropertyChanged();
            ValidateRole();
        }
    }

    public string AbsenceDays
    {
        get => _absenceDays;
        set
        {
            _absenceDays = value;
            OnPropertyChanged();
            ValidateAbsenceDays();
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

    // Error properties
    public string NameError { get; private set; } = string.Empty;
    public string EmailError { get; private set; } = string.Empty;
    public string RoleError { get; private set; } = string.Empty;
    public string AbsenceDaysError { get; private set; } = string.Empty;

    public bool HasNameError => !string.IsNullOrEmpty(NameError);
    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);
    public bool HasRoleError => !string.IsNullOrEmpty(RoleError);
    public bool HasAbsenceDaysError => !string.IsNullOrEmpty(AbsenceDaysError);

    public ICommand UpdateUserCommand { get; }
    public ICommand CancelCommand { get; }

    public EditUserPageModel(HttpService httpService)
    {
        _httpService = httpService;
        
        UpdateUserCommand = new Command(async () => await UpdateUserAsync());
        CancelCommand = new Command(async () => await CancelAsync());

        LoadUserData();
    }

    private void LoadUserData()
    {
        if (EditUserPage.SelectedUser != null)
        {
            var user = EditUserPage.SelectedUser;
            _userId = user.Id;
            Name = user.Name;
            Email = user.Email;
            SelectedRole = user.Role == Role.Admin ? "Administrator" : "Employee";
            AbsenceDays = user.AbsenceDays.ToString();
        }
    }

    private async Task UpdateUserAsync()
    {
        if (!ValidateForm())
        {
            await Application.Current.MainPage.DisplayAlert("Validation Error", "Please fix the errors in the form.", "OK");
            return;
        }

        try
        {
            IsBusy = true;

            var role = SelectedRole == "Administrator" ? Role.Admin : Role.Employee;
            
            if (!int.TryParse(AbsenceDays, out int absenceDays))
            {
                absenceDays = 0;
            }

            var user = new User
            {
                Id = _userId,
                Name = Name,
                Email = Email,
                Role = role,
                AbsenceDays = absenceDays
            };

            // Call API to update user
            var success = await _httpService.UpdateUserAsync(user);
            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Success", "User updated successfully", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to update user", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to update user: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private bool ValidateForm()
    {
        ValidateName();
        ValidateEmail();
        ValidateRole();
        ValidateAbsenceDays();

        return !HasNameError && !HasEmailError && !HasRoleError && !HasAbsenceDaysError;
    }

    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Name is required";
        }
        else if (Name.Length < 2)
        {
            NameError = "Name must be at least 2 characters";
        }
        else
        {
            NameError = string.Empty;
        }
        OnPropertyChanged(nameof(NameError));
        OnPropertyChanged(nameof(HasNameError));
    }

    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = "Email is required";
        }
        else if (!Email.Contains("@") || !Email.Contains("."))
        {
            EmailError = "Please enter a valid email address";
        }
        else
        {
            EmailError = string.Empty;
        }
        OnPropertyChanged(nameof(EmailError));
        OnPropertyChanged(nameof(HasEmailError));
    }

    private void ValidateRole()
    {
        if (string.IsNullOrWhiteSpace(SelectedRole))
        {
            RoleError = "Please select a role";
        }
        else
        {
            RoleError = string.Empty;
        }
        OnPropertyChanged(nameof(RoleError));
        OnPropertyChanged(nameof(HasRoleError));
    }

    private void ValidateAbsenceDays()
    {
        if (string.IsNullOrWhiteSpace(AbsenceDays))
        {
            AbsenceDaysError = "Absence days is required";
        }
        else if (!int.TryParse(AbsenceDays, out int days) || days < 0)
        {
            AbsenceDaysError = "Please enter a valid number of days";
        }
        else
        {
            AbsenceDaysError = string.Empty;
        }
        OnPropertyChanged(nameof(AbsenceDaysError));
        OnPropertyChanged(nameof(HasAbsenceDaysError));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 