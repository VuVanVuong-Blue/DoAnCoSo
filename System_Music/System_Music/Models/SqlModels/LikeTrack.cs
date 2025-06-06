using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class LikeTrack
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Sửa từ int sang string

        [ForeignKey("UserId")]
        public User User { get; set; } = null!; // Navigation property

        public int TrackId { get; set; }
        [ForeignKey("TrackId")]
        public Track Track { get; set; } = null!;

        public DateTime LikeDate { get; set; } = DateTime.UtcNow;
    }
}