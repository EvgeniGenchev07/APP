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

        public HolidayDayController(HolidayDayContext holidayDayContext)
        {
            _holidayDayContext = holidayDayContext ?? throw new ArgumentNullException(nameof(holidayDayContext));
        }
        [HttpPost("create")]
        public IActionResult Create([FromBody] HolidayDay holidayDay)
        {
            if (holidayDay == null)
            {
                return BadRequest("Invalid holiday day data.");
            }
            if (_holidayDayContext.Create(holidayDay))
            {
                return Ok(holidayDay);
            }
            else
            {
                return BadRequest("Failed to create holiday day.");
            }
        }
        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var holidayDays = _holidayDayContext.GetAll();
            if (holidayDays != null && holidayDays.Any())
            {
                return Ok(holidayDays);
            }
            else
            {
                return NotFound("No holiday days found.");
            }
        }
    }
}
