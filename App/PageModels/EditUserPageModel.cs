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
    private string _selectedRole = "Служител";
    private string _password = string.Empty;
    private string _absenceDays = "0";
    private string _userId = string.Empty;
    private bool _isPasswordChanged = false;
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
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            _isPasswordChanged = true; // Mark that the password has been changed
            OnPropertyChanged();
            ValidatePassword();
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
    public string PasswordError { get; set; } = string.Empty;
    public string RoleError { get; private set; } = string.Empty;
    public string AbsenceDaysError { get; private set; } = string.Empty;
    public bool HasPasswordError => !string.IsNullOrEmpty(Password) && Password.Length < 6;
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
            Password = user.Password;
            SelectedRole = user.Role == Role.Admin ? "Administrator" : "Служител";
            AbsenceDays = user.AbsenceDays.ToString();
        }
    }

    private async Task UpdateUserAsync()
    {
        if (!ValidateForm())
        {
            await Application.Current.MainPage.DisplayAlert("Грешка при валидацията", "Моля поправи грешките във формуляра.", "OK");
            return;
        }

        try
        {
            IsBusy = true;

            var role = SelectedRole == "Служител" ? Role.Admin : Role.Employee;
            
            if (!int.TryParse(AbsenceDays, out int absenceDays))
            {
                absenceDays = 0;
            }
            if (_isPasswordChanged)
            {

            using(var md5 = System.Security.Cryptography.MD5.Create())
            {
                // Hash the password if it's provided
                if (!string.IsNullOrWhiteSpace(Password))
                {
                    var passwordBytes = System.Text.Encoding.UTF8.GetBytes(Password);
                    var hashedBytes = md5.ComputeHash(passwordBytes);
                    Password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            }
            var user = new User
            {
                Id = _userId,
                Name = Name,
                Email = Email,
                Password = Password,
                Role = role,
                AbsenceDays = absenceDays
            };

            // Call API to update user
            var success = await _httpService.UpdateUserAsync(user);
            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Успех", "Потребителят бе обновен успешно", "OK");
                await Shell.Current.GoToAsync("//AdminUsersPage");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", "Неуспешно обновяване на потребител", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно обновяване на потребител: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("//AdminUsersPage");
    }

    private bool ValidateForm()
    {
        ValidateName();
        ValidateEmail();
        ValidatePassword();
        ValidateRole();
        ValidateAbsenceDays();

        return !HasNameError && !HasEmailError && !HasRoleError && !HasAbsenceDaysError;
    }

    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Името е задължително поле";
        }
        else if (Name.Length < 2)
        {
            NameError = "Името трябва да е поне 2 символа";
        }
        else
        {
            NameError = string.Empty;
        }
        OnPropertyChanged(nameof(NameError));
        OnPropertyChanged(nameof(HasNameError));
    }
    public void ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            OnPropertyChanged(nameof(Password));
        }
        else if (Password.Length < 6)
        {
            PasswordError = "Паролата трябва да е поне 6 символа";
        }
        else
        {
            PasswordError = string.Empty;
        }
    }
    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = "Имейлът е задължително поле";
        }
        else if (!Email.Contains("@") || !Email.Contains("."))
        {
            EmailError = "Моля въведи правилен имейл";
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
            RoleError = "Моля избери роля";
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
            AbsenceDaysError = "Дните за отсъствие са задължително поле";
        }
        else if (!int.TryParse(AbsenceDays, out int days) || days < 0)
        {
            AbsenceDaysError = "Моля въведи правилен брой на дни";
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