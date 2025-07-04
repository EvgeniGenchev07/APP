using System.Text.Json;
using DataLayer;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost ("register")]
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
    }
}
