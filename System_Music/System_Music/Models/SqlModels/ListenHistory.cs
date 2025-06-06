using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public enum EntityType
    {
        Track,
        Album,
        Artist,
        Playlist
    }

    public class ListenHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public EntityType EntityType { get; set; }

        public int? TrackId { get; set; }
        [ForeignKey("TrackId")]
        public Track? Track { get; set; }

        public int? AlbumId { get; set; }
        [ForeignKey("AlbumId")]
        public Album? Album { get; set; }

        public int? ArtistId { get; set; }
        [ForeignKey("ArtistId")]
        public Artist? Artist { get; set; }

        public int? PlaylistId { get; set; }
        [ForeignKey("PlaylistId")]
        public Playlist? Playlist { get; set; }

        public DateTime ListenDate { get; set; } = DateTime.UtcNow;
    }
}