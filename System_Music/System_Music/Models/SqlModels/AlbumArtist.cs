using System.ComponentModel.DataAnnotations;

namespace System_Music.Models.SqlModels
{
    public class AlbumArtist
    {
        public int AlbumId { get; set; }
        public Album Album { get; set; } // Navigation property

        public int ArtistId { get; set; }
        public Artist Artist { get; set; } // Navigation property
    }
}