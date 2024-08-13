using DAIKIN.CheckSheetPortal.API.Setup;
using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using DAIKIN.CheckSheetPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAIKIN.CheckSheetPortal.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = "RoleAccess")]
    public class UserController : Controller
    {
        private readonly IUserService _iUserService;
        public UserController(IUserService iUserservice)
        {
            _iUserService = iUserservice;
        }
        [HttpGet]
        [RoleAccess("SuperAdmin")]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _iUserService.GetAllAsync();
            return Ok(response);
        }
        [HttpGet]
        [RoleAccess("SuperAdmin")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var response = await _iUserService.GetByIdAsync(id);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("SuperAdmin")]
        public async Task<IActionResult> CreateAsync([FromBody] UserDTO _User)
        {
            var user = User.GetUser();
            var response = await _iUserService.CreateAsync(_User, user);
            return Ok(response);
        }
        [HttpPut]
        [RoleAccess("SuperAdmin")]
        public async Task<IActionResult> UpdateAsync([FromBody] UserDTO _User)
        {
            var user = User.GetUser();
            var response = await _iUserService.UpdateAsync(_User.Id, _User, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Operator,Validator,Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> UpdatePasswordAsync(string id, string newPassword, string confirmPassword)
        {
            var response = await _iUserService.UpdatePasswordAsync(id, newPassword, confirmPassword);
            return Ok(response);
        }
        [HttpDelete]
        [RoleAccess("SuperAdmin")]
        public async Task<IActionResult> DeleteAsync(string _id)
        {
            var response = await _iUserService.DeleteAsync(_id);
            return Ok(response);
        }
        [HttpGet]
        [RoleAccess("Operator,Validator,Creator,Reviewer,Approver,SuperAdmin")]
        [Route("current")]
        public async Task<IActionResult> GetCurrentAsync()
        {
            var currentUser = User.GetUser();
            if (currentUser != null)
            {
                return Ok(OperationResponse<UserDTO>.Success(currentUser));
            }

            return Ok(OperationResponse.Error(Common.Util.UserNotFound));
        }
    }
}
