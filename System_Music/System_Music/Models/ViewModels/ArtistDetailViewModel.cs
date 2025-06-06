using System_Music.Models.SqlModels;
using System.Collections.Generic;

namespace System_Music.Models.ViewModels
{
    public class ArtistDetailViewModel
    {
        public Artist Artist { get; set; }
        public List<Track> TopTracks { get; set; }
        public List<Album> Albums { get; set; }
    }
} 