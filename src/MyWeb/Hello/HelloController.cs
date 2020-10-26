using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MyWeb.Controllers.Hello
{
    [PrimaryConstructor]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public partial class HelloController : ControllerBase
    {
        private readonly ILogger<HelloController> _logger;

        [HttpGet]
        public ActionResult Go()
        {
            _logger.LogInformation("Go ...");
            return Ok("Hello");
        }
    }
}