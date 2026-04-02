using Microsoft.AspNetCore.Http;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

namespace System_Music.Services.Implementations
{
    public class MediaService : IMediaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MediaService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public MediaService(IUnitOfWork unitOfWork, ILogger<MediaService> logger, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không được trống");

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", subFolder);
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{subFolder}/{fileName}";
        }

        public async Task<byte[]?> GetImageBytesAsync(string url)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsByteArrayAsync();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi fetch ảnh từ URL: {url}");
                return null;
            }
        }
    }
}
using Microsoft.AspNetCore.Http;

namespace System_Music.Services.Interfaces
{
    public interface IMediaService
    {
        Task<string> UploadFileAsync(IFormFile file, string subFolder);
        Task<byte[]?> GetImageBytesAsync(string url);
    }
}
