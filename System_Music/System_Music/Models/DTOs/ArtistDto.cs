using System;
using System.Collections.Generic;

namespace System_Music.Models.DTOs
{
    public class ArtistDto
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string ImageUrl { get; set; }
        public string Country { get; set; }
        public string Biography { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool IsActive { get; set; }
        public string ZingMp3ArtistId { get; set; }
        public int? PlayCount { get; set; }
        
        // Navigation properties if needed as DTOs
        public List<TrackDto> Tracks { get; set; } = new List<TrackDto>();
        public List<AlbumDto> Albums { get; set; } = new List<AlbumDto>();
    }
}
