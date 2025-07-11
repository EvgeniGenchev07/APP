using App.PageModels;
using App.Pages;
using App.Services;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

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
            builder.Services.AddTransientWithShellRoute<RequestPage, RequestPageModel>("request");
            builder.Services.AddTransientWithShellRoute<RegisterPage, RegisterPageModel>("register");
            builder.Services.AddTransientWithShellRoute<MainPage, MainPageModel>("MainPage");
            builder.Services.AddTransientWithShellRoute<AdminPage, AdminPageModel>("AdminPage");
            builder.Services.AddTransientWithShellRoute<AdminUsersPage, AdminUsersPageModel>("AdminUsersPage");
            builder.Services.AddTransientWithShellRoute<AddUserPage, AddUserPageModel>("AddUserPage");
            builder.Services.AddTransientWithShellRoute<EditUserPage, EditUserPageModel>("EditUserPage");
            builder.Services.AddTransientWithShellRoute<AdminAllAbsencesPage, AdminAllAbsencesPageModel>("AdminAllAbsencesPage");
            builder.Services.AddTransientWithShellRoute<AdminAllBusinessTripsPage, AdminAllBusinessTripsPageModel>("AdminAllBusinessTripsPage");
            builder.Services.AddTransientWithShellRoute<AbsencePage, AbsencePageModel>("AbsencePage");
            builder.Services.AddTransientWithShellRoute<AllAbsencesPage, AllAbsencesPageModel>("AllAbsencesPage");
            builder.Services.AddTransientWithShellRoute<AbsenceDetailsPage, AbsenceDetailsPageModel>("AbsenceDetailsPage");
            builder.Services.AddTransientWithShellRoute<BusinessTripsPage, BusinessTripsPageModel>("businesstrips");
            builder.Services.AddTransientWithShellRoute<BusinessTripsSummaryPage, BusinessTripsSummaryPageModel>("BusinessTripsSummaryPage");
            builder.Services.AddTransient<BusinessTripDetailsPage>();
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
