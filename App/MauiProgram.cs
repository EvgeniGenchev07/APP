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
            builder.Services.AddTransientWithShellRoute<RegisterPage,RegisterPageModel>("register");
            builder.Services.AddScoped(_ =>
            {
                return new HttpService(
                    new HttpClient
                    {
                        BaseAddress = new Uri("http://localhost:5105/")
                    });
            });
            return builder.Build();
        }
    }
}
