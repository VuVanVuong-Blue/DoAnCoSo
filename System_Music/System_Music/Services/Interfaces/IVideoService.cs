using System_Music.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System_Music.Services.Interfaces
{
    public interface IVideoService
    {
        Task<VideoDto> GetVideoByIdAsync(string encodeId);
        Task<IEnumerable<VideoDto>> GetAllVideosAsync();
        Task AddVideoAsync(VideoDto videoDto);
        Task UpdateVideoAsync(VideoDto videoDto);
        Task DeleteVideoAsync(string encodeId);
        Task<IEnumerable<VideoDto>> GetRelatedVideosAsync(string videoId);
    }
}