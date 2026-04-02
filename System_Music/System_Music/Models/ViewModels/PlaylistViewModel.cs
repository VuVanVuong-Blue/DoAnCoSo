using System_Music.Models.DTOs;
using System.Collections.Generic;

namespace System_Music.Models.ViewModels
{
    public class PlaylistViewModel
    {
        public PlaylistDto? Playlist { get; set; }
        public List<TrackDto>? Tracks { get; set; }
        public Dictionary<int, DateTime>? LikeDates { get; set; }
        public UserDto? CurrentUser { get; set; }
    }
}