using System;
using System.Text.Json;
using BusinessLayer;
using DataLayer;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BusinessTripController : Controller
    {
        private readonly BusinessTripContext _businessTripContext;

        public BusinessTripController(BusinessTripContext businessTripContext)
        {
            _businessTripContext = businessTripContext ?? throw new ArgumentNullException(nameof(businessTripContext));
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] BusinessTrip businessTrip)
        {
            if (businessTrip == null)
            {
                return BadRequest("Invalid business trip data.");
            }

            if (_businessTripContext.Create(businessTrip))
            {
                return Ok(JsonSerializer.Serialize(businessTrip));
            }
            else
            {
                return BadRequest("Failed to create business trip request.");
            }
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user ID.");
            }

            var businessTrips = _businessTripContext.GetByUserId(userId);
            if (businessTrips != null && businessTrips.Any())
            {
                return Ok(businessTrips);
            }
            else
            {
                return NotFound("No business trips found for this user.");
            }
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var businessTrips = _businessTripContext.GetAll();
            if (businessTrips != null && businessTrips.Any())
            {
                return Ok(businessTrips);
            }
            else
            {
                return NotFound("No business trips found.");
            }
        }

        [HttpPut("update")]
        public IActionResult Update([FromBody] BusinessTrip businessTrip)
        {
            if (businessTrip == null || businessTrip.Id <= 0)
            {
                return BadRequest("Invalid business trip data.");
            }

            if (_businessTripContext.Update(businessTrip))
            {
                return Ok(JsonSerializer.Serialize(businessTrip));
            }
            else
            {
                return BadRequest("Failed to update business trip request.");
            }
        }

        [HttpDelete("cancel/{businessTripId}")]
        public IActionResult Cancel(int businessTripId)
        {
            if (businessTripId <= 0)
            {
                return BadRequest("Invalid business trip ID.");
            }

            if (_businessTripContext.Delete(businessTripId))
            {
                return Ok("Business trip request cancelled successfully.");
            }
            else
            {
                return BadRequest("Failed to cancel business trip request.");
            }
        }

        [HttpGet("{businessTripId}")]
        public IActionResult GetById(int businessTripId)
        {
            if (businessTripId <= 0)
            {
                return BadRequest("Invalid business trip ID.");
            }

            var businessTrip = _businessTripContext.GetById(businessTripId);
            if (businessTrip != null)
            {
                return Ok(businessTrip);
            }
            else
            {
                return NotFound("Business trip not found.");
            }
        }
    }
} 