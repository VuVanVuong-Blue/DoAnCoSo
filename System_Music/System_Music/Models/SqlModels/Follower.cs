using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace System_Music.Models.SqlModels
{
    public class Follower
    {
        [Key]
        public int FollowId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ArtistId { get; set; }

        public DateTime FollowTime { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("ArtistId")]
        public Artist Artist { get; set; } = null!;
   
    }

}
