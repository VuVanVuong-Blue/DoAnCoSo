using System.Collections.Generic;
using System.Threading.Tasks;
using System_Music.Models.DTOs;

namespace System_Music.Services.Interfaces
{
    public interface ILikeTrackService
    {
        Task<LikeTrackDto> GetLikeByUserAndTrackAsync(string userId, int trackId);
        Task<List<LikeTrackDto>> GetAllLikesAsync();
        Task<LikeTrackDto> GetLikeByIdAsync(int id);
        Task AddLikeAsync(LikeTrackDto likeTrackDto);
        Task DeleteLikeAsync(int id);
        Task<List<LikeTrackDto>> GetLikesByUserAsync(string userId);
        Task<List<LikeTrackDto>> GetLikesByTrackAsync(int trackId);
        Task<bool> HasLikedTrackAsync(string userId, int trackId);
    }
}