using DAIKIN.CheckSheetPortal.API.Setup;
using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using DAIKIN.CheckSheetPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAIKIN.CheckSheetPortal.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = "RoleAccess")]
    public class CheckSheetTransactionController : Controller
    {
        private readonly ICheckSheetTransactionService _iCheckSheetTransactionService;
        public CheckSheetTransactionController(ICheckSheetTransactionService iCheckSheetTransactionService)
        {
            _iCheckSheetTransactionService = iCheckSheetTransactionService;
        }
        [HttpGet]
        [RoleAccess("Validator,Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> GetTransactionsAsync(string globalSearch, bool searchByDate, DateTime fromDate, DateTime toDate)
        {
            var response = await _iCheckSheetTransactionService.GetTransactionsAsync(globalSearch, searchByDate, fromDate, toDate);
            return Ok(response);
        }
        [HttpGet]
        [RoleAccess("Validator,Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> ExportTransactionsAsync(bool exportWithCheckPoints, string globalSearch, bool searchByDate, DateTime fromDate, DateTime toDate)
        {
            var (memoryStream, fileName) = await _iCheckSheetTransactionService.ExportTransactionsAsync(exportWithCheckPoints, globalSearch, searchByDate, fromDate, toDate);
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        [HttpGet]
        [RoleAccess("Operator,Validator,SuperAdmin")]
        public async Task<IActionResult> GetCheckSheetsAsync(string globalSearch)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetTransactionService.GetCheckSheetsAsync(globalSearch, user);
            return Ok(response);
        }
        [HttpGet]
        [RoleAccess("Operator,Validator,SuperAdmin")]
        public async Task<IActionResult> GetCheckSheetByIdAsync(string _id)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetTransactionService.GetCheckSheetByIdAsync(_id, user);
            return Ok(response);
        }
        [HttpGet]
        [RoleAccess("Operator,Validator,SuperAdmin")]
        public async Task<IActionResult> ExportCheckSheetByIdAsync(string _id)
        {
            var (memoryStream, fileName) = await _iCheckSheetTransactionService.ExportCheckSheetByIdAsync(_id);
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        [HttpPost]
        [RoleAccess("Operator,Validator,SuperAdmin")]
        public async Task<IActionResult> BulkUpdateCheckRecordAsync([FromBody] CheckPointBulkEntryDTO checkPointBulkEntryDTO)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetTransactionService.BulkUpdateCheckRecordAsync(checkPointBulkEntryDTO, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Operator,Validator,SuperAdmin")]
        public async Task<IActionResult> UpdateCheckRecordAsync([FromBody] CheckPointEntryDTO checkPointEntryDTO)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetTransactionService.UpdateCheckRecordAsync(checkPointEntryDTO, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Validator,SuperAdmin")]
        public async Task<IActionResult> ApproveCheckSheetAsync(string _id)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetTransactionService.ApproveCheckSheetAsync(_id, user);
            return Ok(response);
        }
        [HttpPost]
        [RoleAccess("Operator,Validator,SuperAdmin")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> SubmitCheckSheetAsync(string _id)
        {
            var user = User.GetUser();
            var response = await _iCheckSheetTransactionService.SubmitCheckSheetAsync(_id, user);
            return Ok(response);
        }
        [HttpDelete]
        [RoleAccess("SuperAdmin")]
        public async Task<IActionResult> DeleteAsync(string _id)
        {
            var response = await _iCheckSheetTransactionService.DeleteAsync(_id);
            return Ok(response);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SendCheckSheetEmails()
        {
            Request.Headers.TryGetValue("x-api-key", out var key_headers);
            if (key_headers.Count > 1 || string.IsNullOrWhiteSpace(key_headers.FirstOrDefault()))
            {
                return Unauthorized($"x-api-key is missing");
            }
            var api_key = key_headers.FirstOrDefault();
            var response = await _iCheckSheetTransactionService.SendCheckSheetEmails(api_key);
            return Ok(response);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ArchiveAsync()
        {
            Request.Headers.TryGetValue("x-api-key", out var key_headers);
            if (key_headers.Count > 1 || string.IsNullOrWhiteSpace(key_headers.FirstOrDefault()))
            {
                return Unauthorized($"x-api-key is missing");
            }
            var api_key = key_headers.FirstOrDefault();
            var response = await _iCheckSheetTransactionService.ArchiveAsync(api_key);
            return Ok(response);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ArchiveInitialRunAsync()
        {
            Request.Headers.TryGetValue("x-api-key", out var key_headers);
            if (key_headers.Count > 1 || string.IsNullOrWhiteSpace(key_headers.FirstOrDefault()))
            {
                return Unauthorized($"x-api-key is missing");
            }
            var api_key = key_headers.FirstOrDefault();
            var response = await _iCheckSheetTransactionService.ArchiveInitialRunAsync(api_key);
            return Ok(response);
        }
    }
}
