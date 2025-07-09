using App.Services;
using BusinessLayer;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows.Input;

namespace App.PageModels;

public class AddUserPageModel : INotifyPropertyChanged
{
    private readonly HttpService _httpService;
    private bool _isBusy;
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _selectedRole = "Служител";

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
            OnPropertyChanged();
            ValidatePassword();
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            _confirmPassword = value;
            OnPropertyChanged();
            ValidateConfirmPassword();
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
    public string PasswordError { get; private set; } = string.Empty;
    public string ConfirmPasswordError { get; private set; } = string.Empty;
    public string RoleError { get; private set; } = string.Empty;

    public bool HasNameError => !string.IsNullOrEmpty(NameError);
    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);
    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);
    public bool HasConfirmPasswordError => !string.IsNullOrEmpty(ConfirmPasswordError);
    public bool HasRoleError => !string.IsNullOrEmpty(RoleError);

    public ICommand AddUserCommand { get; }
    public ICommand CancelCommand { get; }

    public AddUserPageModel(HttpService httpService)
    {
        _httpService = httpService;
        
        AddUserCommand = new Command(async () => await AddUserAsync());
        CancelCommand = new Command(async () => await CancelAsync());
    }

    private async Task AddUserAsync()
    {
        if (!ValidateForm())
        {
            await Application.Current.MainPage.DisplayAlert("Грешка при валидиране", "Моля поправете грешките във формуляра.", "OK");
            return;
        }

        try
        {
            IsBusy = true;

            var role = SelectedRole == "Администратор" ? Role.Admin : Role.Employee;
            using (var md5 = MD5.Create())
            {
                // Hash the password using MD5
                var passwordBytes = System.Text.Encoding.UTF8.GetBytes(Password);
                var hashedPassword = md5.ComputeHash(passwordBytes);
                Password = BitConverter.ToString(hashedPassword).Replace("-", "").ToLowerInvariant();
            }
                var user = new User
                {
                    Name = Name,
                    Email = Email,
                    Password = Password,
                    Role = role
                };

            // Call API to create user
            var success = await _httpService.CreateUserAsync(user);
            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Success", "User created successfully", "OK");
                await Shell.Current.GoToAsync("//AdminUsersPage");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", "Неуспешно създаване на потребител", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно създаване на потребител: {ex.Message}", "OK");
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
        ValidateConfirmPassword();
        ValidateRole();

        return !HasNameError && !HasEmailError && !HasPasswordError && !HasConfirmPasswordError && !HasRoleError;
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

    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = "Имейлът е задължително поле";
        }
        else if (!Email.Contains("@") || !Email.Contains("."))
        {
            EmailError = "Моля въведете правилен имейл";
        }
        else
        {
            EmailError = string.Empty;
        }
        OnPropertyChanged(nameof(EmailError));
        OnPropertyChanged(nameof(HasEmailError));
    }

    private void ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = "Паролата е задължително поле";
        }
        else if (Password.Length < 6)
        {
            PasswordError = "Паролата трябва да е поне 6 символа";
        }
        else
        {
            PasswordError = string.Empty;
        }
        OnPropertyChanged(nameof(PasswordError));
        OnPropertyChanged(nameof(HasPasswordError));
    }

    private void ValidateConfirmPassword()
    {
        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ConfirmPasswordError = "Моля потвърдете паролата си";
        }
        else if (ConfirmPassword != Password)
        {
            ConfirmPasswordError = "Паролата не съвпада";
        }
        else
        {
            ConfirmPasswordError = string.Empty;
        }
        OnPropertyChanged(nameof(ConfirmPasswordError));
        OnPropertyChanged(nameof(HasConfirmPasswordError));
    }

    private void ValidateRole()
    {
        if (string.IsNullOrWhiteSpace(SelectedRole))
        {
            RoleError = "Моля изберете роля";
        }
        else
        {
            RoleError = string.Empty;
        }
        OnPropertyChanged(nameof(RoleError));
        OnPropertyChanged(nameof(HasRoleError));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 