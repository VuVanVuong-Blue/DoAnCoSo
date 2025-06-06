using System_Music.Models.SqlModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System_Music.Services.Interfaces
{
    public interface IPlaylistService
    {
        Task<List<Playlist>> GetAllPlaylistsAsync();
        Task<List<Playlist>> GetPlaylistsByUserAsync(string userId);
        Task<Playlist> GetPlaylistByIdAsync(int id);
        Task AddPlaylistAsync(Playlist playlist);
        Task UpdatePlaylistAsync(Playlist playlist);
        Task DeletePlaylistAsync(int id);
        Task<List<Playlist>> GetUserPlaylistsAsync(string userId);
        Task CreatePlaylistAsync(Playlist playlist);
    }
}