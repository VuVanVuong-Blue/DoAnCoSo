namespace System_Music.Models.DTOs
{
    public class ChartRankingDto
    {
        public int ChartId { get; set; }
        public int TrackId { get; set; }
        public TrackDto Track { get; set; }
        public int Rank { get; set; }
        public int PreviousRank { get; set; }
        public int Score { get; set; }
        public string Country { get; set; }
        public string TimeFrame { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
