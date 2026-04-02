namespace System_Music.Models.DTOs
{
    public class GenreDto
    {
        public int GenreId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public List<TrackDto> Tracks { get; set; } = new List<TrackDto>();
    }
}
