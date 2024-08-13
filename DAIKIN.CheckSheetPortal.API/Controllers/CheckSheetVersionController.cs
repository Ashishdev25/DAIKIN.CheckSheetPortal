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
    public class CheckSheetVersionController : Controller
    {
        private readonly ICheckSheetVersionService _iCheckSheetVersionService;
        public CheckSheetVersionController(ICheckSheetVersionService iCheckSheetVersionService)
        {
            _iCheckSheetVersionService = iCheckSheetVersionService;
        }
        [HttpGet]
        [RoleAccess("Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> GetAllAsync(string globalSearch)
        {
            var response = await _iCheckSheetVersionService.GetAllAsync(globalSearch);
            return Ok(response);
        }
        [HttpGet]
        [RoleAccess("Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> GetLatestAsync(string globalSearch)
        {
            var response = await _iCheckSheetVersionService.GetLatestVersionsAsync(globalSearch);
            return Ok(response);
        }
        [HttpGet]
        [RoleAccess("Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> GetReviewersAndApproversAsync()
        {
            var response = await _iCheckSheetVersionService.GetReviewersAndApproversAsync();
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Creator,SuperAdmin")]
        public async Task<IActionResult> CreateAsync([FromBody] CheckSheetCreateDTO checkSheetCreateDTO)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetVersionService.CreateAsync(checkSheetCreateDTO, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Creator,SuperAdmin")]
        public async Task<IActionResult> CreateVersionAsync([FromBody] CheckSheetCreateVersionDTO checkSheetCreateVersionDTO)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetVersionService.CreateVersionAsync(checkSheetCreateVersionDTO, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Creator,SuperAdmin")]
        public async Task<IActionResult> UpdateWorkFlowAsync([FromBody] CheckSheetCreateVersionDTO checkSheetCreateVersionDTO)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetVersionService.UpdateWorkFlowAsync(checkSheetCreateVersionDTO, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Creator,SuperAdmin")]
        public async Task<IActionResult> ReplicateAsync([FromBody] CheckSheetReplicateDTO checkSheetReplicateDTO)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetVersionService.ReplicateAsync(checkSheetReplicateDTO, user);
            return Ok(response);
        }
        [HttpPut]
        [RoleAccess("Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> UpdateCheckSheetAsync(string checkSheetId, [FromBody] CheckSheetVersionUpdateDTO checkSheetVersionUpdate)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetVersionService.UpdateCheckSheetAsync(checkSheetId, checkSheetVersionUpdate, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> CreateCheckPointAsync(string checkSheetId, [FromBody] CheckPointDTO checkPointDTO)
        {
            var response = await _iCheckSheetVersionService.CreateCheckPointAsync(checkSheetId, checkPointDTO);
            return Ok(response);
        }
        [HttpPut]
        [RoleAccess("Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> UpdateCheckPointAsync(string checkSheetId, [FromBody] CheckPointDTO checkPointDTO)
        {
            var response = await _iCheckSheetVersionService.UpdateCheckPointAsync(checkSheetId, checkPointDTO);
            return Ok(response);
        }
        [HttpGet]
        [RoleAccess("Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> GetCheckSheetByIdAsync(string checkSheetId)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetVersionService.GetCheckSheetByIdAsync(checkSheetId, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Approver,SuperAdmin")]
        public async Task<IActionResult> ApproveCheckSheetAsync(string _id)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetVersionService.ApproveCheckSheetAsync(_id, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Reviewer,SuperAdmin")]
        public async Task<IActionResult> ReviewCheckSheetAsync(string _id)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetVersionService.ReviewCheckSheetAsync(_id, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> RejectCheckSheetAsync(string _id, string reason)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetVersionService.RejectCheckSheetAsync(_id, reason, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Creator,SuperAdmin")]
        public async Task<IActionResult> SubmitCheckSheetAsync(string _id, string changeDetails)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetVersionService.SubmitCheckSheetAsync(_id, changeDetails, user);
            return Ok(response);
        }
        [HttpDelete]
        [RoleAccess("Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> DeleteAsync(string _id)
        {
            var response = await _iCheckSheetVersionService.DeleteAsync(_id);
            return Ok(response);
        }
        [HttpDelete]
        [RoleAccess("Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> DeleteCheckPointAsync(string checkSheetId, string id)
        {
            var response = await _iCheckSheetVersionService.DeleteCheckPointAsync(checkSheetId, id);
            return Ok(response);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> MoveApprovedCheckSheets()
        {
            Request.Headers.TryGetValue("x-api-key", out var key_headers);
            if (key_headers.Count > 1 || string.IsNullOrWhiteSpace(key_headers.FirstOrDefault()))
            {
                return Unauthorized($"x-api-key is missing");
            }
            var api_key = key_headers.FirstOrDefault();
            var response = await _iCheckSheetVersionService.MoveApprovedCheckSheets(api_key);
            return Ok(response);
        }
    }
}
