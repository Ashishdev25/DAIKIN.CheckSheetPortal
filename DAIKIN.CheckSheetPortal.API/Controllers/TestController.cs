using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DAIKIN.CheckSheetPortal.API.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TestController : Controller
    {
        private readonly ITestService _iService;
        public TestController(ITestService iService)
        {
            _iService = iService;
        }
        [HttpPost]
        public IActionResult SendEmail(string subject, string body, string env, string emailTo, string emailCC)
        {
            var result = _iService.SendMail(subject, body, env, emailTo, emailCC);
            return Ok(result);
        }
    }
}
