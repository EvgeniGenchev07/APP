using BusinessLayer;
using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using System.Text.Json;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BusinessTripController : Controller
    {
        private readonly BusinessTripContext _businessTripContext;
        private readonly ILogger<BusinessTripController> _logger;

        public BusinessTripController(BusinessTripContext businessTripContext, ILogger<BusinessTripController> logger)
        {
            _businessTripContext = businessTripContext;
            _logger = logger;
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] BusinessTrip businessTrip)
        {
            try
            {
                if (businessTrip == null || string.IsNullOrEmpty(businessTrip.UserId) || string.IsNullOrEmpty(businessTrip.ProjectName))
                {
                    return BadRequest("Invalid business trip data.");
                }

                if (_businessTripContext.Create(businessTrip))
                {
                    _logger.LogInformation($"Business trip request created successfully for user {businessTrip.UserId}.");
                    return Ok(JsonSerializer.Serialize(businessTrip));
                }
                else
                {
                    _logger.LogWarning($"Failed to create business trip request for user {businessTrip.UserId}.");
                    return BadRequest("Failed to create business trip request.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating business trip request.");
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

                var businessTrips = _businessTripContext.GetByUserId(userId);
                if (businessTrips != null && businessTrips.Any())
                {
                    _logger.LogInformation($"Business trips found for user {userId}.");
                    return Ok(businessTrips);
                }
                else
                {
                    _logger.LogInformation($"No business trips found for user {userId}.");
                    return NotFound("No business trips found for this user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business trips by user ID.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            try
            {

                var businessTrips = _businessTripContext.GetAll();
                if (businessTrips != null && businessTrips.Any())
                {
                    _logger.LogInformation("Business trips retrieved successfully.");
                    return Ok(businessTrips);
                }
                else
                {
                    _logger.LogInformation("No business trips found.");
                    return NotFound("No business trips found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all business trips.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("update")]
        public IActionResult Update([FromBody] BusinessTrip businessTrip)
        {
            try
            {
                if (businessTrip == null || businessTrip.Id <= 0)
                {
                    return BadRequest("Invalid business trip data.");
                }

                if (_businessTripContext.Update(businessTrip))
                {
                    _logger.LogInformation($"Business trip request updated successfully for ID {businessTrip.Id}.");
                    return Ok(JsonSerializer.Serialize(businessTrip));
                }
                else
                {
                    _logger.LogWarning($"Failed to update business trip request for ID {businessTrip.Id}.");
                    return BadRequest("Failed to update business trip request.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating business trip request.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("cancel/{businessTripId}")]
        public IActionResult Cancel(int businessTripId)
        {
            try
            {

                if (businessTripId <= 0)
                {
                    return BadRequest("Invalid business trip ID.");
                }

                if (_businessTripContext.Delete(businessTripId))
                {
                    _logger.LogInformation($"Business trip request cancelled successfully for ID {businessTripId}.");
                    return Ok("Business trip request cancelled successfully.");
                }
                else
                {
                    _logger.LogWarning($"Failed to cancel business trip request for ID {businessTripId}.");
                    return BadRequest("Failed to cancel business trip request.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling business trip request.");
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
                    return BadRequest("Invalid business trip data.");
                }
                BusinessTrip businessTrip = _businessTripContext.GetById(request.Id);
                if (businessTrip == null)
                {
                    _logger.LogWarning($"Business trip not found for ID {request.Id}.");
                    return NotFound("Business trip not found.");
                }
                businessTrip.Status = (BusinessTripStatus)request.Status;
                if (_businessTripContext.Update(businessTrip))
                {
                    _logger.LogInformation($"Business trip request updated successfully for ID {businessTrip.Id}.");
                    return Ok(JsonSerializer.Serialize(businessTrip));
                }
                else
                {
                    _logger.LogWarning($"Failed to request update for business trip with ID {businessTrip.Id}.");
                    return BadRequest("Failed to request update for business trip.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting update for business trip.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}