using System_Music.Models.SqlModels;
using System.Threading.Tasks;

namespace System_Music.Interfaces
{
    public interface IVideoRepository
    {
        Task<Video> GetVideoByIdAsync(string encodeId);
        Task AddVideoAsync(Video video);
        Task AddVideoArtistsAsync(List<VideoArtist> videoArtists);
        Task AddOrUpdateVideoAsync(Video video); // Thêm phương thức này
        Task DeleteVideoAsync(string encodeId); // Thêm phương thức xóa
    }
}