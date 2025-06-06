using System.ComponentModel.DataAnnotations;

namespace System_Music.Models.SqlModels
{
    public class PlaylistTrack
    {
        public int PlaylistId { get; set; }
        public Playlist Playlist { get; set; } // Navigation property

        public int TrackId { get; set; }
        public Track Track { get; set; } // Navigation property

        public DateTime AddedDate { get; set; } = DateTime.UtcNow; // Thời gian thêm vào playlist
    }
}