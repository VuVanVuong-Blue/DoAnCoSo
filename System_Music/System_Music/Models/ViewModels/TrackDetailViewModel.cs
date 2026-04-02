using System_Music.Models.DTOs;
using System.Collections.Generic;

namespace System_Music.Models.ViewModels
{
    public class TrackDetailViewModel
    {
        public TrackDto Track { get; set; } = new TrackDto();
        public List<TrackDto> RecommendedTracks { get; set; } = new List<TrackDto>();
    }
}