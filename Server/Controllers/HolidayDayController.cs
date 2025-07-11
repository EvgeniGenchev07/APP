using BusinessLayer;
using DataLayer;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HolidayDayController : Controller
    {
        private readonly HolidayDayContext _holidayDayContext;
        private readonly ILogger<HolidayDayController> _logger;
        public HolidayDayController(HolidayDayContext holidayDayContext, ILogger<HolidayDayController> logger)
        {
            _holidayDayContext = holidayDayContext;
            _logger = logger;
        }
        [HttpPost("create")]
        public IActionResult Create([FromBody] HolidayDay holidayDay)
        {
            try
            {
                if (holidayDay == null)
                {
                    return BadRequest("Invalid holiday day data.");
                }
                if (_holidayDayContext.Create(holidayDay))
                {
                    _logger.LogInformation($"Holiday day created successfully: {holidayDay.Name} on {holidayDay.Date}");
                    return Ok(holidayDay);
                }
                else
                {
                    _logger.LogWarning($"Failed to create holiday day: {holidayDay.Name} on {holidayDay.Date}");
                    return BadRequest("Failed to create holiday day.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating holiday day.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            try
            {
                var holidayDays = _holidayDayContext.GetAll();
                if (holidayDays != null && holidayDays.Any())
                {
                    _logger.LogInformation("Retrieved all holiday days successfully.");
                    return Ok(holidayDays);
                }
                else
                {
                    _logger.LogInformation("No holiday days found.");
                    return NotFound("No holiday days found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving holiday days.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
