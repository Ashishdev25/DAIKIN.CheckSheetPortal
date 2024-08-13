using DAIKIN.CheckSheetPortal.API.Setup;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace DAIKIN.CheckSheetPortal.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = "RoleAccess")]
    public class CheckSheetImageController : Controller
    {
        private readonly ICheckSheetImageService _iCheckSheetImageService;
        public CheckSheetImageController(ICheckSheetImageService iCheckSheetImageService)
        {
            _iCheckSheetImageService = iCheckSheetImageService;
        }
        [HttpPost]
        [RoleAccess("Creator,Reviewer,Approver,SuperAdmin")]
        public async Task<IActionResult> UploadAsync(string checkSheetId, string checkPointId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }
            var response = await _iCheckSheetImageService.UploadAsync(checkSheetId, checkPointId, file);

            return Ok(response);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadAsync(string folder, string fileName = "No_Image_Available.jpg", string uniqueFileName = "No_Image_Available.jpg", int width = 100, int height = 100)
        {
            var fileContent = await _iCheckSheetImageService.DownloadAsync(folder, uniqueFileName, width, height);

            if (fileContent != null)
            {
                var contentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                var contentType = GetContentType(uniqueFileName);
                return File(fileContent, contentType);
            }
            else
            {
                return NotFound("Image not found");
            }
        }
        private string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                // Add more cases as needed for other image types
                default:
                    return "application/octet-stream"; // Default to binary if the type is unknown
            }
        }
    }
}

