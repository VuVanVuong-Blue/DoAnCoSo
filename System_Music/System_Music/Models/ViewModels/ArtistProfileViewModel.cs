using System_Music.Models.SqlModels;

namespace System_Music.Models.ViewModels
{
    public class ArtistProfileViewModel
    {
        public Artist Artist { get; set; }
        public List<Track> Tracks { get; set; }
        public List<Album> Albums { get; set; }
    }
}