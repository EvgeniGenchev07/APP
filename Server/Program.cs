using DataLayer;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
