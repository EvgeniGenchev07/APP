using BusinessLayer;

namespace App
{
    public partial class App : Application
    {
        internal static User User { get; set; }
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
