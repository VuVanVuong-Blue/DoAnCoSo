using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace System_Music.Models.SqlModels
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string? Country { get; set; }

        public int? AvatarMediaId { get; set; } // Thêm trường này
        [ForeignKey("AvatarMediaId")]
        public UserMedia? AvatarMedia { get; set; } // Navigation property

        public User() {}

        // Các navigation properties
        public ICollection<BlockList>? BlockedItems { get; set; }
        public ICollection<Download>? Downloads { get; set; }
        public ICollection<Follower>? FollowedArtists { get; set; }
        public ICollection<LikeTrack>? LikedTracks { get; set; }
        public ICollection<PlayQueue>? PlayQueues { get; set; }
        public ICollection<Playlist>? Playlists { get; set; }
        public ICollection<UserMedia>? UserMedias { get; set; }
        public ICollection<ListenHistory>? ListenHistories { get; set; } = new List<ListenHistory>();
    }
}