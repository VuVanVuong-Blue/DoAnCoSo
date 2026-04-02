using System_Music.Models.DTOs;

namespace System_Music.Models.ViewModels
{
    public class ArtistProfileViewModel
    {
        public ArtistDto Artist { get; set; }
        public List<TrackDto> Tracks { get; set; }
        public List<AlbumDto> Albums { get; set; }
    }
}