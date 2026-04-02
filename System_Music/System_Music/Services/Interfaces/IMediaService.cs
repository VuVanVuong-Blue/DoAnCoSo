using Microsoft.AspNetCore.Http;

namespace System_Music.Services.Interfaces
{
    public interface IMediaService
    {
        Task<string> UploadFileAsync(IFormFile file, string subFolder);
        Task<byte[]?> GetImageBytesAsync(string url);
    }
}
