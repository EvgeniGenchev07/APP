using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using App.Pages;
using App.PageModels;
using App.Services;

namespace App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddTransientWithShellRoute<RequestPage,RequestPageModel>("request");
            builder.Services.AddTransientWithShellRoute<LoginPage,LoginPageModel>("login");
            builder.Services.AddTransientWithShellRoute<RegisterPage,RegisterPageModel>("register");
            builder.Services.AddScoped(_ => {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5105/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                return new HttpService(httpClient);
            });
            return builder.Build();
        }
    }
}
