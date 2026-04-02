namespace System_Music.Models.DTOs
{
    public class SearchResultDto
    {
        public List<TrackDto> Songs { get; set; } = new List<TrackDto>();
        public List<AlbumDto> Albums { get; set; } = new List<AlbumDto>();
        public List<ArtistDto> Artists { get; set; } = new List<ArtistDto>();
    }

    public class AlbumDto
    {
        public int AlbumId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public List<ArtistDto> Artists { get; set; } = new List<ArtistDto>();
        public List<TrackDto> Tracks { get; set; } = new List<TrackDto>();
    }
}
