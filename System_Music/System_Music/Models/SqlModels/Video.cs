using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class Video
    {
        [Key]
        public string EncodeId { get; set; } // Sử dụng EncodeId làm khóa chính

        [Required, MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Thumbnail { get; set; }

        [MaxLength(1000)]
        public string ThumbnailM { get; set; }

        public int Duration { get; set; } // Thời lượng (giây)

        [MaxLength(500)]
        public string ArtistsNames { get; set; }

        [MaxLength(1000)]
        public string Link { get; set; }

        [MaxLength(1000)]
        public string Mp4_480 { get; set; }

        [MaxLength(1000)]
        public string Mp4_720 { get; set; }

        [MaxLength(1000)]
        public string Mp4_1080 { get; set; }

        [MaxLength(1000)]
        public string Hls { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public ICollection<VideoArtist> VideoArtists { get; set; } = new List<VideoArtist>();
    }
}