using BusinessLayer;
using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using System.Text.Json;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AbsenceController : Controller
    {
        private readonly AbsenceContext _absenceContext;
        private readonly UserContext _userContext;
        private readonly HolidayDayContext _holidayDayContext;
        private readonly ILogger<AbsenceController> _logger;
        public AbsenceController(AbsenceContext absenceContext, UserContext userContext, HolidayDayContext holidayDayContext, ILogger<AbsenceController> logger)
        {
            _absenceContext = absenceContext;
            _userContext = userContext;
            _holidayDayContext = holidayDayContext;
            _logger = logger;
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] Absence absence)
        {
            try
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
                if (absence.Type == AbsenceType.PersonalLeave)
                {

                    var holidays = _holidayDayContext.GetAll();
                    int holidaysCount = holidays.Count(h => h.Date >= absence.StartDate && h.Date < absence.StartDate.AddDays(absence.DaysCount));
                    for (int i = 0; i < absence.DaysCount; i++)
                    {
                        DateTime dateTime = absence.StartDate.AddDays(i);
                        if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday) holidaysCount++;
                    }
                    absence.DaysTaken = (byte)(absence.DaysCount - holidaysCount);
                    user.AbsenceDays -= absence.DaysTaken;
                    if (!_userContext.Update(user))
                    {
                        _logger.LogWarning($"Failed to update user absence days for user {user.Id}.");
                        return BadRequest("Failed to update user absence days.");
                    }
                }
                if (_absenceContext.Create(absence))
                {
                    _logger.LogInformation($"Absence request created successfully for user {absence.UserId}.");
                    return Ok(JsonSerializer.Serialize(absence));
                }
                else
                {
                    _logger.LogWarning($"Failed to create absence request for user {absence.UserId}.");
                    return BadRequest("Failed to create absence request.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating absence request.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Invalid user ID.");
                }

                var absences = _absenceContext.GetByUserId(userId);
                if (absences != null && absences.Any())
                {
                    _logger.LogInformation($"Retrieved {absences.Count} absences for user {userId}.");
                    return Ok(absences);
                }
                else
                {
                    _logger.LogInformation($"No absences found for user {userId}.");
                    return NotFound("No absences found for this user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving absences for user.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            try
            {
                var absences = _absenceContext.GetAll();
                if (absences != null && absences.Any())
                {
                    _logger.LogInformation($"Retrieved {absences.Count} absences.");
                    return Ok(absences);
                }
                else
                {
                    _logger.LogInformation("No absences found.");
                    return NotFound("No absences found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all absences.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("update")]
        public IActionResult Update([FromBody] Absence absence)
        {
            try
            {

                if (absence == null || absence.Id <= 0)
                {
                    return BadRequest("Invalid absence data.");
                }

                if (_absenceContext.Update(absence))
                {
                    _logger.LogInformation($"Absence request updated successfully for ID {absence.Id}.");
                    return Ok(JsonSerializer.Serialize(absence));
                }
                else
                {
                    _logger.LogWarning($"Failed to update absence request for ID {absence.Id}.");
                    return BadRequest("Failed to update absence request.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating absence request.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("cancel/{absenceId}")]
        public IActionResult Cancel(int absenceId)
        {
            try
            {
                if (absenceId <= 0)
                {
                    return BadRequest("Invalid absence ID.");
                }

                if (_absenceContext.Delete(absenceId))
                {
                    _logger.LogInformation($"Absence request with ID {absenceId} cancelled successfully.");
                    return Ok("Absence request cancelled successfully.");
                }
                else
                {
                    _logger.LogWarning($"Failed to cancel absence request with ID {absenceId}.");
                    return BadRequest("Failed to cancel absence request.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling absence request.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("requestupdate")]
        public IActionResult RequestUpdate([FromBody] RequestUpdateModel request)
        {
            try
            {
                if (request == null || request.Id <= 0)
                {
                    return BadRequest("Invalid absence data.");
                }
                Absence absence = _absenceContext.GetById(request.Id);
                if (absence == null)
                {
                    _logger.LogWarning($"Absence not found for ID {request.Id}.");
                    return NotFound("Absence not found.");
                }
                absence.Status = (AbsenceStatus)(request.Status);
                if (absence.Status == AbsenceStatus.Rejected)
                {
                    User user = _userContext.GetById(absence.UserId);
                    if (user == null)
                    {
                        _logger.LogWarning($"User not found for absence ID {absence.Id}.");
                        return NotFound("User not found.");
                    }
                    if (absence.Type == AbsenceType.PersonalLeave)
                    {
                        user.AbsenceDays += absence.DaysTaken;
                        if (!_userContext.Update(user))
                        {
                            _logger.LogWarning($"Failed to update user absence days for user {user.Id}.");
                            return BadRequest("Failed to update user absence days.");
                        }
                    }
                }
                if (_absenceContext.Update(absence))
                {
                    _logger.LogInformation($"Absence request with ID {absence.Id} updated successfully.");
                    return Ok(JsonSerializer.Serialize(absence));
                }
                else
                {
                    _logger.LogWarning($"Failed to request update for absence ID {absence.Id}.");
                    return BadRequest("Failed to request update for absence.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting update for absence.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}