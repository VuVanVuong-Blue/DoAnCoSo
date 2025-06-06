using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System_Music.Services.Implementations
{
    public class UserMediaService : IUserMediaService
    {
        private readonly IUserMediaRepository _userMediaRepository;

        public UserMediaService(IUserMediaRepository userMediaRepository)
        {
            _userMediaRepository = userMediaRepository;
        }

        public async Task AddUserMediaAsync(UserMedia userMedia)
        {
            await _userMediaRepository.AddAsync(userMedia);
        }

        public async Task<UserMedia> GetUserMediaByIdAsync(int mediaId)
        {
            return await _userMediaRepository.GetByIdAsync(mediaId);
        }

        public async Task DeleteUserMediaAsync(int mediaId)
        {
            await _userMediaRepository.DeleteAsync(mediaId);
        }

        public async Task<List<UserMedia>> GetAllUserMediasAsync()
        {
            return await _userMediaRepository.GetAllAsync();
        }

        public async Task<List<UserMedia>> GetMediaByUserAsync(string userId)
        {
            return await _userMediaRepository.GetMediaByUserAsync(userId);
        }
    }
}