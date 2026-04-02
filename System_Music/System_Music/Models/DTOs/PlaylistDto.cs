namespace System_Music.Models.DTOs
{
    public class PlaylistDto
    {
        public int PlaylistId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string UserId { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<TrackDto> Tracks { get; set; } = new List<TrackDto>();
    }

    public class PlaylistCreateModel
    {
        public string Name { get; set; }
        public bool IsPublic { get; set; }
    }
}
