using DAIKIN.CheckSheetPortal.API.Setup;
using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using DAIKIN.CheckSheetPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAIKIN.CheckSheetPortal.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = "RoleAccess")]
    public class MasterSettingsController : Controller
    {
        private readonly IMasterSettingsService _iMasterSettingsService;
        public MasterSettingsController(IMasterSettingsService iMasterSettingsservice)
        {
            _iMasterSettingsService = iMasterSettingsservice;
        }
        [HttpGet]
        [RoleAccess("Operator,Validator,Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> GetSettingsAsync()
        {
            var response = await _iMasterSettingsService.GetSettingsAsync();
            return Ok(response);
        }
        [HttpPut]
        [RoleAccess("SuperAdmin")]
        public async Task<IActionResult> UpdateAsync([FromBody] MasterSettingsDTO _MasterSettings)
        {
            var user = User.GetUser();
            var response = await _iMasterSettingsService.UpdateAsync(_MasterSettings.Id, _MasterSettings, user);
            return Ok(response);
        }
    }
}
