using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class ChartInteraction
    {
        [Key]
        public int InteractionId { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public string ChartType { get; set; } = string.Empty;

        [Required]
        public int TrackId { get; set; }

        [Required]
        public string ActionType { get; set; } = string.Empty;
        // 👇 Thêm dòng này để fix lỗi
        public Track? Track { get; set; }

        public DateTime InteractionTime { get; set; } = DateTime.UtcNow;
    }
}
