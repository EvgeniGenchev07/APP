using App.Services;
using BusinessLayer;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace App.PageModels;

public class RegisterPageModel : INotifyPropertyChanged
{
    private string _email;
    private string _password;
    private string _errorMessage;
    private bool _isBusy;
    private bool _isEnabled;
    private readonly HttpService _httpService;

    public event PropertyChangedEventHandler PropertyChanged;

   
    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            IsEnabled = CanRegister();

            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            IsEnabled = CanRegister();

            OnPropertyChanged();
        }
    }


    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
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

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            OnPropertyChanged();
        }
    }

    public ICommand RegisterCommand { get; }
    public RegisterPageModel(HttpService httpService)
    {
        RegisterCommand = new Command(async () => await RegisterAsync(), CanRegister);

        // Subscribe to property changes to re-evaluate CanExecute
        PropertyChanged += (_, __) => ((Command)RegisterCommand).ChangeCanExecute();
        _httpService = httpService;

    }

    private bool CanRegister()
    {
        return !IsBusy
            && !string.IsNullOrWhiteSpace(Email)
            && !string.IsNullOrWhiteSpace(Password);
    }

    private async Task RegisterAsync()
    {
        try
        {
            IsBusy = true;
            IsEnabled = false;
            ErrorMessage = string.Empty;

            // Validate email format
            if (!Email.Contains("@") || !Email.Contains("."))
            {
                ErrorMessage = "Моля въведи правилен имейл";
                return;
            }


            // Validate password length
            if (Password.Length < 6)
            {
                ErrorMessage = "Паролата трябва да е поне 6 символа";
                return;
            }

            User user = await _httpService.PostUserLogin(Email, Password);
            if (user == null)
            {
                ErrorMessage = "Неуспешна регистрация. Моля опитайте отново.";
                return;
            }
            App.User = user;
            // Registration successful - navigate based on role
            if (user.Role == Role.Admin)
            {
                await Shell.Current.GoToAsync("//AdminPage");
            }
            else
            {
                await Shell.Current.GoToAsync("//MainPage");
            }

            // Clear form
            Email = string.Empty;
            Password = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Неуспешна регистрация: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            IsEnabled = true;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        //IsEnabled = CanRegister();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}