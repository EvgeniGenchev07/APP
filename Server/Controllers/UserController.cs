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

        public UserController(AuthenticationContext authenticationContext)
        {
            _authenticationContext = authenticationContext ?? throw new ArgumentNullException(nameof(authenticationContext));
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
            if(user!=null) return Ok(user);
            else
            {
                return Unauthorized("Invalid email or password.");
            }
        }
    }
}
