using App.Services;
using BusinessLayer;
using DocumentFormat.OpenXml.Bibliography;
using System.Diagnostics;
using System.Globalization;

namespace App
{
    public partial class App : Application
    {
        internal static User User { get; set; }
        string folderName = "tempapp";
        string appDirectory = FileSystem.AppDataDirectory;



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
            httpService.GetAppVersionAsync().ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var response = task.Result;
                    if (response != null && response.TryGetValue("version", out var versionObj) && versionObj is double version)
                    {
                        string path = Path.Combine(appDirectory, folderName);
                        if (version.ToString() != AppInfo.Current.VersionString)
                        {
                            InstallAppUpdate(response["downloadUrl"].ToString(),version.ToString(), path);
                        }
                        else
                        {
                            if (Directory.Exists(path))
                            {
                                Directory.Delete(path);
                            }
                        }
                    }
                }
            });

        }
        private async void InstallAppUpdate(string fileUrl,string version, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using HttpClient client = new HttpClient();
            byte[] fileBytes = await client.GetByteArrayAsync(fileUrl);
            File.WriteAllBytes(path, fileBytes);
            var startInfo = new ProcessStartInfo
            {
                FileName = $"app_{version}.msix",
                Arguments = $"/i \"{path}\"",
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(startInfo);
                Application.Current.Quit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting process: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to install the update. Please try again later.", "OK");
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
