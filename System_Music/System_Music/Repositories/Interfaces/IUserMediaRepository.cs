using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IUserMediaRepository : IRepository<UserMedia>
    {
        Task<List<UserMedia>> GetMediaByUserAsync(string userId);
        Task AddAsync(UserMedia userMedia);
        Task<UserMedia> GetByIdAsync(int mediaId);
        Task DeleteAsync(int mediaId);
        Task<List<UserMedia>> GetAllAsync();
    }
}