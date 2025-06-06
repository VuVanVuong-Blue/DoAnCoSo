using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(SmartMusicDbContext context) : base(context)
        {
        }

        public Task DeleteAsync(string id)
        {
            return _context.Users
                .Where(u => u.Id == id)
                .ExecuteDeleteAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<User> GetByIdAsync(string id)
        {
            return _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}