using System;
using System.Text.Json;
using BusinessLayer;
using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Server.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AbsenceController : Controller
    {
        private readonly AbsenceContext _absenceContext;
        private readonly UserContext _userContext;
        private readonly HolidayDayContext _holidayDayContext;
        public AbsenceController(AbsenceContext absenceContext, UserContext userContext, HolidayDayContext holidayDayContext)
        {
            _absenceContext = absenceContext ?? throw new ArgumentNullException(nameof(absenceContext));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(absenceContext));
            _holidayDayContext = holidayDayContext ?? throw new ArgumentNullException(nameof(holidayDayContext));
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] Absence absence)
        {
            if (absence == null)
            {
                return BadRequest("Invalid absence data.");
            }
                User user = _userContext.GetById(absence.UserId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                var holidays = _holidayDayContext.GetAll();
            user.AbsenceDays -= absence.DaysCount;
                if (!_userContext.Update(user))
                {
                    return BadRequest("Failed to update user absence days.");
                }
            if (_absenceContext.Create(absence))
            {
                return Ok(JsonSerializer.Serialize(absence));
            }
            else
            {
                return BadRequest("Failed to create absence request.");
            }
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user ID.");
            }

            var absences = _absenceContext.GetByUserId(userId);
            if (absences != null && absences.Any())
            {
                return Ok(absences);
            }
            else
            {
                return NotFound("No absences found for this user.");
            }
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var absences = _absenceContext.GetAll();
            if (absences != null && absences.Any())
            {
                return Ok(absences);
            }
            else
            {
                return NotFound("No absences found.");
            }
        }

        [HttpPut("update")]
        public IActionResult Update([FromBody] Absence absence)
        {
            if (absence == null || absence.Id <= 0)
            {
                return BadRequest("Invalid absence data.");
            }

            if (_absenceContext.Update(absence))
            {
                return Ok(JsonSerializer.Serialize(absence));
            }
            else
            {
                return BadRequest("Failed to update absence request.");
            }
        }

        [HttpDelete("cancel/{absenceId}")]
        public IActionResult Cancel(int absenceId)
        {
            if (absenceId <= 0)
            {
                return BadRequest("Invalid absence ID.");
            }

            if (_absenceContext.Delete(absenceId))
            {
                return Ok("Absence request cancelled successfully.");
            }
            else
            {
                return BadRequest("Failed to cancel absence request.");
            }
        }

        [HttpGet("{absenceId}")]
        public IActionResult GetById(int absenceId)
        {
            if (absenceId <= 0)
            {
                return BadRequest("Invalid absence ID.");
            }

            var absence = _absenceContext.GetById(absenceId);
            if (absence != null)
            {
                return Ok(absence);
            }
            else
            {
                return NotFound("Absence not found.");
            }
        }
        [HttpPut("requestupdate")]
        public IActionResult RequestUpdate([FromBody] RequestUpdateModel request)
        {
            if (request == null || request.Id <= 0)
            {
                return BadRequest("Invalid absence data.");
            }
            Absence absence = _absenceContext.GetById(request.Id);
            if (absence == null)
            {
                return NotFound("Absence not found.");
            }
            absence.Status = (AbsenceStatus)(request.Status);
            if (absence.Status == AbsenceStatus.Rejected)
            {
                User user = _userContext.GetById(absence.UserId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                user.AbsenceDays += absence.DaysCount;
                if (!_userContext.Update(user))
                {
                    return BadRequest("Failed to update user absence days.");
                }
            }
            if (_absenceContext.Update(absence))
            {
                return Ok(JsonSerializer.Serialize(absence));
            }
            else
            {
                return BadRequest("Failed to request update for absence.");
            }
        }
    }
} 