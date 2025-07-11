using BusinessLayer;
using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using System.Text.Json;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly AuthenticationContext _authenticationContext;
        private readonly UserContext _userContext;
        private readonly ILogger<UserController> _logger;
        public UserController(AuthenticationContext authenticationContext, UserContext userContext, ILogger<UserController> logger)
        {
            _authenticationContext = authenticationContext;
            _userContext = userContext;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            try
            {
                if (loginModel == null || string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
                {
                    return BadRequest("Invalid login data.");
                }
                User user = _authenticationContext.Authenticate(loginModel.Email, loginModel.Password);
                if (user != null)
                {
                    _logger.LogInformation($"User {user.Email} logged in successfully.");
                    return Ok(user);
                }
                else
                {
                    _logger.LogWarning($"Failed login attempt for email: {loginModel.Email}");
                    return Unauthorized("Invalid email or password.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("create")]
        public IActionResult CreateUser([FromBody] BusinessLayer.User user)
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                {
                    return BadRequest("Invalid user data.");
                }
                user.Id = Guid.NewGuid().ToString();
                if (_userContext.Create(user))
                {
                    _logger.LogInformation($"User {user.Email} created successfully.");
                    return Ok(JsonSerializer.Serialize(user));
                }
                else
                {
                    _logger.LogWarning($"User creation failed for email: {user.Email}");
                    return Conflict("User already exists.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("all")]
        public IActionResult GetAllUsers()
        {
            try
            {
                var users = _userContext.ReadAll();
                _logger.LogInformation("Retrieved all users successfully.");
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("delete/{userId}")]
        public IActionResult DeleteUser(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest("Invalid user ID.");
                }

                if (_userContext.Delete(userId))
                {
                    _logger.LogInformation($"User with ID {userId} deleted successfully.");
                    return Ok();
                }
                else
                {
                    _logger.LogWarning($"Failed to delete user with ID {userId}. User not found.");
                    return StatusCode(500, "Failed to delete user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {userId}.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("edit")]
        public IActionResult EditUser([FromBody] User user)
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                {
                    return BadRequest("Invalid user data.");
                }
                if (_userContext.Update(user))
                {
                    _logger.LogInformation($"User {user.Email} updated successfully.");
                    return Ok(JsonSerializer.Serialize(user));
                }
                else
                {
                    _logger.LogWarning($"Failed to update user with ID {user.Id}. User not found.");
                    return NotFound("User not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
