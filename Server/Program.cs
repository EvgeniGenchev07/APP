using DataLayer;
using MySqlConnector;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Infinite)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<EapDbContext>(_ =>
{
    var connection = new MySqlConnection(connectionString);
    return new EapDbContext(connection);
});
builder.Services.AddScoped<UserContext>();
builder.Services.AddScoped<AuthenticationContext>();
builder.Services.AddScoped<AbsenceContext>();
builder.Services.AddScoped<HolidayDayContext>();
builder.Services.AddScoped<BusinessTripContext>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
