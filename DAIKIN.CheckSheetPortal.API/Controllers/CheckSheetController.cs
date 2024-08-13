using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAIKIN.CheckSheetPortal.API.Controllers
{
    [Route("api/[controller]/[action]")]
    public class CheckSheetController : Controller
    {
        private readonly ICheckSheetService _iCheckSheetService;
        public CheckSheetController(ICheckSheetService iCheckSheetService)
        {
            _iCheckSheetService = iCheckSheetService;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateDailyAsync()
        {
            Request.Headers.TryGetValue("x-api-key", out var key_headers);
            if (key_headers.Count > 1 || string.IsNullOrWhiteSpace(key_headers.FirstOrDefault()))
            {
                return Unauthorized($"x-api-key is missing");
            }
            var api_key = key_headers.FirstOrDefault();
            var response = await _iCheckSheetService.GenerateDailyCheckSheetsAsync(api_key);
            return Ok(response);
        }
    }
}
