using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private string _appVersion;
        private string downloadUrl = "http://78.130.149.254:60000/download";
        private readonly IConfiguration _configuration;
        public AppController(IConfiguration configuration = null)
        {
            _configuration = configuration;
            _appVersion = configuration["AppVersion"]??"1.0";

        }

        [HttpGet("getappversion")]
        public IActionResult GetAppVersion()
        {
            var response = new Dictionary<string, object>();
            response.Add("version", _appVersion);
            response.Add("downloadUrl", downloadUrl);
            return Ok(response);
        }
        [HttpGet("download")]
        public IActionResult GetDownloadUrl()
        {
            var filePath = Path.Combine("AppPackages", $"app_{_appVersion}.msix");

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/msix";
            var fileName = $"app_{_appVersion}.msix";
            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, contentType,fileName);
        }
    }
}
