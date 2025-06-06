using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System_Music.Models.SqlModels
{
    public class Genre
    {
        [Key]
        public int GenreId { get; set; } // Có thể đổi thành string nếu cần: public string GenreId { get; set; } = Guid.NewGuid().ToString();

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; } // Thêm mô tả thể loại
        // Thêm ID từ Zing MP3 để đồng bộ
        [MaxLength(50)]
        public string? ZingMp3GenreId { get; set; }

        // Quan hệ nhiều-nhiều với Track
        public ICollection<TrackGenre> TrackGenres { get; set; } = new List<TrackGenre>();
    }
}