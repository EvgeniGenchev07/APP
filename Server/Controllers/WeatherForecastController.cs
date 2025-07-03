using DataLayer;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly UserContext _userContext;
        public WeatherForecastController(ILogger<WeatherForecastController> logger,UserContext userContext)
        {
            _logger = logger;
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public string Get()
        {
            return (_userContext.ReadAll());
        }
    }
}
