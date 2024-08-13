using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class CheckSheetImageService : ICheckSheetImageService
    {
        private readonly ICheckSheetVersionService _service;
        private readonly IOptions<AppSettings> _options;
        public CheckSheetImageService(ICheckSheetVersionService service, IOptions<AppSettings> options)
        {
            _service = service;
            _options = options;
        }        
        public async Task<IOperationResponse> UploadAsync(string checkSheetId, string checkPointId, IFormFile file)
        {
            var uploadsFolder = _options.Value.FolderPath + "/images/";
            var uniqueFileName = ObjectId.GenerateNewId().ToString() + Path.GetExtension(file.FileName);
            var filePath = uploadsFolder + uniqueFileName;
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            var checkPoint = await _service.UpdateCheckSheetImageAsync(checkSheetId, checkPointId, uniqueFileName, file.FileName);

            return OperationResponse<CheckPoint>.Success(checkPoint, Common.Util.ImageUploadedSuccessfully);
        }
        public async Task<byte[]> DownloadAsync(string folder, string fileName, int width, int height)
        {
            var filePath = _options.Value.FolderPath + $"/{folder}/" + fileName;

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                filePath = _options.Value.FolderPath + $"/{folder}/No_Image_Available.jpg";
            }

            if (File.Exists(filePath))
            {
                using (var image = Image.Load(filePath))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = ResizeMode.Max
                    }));
                    using (var outputStream = new MemoryStream())
                    {
                        await image.SaveAsync(outputStream, new JpegEncoder());
                        return outputStream.ToArray();
                    }
                }
            }

            return null;
        }
        public async Task<byte[]> ImageToByteArrayAsync(string folder, string fileName)
        {
            var filePath = _options.Value.FolderPath + $"/{folder}/" + fileName;
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Image file not found.", filePath);
            }

            byte[] imageBytes;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }
            }

            return imageBytes;
        }
    }
}
