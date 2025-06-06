using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class TrackGenre
    {
        [Key, Column(Order = 0)]
        public int TrackId { get; set; }
        public Track? Track { get; set; } // Cho phép nullable

        [Key, Column(Order = 1)]
        public int GenreId { get; set; }
        public Genre? Genre { get; set; } // Cho phép nullable
    }
}