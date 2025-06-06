using System_Music.Models.SqlModels;
using System.Collections.Generic;

namespace System_Music.Models.ViewModels
{
    public class PlaylistViewModel
    {
        public Playlist? Playlist { get; set; }
        public List<Track>? Tracks { get; set; }
        public Dictionary<int, DateTime>? LikeDates { get; set; }
    }
}