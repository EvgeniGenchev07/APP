using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private double _appVersion = 1.0;
        private string downloadUrl = "https://example.com/download";
        [HttpGet("getappversion")]
        public IActionResult GetAppVersion()
        {
            var response = new Dictionary<string, object>();
            response.Add("version", _appVersion);
            response.Add("downloadUrl", downloadUrl);
            return Ok(response);
        }
    }
}
