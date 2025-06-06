using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class ChartRanking
    {
        [Key]
        public int RankingId { get; set; }

        [Required]
        public int TrackId { get; set; }
        [ForeignKey("TrackId")]
        public virtual Track Track { get; set; } = null!;

        [Required]
        public int RankPosition { get; set; }

        public float TrendScore { get; set; }
        public int TotalPlays { get; set; }
        public int TotalLikes { get; set; }

        public DateTime Date { get; set; }
        public string TimeFrame { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}