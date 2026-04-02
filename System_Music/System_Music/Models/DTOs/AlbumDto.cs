namespace System_Music.Models.DTOs
{
    public class AlbumDto
    {
        public int AlbumId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<ArtistDto> Artists { get; set; } = new List<ArtistDto>();
        public List<TrackDto> Tracks { get; set; } = new List<TrackDto>();
        public string ZingMp3AlbumId { get; set; }
    }
}
