using DAIKIN.CheckSheetPortal.API.Setup;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using DAIKIN.CheckSheetPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAIKIN.CheckSheetPortal.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = "RoleAccess")]
    public class ConfigurationController : Controller
    {
        private readonly IConfigurationService _iConfigurationService;
        public ConfigurationController(IConfigurationService iConfigurationservice)
        {
            _iConfigurationService = iConfigurationservice;
        }
        [HttpGet]
        [RoleAccess("Operator,Validator,Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _iConfigurationService.GetAllAsync();
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("SuperAdmin,Creator")]
        public async Task<IActionResult> CreateAsync([FromBody] Configuration _Configuration)
        {
            var user = User.GetUser();
            var response = await _iConfigurationService.CreateAsync(_Configuration, user);
            return Ok(response);
        }
        [HttpPut]
        [RoleAccess("SuperAdmin,Creator")]
        public async Task<IActionResult> UpdateAsync([FromBody] Configuration _Configuration )
        {
            var user = User.GetUser();
            var response = await _iConfigurationService.UpdateAsync(_Configuration.Id, _Configuration, user);
            return Ok(response);
        }
        [HttpDelete]
        [RoleAccess("SuperAdmin,Creator")]
        public async Task<IActionResult> DeleteAsync(string _id)
        {
            var user = User.GetUser();
            var response = await _iConfigurationService.DeleteAsync(_id, user);
            return Ok(response);
        }
    }
}
