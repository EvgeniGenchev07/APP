using App.Services;
using BusinessLayer;
using System.Diagnostics;
using System.Globalization;

namespace App
{
    public partial class App : Application
    {
        internal static User User { get; set; }

        private readonly string folderName = "tempapp";
        private readonly string appDirectory = FileSystem.AppDataDirectory;
        private readonly HttpService _httpService;

        public App(HttpService httpService)
        {
            InitializeComponent();
            MainPage = new AppShell();
            Current.UserAppTheme = AppTheme.Light;

            var culture = CultureInfo.CreateSpecificCulture("bg-BG");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            _httpService = httpService;

            Application.Current.Dispatcher.Dispatch(async () =>
            {
                await CheckForAppUpdateAsync();
            });
        }

        private async Task CheckForAppUpdateAsync()
        {
            try
            {
                var response = await _httpService.GetAppVersionAsync();
                if (response != null)
                {
                    string version = response["version"].ToString();
                    string path = Path.Combine(appDirectory, folderName);

                    if (version != AppInfo.Current.VersionString)
                    {
                        await InstallAppUpdateAsync(response["downloadUrl"].ToString(), version, path);
                    }
                    else if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for update: {ex.Message}");
            }
        }

        private async Task InstallAppUpdateAsync(string fileUrl, string version, string path)
        {
            try
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Потвърждение",
                    "Сигурни ли сте, че искате да инсталирате нова версия на приложението?",
                    "Да, инсталирай",
                    "Отказ");

                if (!confirm)
                    return;

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                using HttpClient client = new HttpClient();
                byte[] fileBytes = await client.GetByteArrayAsync(fileUrl);

                string filePath = Path.Combine(path, $"app_{version}.msix");
                File.WriteAllBytes(filePath, fileBytes);

                /* var startInfo = new ProcessStartInfo
                  {
                      FileName = "explorer.exe",
                      Arguments = $"\"{filePath}\"",
                      UseShellExecute = true,
                      Verb = "runas"
                  };*/
                string msixFullPath = filePath.Replace('\\', '/'); // make URI-friendly path

                string uri = $"ms-appinstaller:?source=file:///{msixFullPath}";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = uri,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);
                Application.Current.Quit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during update: {ex.Message}");
                await Shell.Current.DisplayAlert("Грешка", "Инсталирането на новата версия не бе успешно. Моля, опитайте отново по-късно.", "OK");
            }
            finally
            {
                if (Directory.Exists(path))
                {
                    try
                    {
                        Directory.Delete(path, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to clean up temp files: {ex.Message}");
                    }
                }
            }
        }
    }
}
