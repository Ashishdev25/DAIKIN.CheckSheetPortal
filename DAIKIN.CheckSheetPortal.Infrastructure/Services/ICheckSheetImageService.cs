using Microsoft.AspNetCore.Http;

namespace DAIKIN.CheckSheetPortal.Infrastructure.Services
{
    public interface ICheckSheetImageService
    {
        Task<IOperationResponse> UploadAsync(string checkSheetId, string checkPointId, IFormFile file);
        Task<byte[]> DownloadAsync(string folder, string fileName, int width, int height);
        Task<byte[]> ImageToByteArrayAsync(string folder, string fileName);
    }
}
