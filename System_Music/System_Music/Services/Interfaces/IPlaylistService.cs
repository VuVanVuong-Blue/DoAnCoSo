using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;

namespace System_Music.Services.Interfaces
{
    public interface IPlaylistService
    {
        Task<List<PlaylistDto>> GetAllPlaylistsAsync();
        Task<PlaylistDto> GetPlaylistByIdAsync(int id);
        Task<List<PlaylistDto>> GetUserPlaylistsAsync(string userId);
        Task AddPlaylistAsync(Playlist playlist);
        Task UpdatePlaylistAsync(Playlist playlist);
        Task DeletePlaylistAsync(int id);
        Task CreatePlaylistAsync(Playlist playlist);
    }
}