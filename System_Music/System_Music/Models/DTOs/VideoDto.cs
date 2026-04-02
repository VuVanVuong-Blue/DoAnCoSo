using System.Collections.Generic;

namespace System_Music.Models.DTOs
{
    public class VideoDto
    {
        public string EncodeId { get; set; }
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public string ThumbnailM { get; set; }
        public int Duration { get; set; }
        public string ArtistsNames { get; set; }
        public string Link { get; set; }
        public string Mp4_480 { get; set; }
        public string Mp4_720 { get; set; }
        public string Mp4_1080 { get; set; }
        public string Hls { get; set; }
        public List<ArtistDto> Artists { get; set; } = new List<ArtistDto>();
    }
}
