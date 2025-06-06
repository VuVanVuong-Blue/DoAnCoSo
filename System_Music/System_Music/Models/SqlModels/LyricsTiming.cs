using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class LyricsTiming
    {
        [Key]
        public int LyricsTimingId { get; set; }

        public int TrackId { get; set; }
        [ForeignKey("TrackId")]
        public Track Track { get; set; }

        public int StartTime { get; set; } // Thời gian bắt đầu (ms)
        public int EndTime { get; set; }   // Thời gian kết thúc (ms)

        [MaxLength(500)]
        public string LyricText { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}