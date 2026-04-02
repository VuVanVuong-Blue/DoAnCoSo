using System;

namespace System_Music.Models.DTOs
{
    public class LikeTrackDto
    {
        public int LikeTrackId { get; set; }
        public string UserId { get; set; }
        public int TrackId { get; set; }
        public DateTime LikeDate { get; set; }
        
        // Navigation properties if needed
        public TrackDto Track { get; set; }
    }
}
