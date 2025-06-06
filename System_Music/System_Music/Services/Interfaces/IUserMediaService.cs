using System_Music.Models.SqlModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace System_Music.Services.Interfaces
{
    public interface IUserMediaService
    {
        Task AddUserMediaAsync(UserMedia userMedia);
        Task<UserMedia> GetUserMediaByIdAsync(int mediaId);
        Task DeleteUserMediaAsync(int mediaId);
        Task<List<UserMedia>> GetAllUserMediasAsync();
        Task<List<UserMedia>> GetMediaByUserAsync(string userId); // Thêm phương thức này
    }
}