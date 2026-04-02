using System_Music.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System_Music.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string id);
        Task<UserDto> GetUserByEmailAsync(string email);
        Task AddUserAsync(UserRegisterRequest request);
        Task UpdateUserAsync(UserDto userDto);
        Task DeleteUserAsync(string id);
        Task<List<string>> GetRolesAsync(string userId);
        Task<bool> AddToRoleAsync(string userId, string role);
        Task<bool> RemoveFromRolesAsync(string userId, IEnumerable<string> roles);
        Task UpdateRefreshTokenAsync(string userId, string refreshToken, DateTime expiryTime);
        Task<UserDto> GetUserByRefreshTokenAsync(string refreshToken);
    }
}