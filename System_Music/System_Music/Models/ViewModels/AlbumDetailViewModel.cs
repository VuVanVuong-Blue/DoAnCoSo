using System_Music.Models.SqlModels;
using System.Collections.Generic;

namespace System_Music.Models.ViewModels
{
    public class AlbumDetailViewModel
    {
        public Album Album { get; set; }
        public List<Artist> Artists { get; set; }
        public List<Track> Tracks { get; set; }
    }
} 