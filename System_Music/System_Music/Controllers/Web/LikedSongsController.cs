// LikedSongsController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Models.ViewModels;
using System_Music.Services.Interfaces;
using System.Threading.Tasks;
using System_Music.Services.Implementations;

public class LikedSongsController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ITrackService _trackService;
    private readonly ILikeTrackService _likeTrackService;

    public LikedSongsController(UserManager<User> userManager, ITrackService trackService, ILikeTrackService likeTrackService)
    {
        _userManager = userManager;
        _trackService = trackService;
        _likeTrackService = likeTrackService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var likedSongsPlaylist = new Playlist
        {
            PlaylistId = 0,
            Name = "Bài hát đã thích",
            UserId = user.Id,
            IsPublic = false,
            CreatedDate = DateTime.Now,
            ImageMediaId = null,
            PlaylistTracks = new List<PlaylistTrack>()
        };

        var likedTracks = await _trackService.GetLikedTracksAsync(user.Id);

        // Khởi tạo và điền dữ liệu vào LikeDates
        var likeDates = new Dictionary<int, DateTime>();
        foreach (var track in likedTracks)
        {
            var likeTrack = await _likeTrackService.GetLikeByUserAndTrackAsync(user.Id, track.TrackId);
            if (likeTrack != null)
            {
                likeDates[track.TrackId] = likeTrack?.LikeDate ?? DateTime.UtcNow;
            }
            else
            {
                likeDates[track.TrackId] = DateTime.UtcNow; // Giá trị mặc định nếu không có LikeDate
            }
            likedSongsPlaylist.PlaylistTracks.Add(new PlaylistTrack
            {
                Track = track,
                AddedDate = likeDates[track.TrackId]
            });
        }

        var model = new PlaylistViewModel
        {
            Playlist = likedSongsPlaylist,
            Tracks = likedTracks,
            LikeDates = likeDates
        };

        return View("~/Views/Playlist/IndexPlayList.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToLikedSongs(int trackId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var track = await _trackService.GetTrackByIdAsync(trackId);
        if (track == null)
        {
            return NotFound();
        }

        // Kiểm tra xem bài hát đã được thích chưa
        var hasLiked = await _likeTrackService.HasLikedTrackAsync(user.Id, trackId);
        if (!hasLiked)
        {
            var likeTrack = new LikeTrack
            {
                UserId = user.Id,
                TrackId = trackId,
                LikeDate = DateTime.UtcNow
            };
            await _likeTrackService.AddLikeAsync(likeTrack);
        }

        return Json(new { success = true });
    }
}