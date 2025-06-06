using System.ComponentModel.DataAnnotations;

namespace System_Music.Models.SqlModels
{
    public class VideoArtist
    {
        [Key]
        public int VideoArtistId { get; set; }

        [Required]
        public string VideoEncodeId { get; set; }

        [Required]
        public int ArtistId { get; set; }

        public Video Video { get; set; }
        public Artist Artist { get; set; }
    }
}