using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class Album
    {
        [Key]
        public int AlbumId { get; set; }

        [MaxLength(500)]
        public string? NormalizedName { get; set; }

        [Required, MaxLength(150)]
        [Column(TypeName = "nvarchar(500)")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Image { get; set; }

        public DateTime ReleaseDate { get; set; } // Loại bỏ mặc định để API cung cấp

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        [MaxLength(50)] // Giả định ID từ Zing MP3 có độ dài tối đa 50 ký tự
        public string? ZingMp3AlbumId { get; set; } // Thêm cột mới
        public int? PlayCount { get; set; } // Thêm PlayCount
        public ICollection<Track> Tracks { get; set; } = new List<Track>();
        public ICollection<AlbumArtist> AlbumArtists { get; set; } = new List<AlbumArtist>();
    }
}