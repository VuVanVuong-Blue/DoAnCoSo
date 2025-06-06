using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class Playlist
    {
        [Key]
        public int PlaylistId { get; set; }

        [MaxLength(150)]
        public string? Name { get; set; } 

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public bool IsPublic { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public int? ImageMediaId { get; set; }
        [ForeignKey("ImageMediaId")]
        public UserMedia? ImageMedia { get; set; }
        public int? PlayCount { get; set; } // Thêm PlayCount
        public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
    }
}