namespace System_Music.Models.DTOs
{
    public class ListenHistoryDto
    {
        public int ListenHistoryId { get; set; }
        public string UserId { get; set; }
        public int TrackId { get; set; }
        public TrackDto Track { get; set; }
        public DateTime ListenDate { get; set; }
    }
}
