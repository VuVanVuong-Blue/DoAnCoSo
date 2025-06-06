using System.ComponentModel.DataAnnotations;

namespace System_Music.Models.SqlModels
{
    public class TrackArtist
    {
        public int TrackId { get; set; }
        public Track Track { get; set; } // Navigation property

        public int ArtistId { get; set; }
        public Artist Artist { get; set; } // Navigation property

        [MaxLength(50)]
        public string Role { get; set; } = "Primary"; // Vai trò: Primary, Featured, Producer, v.v.
    }
}