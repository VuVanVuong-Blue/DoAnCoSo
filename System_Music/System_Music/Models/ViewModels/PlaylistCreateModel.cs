namespace System_Music.Models.ViewModels
{
    public class PlaylistCreateModel
    {
        public string Name { get; set; } // Tên playlist, có thể rỗng
        public string? UserId { get; set; } // Tùy chọn, lấy từ server nếu không gửi
        public bool IsPublic { get; set; } = false; // Tùy chọn, mặc định là false
    }
}