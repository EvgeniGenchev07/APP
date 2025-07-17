using App.Services;
using BusinessLayer;
using System.Diagnostics;
using System.Globalization;

namespace App
{
    public partial class App : Application
    {
        internal static User User { get; set; }

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
            Current.UserAppTheme = AppTheme.Light;

            var culture = CultureInfo.CreateSpecificCulture("bg-BG");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

    }
}
