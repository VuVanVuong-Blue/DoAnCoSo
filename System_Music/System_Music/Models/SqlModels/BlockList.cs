using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class BlockList : IValidatableObject
    {
        [Key]
        public int BlockId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int? TrackId { get; set; }
        public int? ArtistId { get; set; }

        public DateTime BlockTime { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("TrackId")]
        public Track? Track { get; set; }

        [ForeignKey("ArtistId")]
        public Artist? Artist { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TrackId.HasValue && ArtistId.HasValue)
            {
                yield return new ValidationResult(
                    "Chỉ được nhập TrackId hoặc ArtistId, không thể có cả hai.",
                    new[] { nameof(TrackId), nameof(ArtistId) }
                );
            }

            if (!TrackId.HasValue && !ArtistId.HasValue)
            {
                yield return new ValidationResult(
                    "Phải có ít nhất một trong TrackId hoặc ArtistId.",
                    new[] { nameof(TrackId), nameof(ArtistId) }
                );
            }
        }
    }
}
