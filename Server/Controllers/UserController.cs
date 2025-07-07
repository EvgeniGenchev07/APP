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
    public class UserController : Controller
    {
        private readonly AuthenticationContext _authenticationContext;
        private readonly UserContext _userContext;

        public UserController(AuthenticationContext authenticationContext, UserContext userContext)
        {
            _authenticationContext = authenticationContext ?? throw new ArgumentNullException(nameof(authenticationContext));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] BusinessLayer.User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Invalid user data.");
            }

            if (_authenticationContext.Register(user))
            {
                return Ok(JsonSerializer.Serialize(user));
            }
            else
            {
                return Conflict("User already exists.");
            }

        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            if (loginModel == null || string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest("Invalid login data.");
            }
            User user = _authenticationContext.Authenticate(loginModel.Email, loginModel.Password);
            if (user != null) return Ok(user);
            else
            {
                return Unauthorized("Invalid email or password.");
            }
        }
        [HttpPost("create")]
        public IActionResult CreateUser([FromBody] BusinessLayer.User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Invalid user data.");
            }
            user.Id = Guid.NewGuid().ToString();
            if (_userContext.Create(user))
            {   
                return Ok(JsonSerializer.Serialize(user));
            }
            else
            {
                return Conflict("User already exists.");
            }
        }
        [HttpGet("all")]
        public IActionResult GetAllUsers()
        {
            try
            {
                var users = _userContext.ReadAll();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("delete/{userId}")]
        public IActionResult DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user ID.");
            }

            if (_userContext.Delete(userId))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, "Failed to delete user.");
            }
        }
    }
}
