using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class Track
    {
        [Key]
        public int TrackId { get; set; }

        [Required, MaxLength(150)]
        [Column(TypeName = "nvarchar(500)")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? NormalizedTitle { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
        public int Duration { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string AudioUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(50)] // Giả định ID từ Zing MP3 có độ dài tối đa 50 ký tự
        public string? ZingMp3TrackId { get; set; } // Thêm cột mới để lưu ID từ Zing MP3

        public int? AlbumId { get; set; }
        [ForeignKey("AlbumId")]
        public Album? Album { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public int PlayCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;

        public ICollection<TrackArtist> TrackArtists { get; set; } = new List<TrackArtist>();
        public ICollection<TrackGenre> TrackGenres { get; set; } = new List<TrackGenre>();
        public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
        public ICollection<ListenHistory> ListenHistories { get; set; } = new List<ListenHistory>();
        public ICollection<LikeTrack> LikeTracks { get; set; } = new List<LikeTrack>();
       
    }
}