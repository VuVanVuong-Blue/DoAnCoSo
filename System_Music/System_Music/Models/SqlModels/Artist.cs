using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class Artist
    {
        [Key]
        public int ArtistId { get; set; }

        [Required, MaxLength(100)]
        [Column(TypeName = "nvarchar(500)")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? NormalizedName { get; set; }

        [MaxLength(500)]
        public string? Image { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "nvarchar(500)")]
        public string? Country { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        public DateTime? BirthDate { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        [MaxLength(50)] // Giả định ID từ Zing MP3 có độ dài tối đa 50 ký tự
        public string? ZingMp3ArtistId { get; set; } // Thêm cột mới

        public int? PlayCount { get; set; } // Thêm PlayCount
        public ICollection<TrackArtist> TrackArtists { get; set; } = new List<TrackArtist>();
        public ICollection<AlbumArtist> AlbumArtists { get; set; } = new List<AlbumArtist>();
        public ICollection<VideoArtist> VideoArtists { get; set; } = new List<VideoArtist>();
    }
}