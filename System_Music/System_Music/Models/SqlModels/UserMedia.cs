using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class UserMedia
    {
        [Key]
        public int MediaId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [MaxLength(500)]
        public string MediaPath { get; set; } = string.Empty;

        public DateTime UploadTime { get; set; } = DateTime.UtcNow;

        public int? PlaylistId { get; set; } // Thêm liên kết với Playlist (nullable)
        [ForeignKey("PlaylistId")]
        public Playlist? Playlist { get; set; } // Navigation property
    }
}