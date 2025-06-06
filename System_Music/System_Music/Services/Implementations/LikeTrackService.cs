using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Interfaces;

public class LikeTrackService : ILikeTrackService
{
    private readonly ILikeTrackRepository _likeTrackRepository;
    private readonly SmartMusicDbContext _context;

    public LikeTrackService(SmartMusicDbContext context, ILikeTrackRepository likeTrackRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _likeTrackRepository = likeTrackRepository ?? throw new ArgumentNullException(nameof(likeTrackRepository));
    }

    public async Task<LikeTrack> GetLikeByUserAndTrackAsync(string userId, int trackId)
    {
        var likes = await _likeTrackRepository.GetLikesByUserAsync(userId);
        return likes.FirstOrDefault(lt => lt.TrackId == trackId);
    }

    public async Task<List<LikeTrack>> GetAllLikesAsync()
    {
        return await _likeTrackRepository.GetAllAsync();
    }

    public async Task<LikeTrack> GetLikeByIdAsync(int id)
    {
        return await _likeTrackRepository.GetByIdAsync(id);
    }

    public async Task AddLikeAsync(LikeTrack likeTrack)
    {
        await _likeTrackRepository.AddAsync(likeTrack);
    }

    public async Task DeleteLikeAsync(int id)
    {
        await _likeTrackRepository.DeleteAsync(id);
    }

    public async Task<List<LikeTrack>> GetLikesByUserAsync(string userId)
    {
        return await _likeTrackRepository.GetLikesByUserAsync(userId);
    }

    public async Task<List<LikeTrack>> GetLikesByTrackAsync(int trackId)
    {
        return await _likeTrackRepository.GetLikesByTrackAsync(trackId);
    }

    public async Task<bool> HasLikedTrackAsync(string userId, int trackId)
    {
        return await _likeTrackRepository.HasUserLikedTrackAsync(userId, trackId);
    }
}
