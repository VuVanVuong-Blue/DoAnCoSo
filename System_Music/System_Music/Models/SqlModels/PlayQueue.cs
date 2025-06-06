using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace System_Music.Models.SqlModels
{
    public class PlayQueue
    {
        [Key]
        public int QueueId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int TrackId { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Thứ tự phải là số nguyên dương.")]
        public int OrderNumber { get; set; }

        public DateTime AddedTime { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("TrackId")]
        public Track Track { get; set; } = null!;
    }

}
