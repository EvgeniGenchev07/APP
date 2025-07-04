using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace App.PageModels
{
    public class LoginPageModel : INotifyPropertyChanged
    {
        private string _email;
        private string _password;
        private bool _isBusy;
        private string _errorMessage;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand LoginCommand { get; }

        public LoginPageModel()
        {
            LoginCommand = new AsyncRelayCommand(OnLoginAsync);
        }

        private async Task OnLoginAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            await Task.Delay(1000); // Simulate API call

            if (Email == "admin" && Password == "password")
            {
                // Success: Navigate or show success
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }

            IsBusy = false;
        }

        public ICommand GoToRegisterCommand
        {
            get;
            set;
        } = new Command(() => Shell.Current.GoToAsync("//register"));

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
