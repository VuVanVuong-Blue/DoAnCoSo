using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace System_Music.Models.SqlModels
{
    public class Download
    {
        [Key]
        public int DownloadId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int TrackId { get; set; }

        public DateTime DownloadTime { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("TrackId")]
        public Track Track { get; set; } = null!;
    }

}
