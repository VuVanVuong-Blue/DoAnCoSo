using System;
using System.Collections.Generic;

namespace System_Music.Models.DTOs
{
    public class TrackDto
    {
        public int TrackId { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string AudioUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public int? AlbumId { get; set; }
        public string AlbumName { get; set; }
        public List<ArtistDto> Artists { get; set; } = new List<ArtistDto>();
        public List<GenreDto> Genres { get; set; } = new List<GenreDto>();
        public DateTime AddedDate { get; set; }
        public int PlayCount { get; set; }
        public int LikeCount { get; set; }
        public string ZingMp3TrackId { get; set; }
    }
}
