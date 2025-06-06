using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task DeleteAsync(string id);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(string id);
    }
}