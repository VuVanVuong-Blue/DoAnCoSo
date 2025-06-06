using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Services.Interfaces;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserActivityController : Controller
    {
        private readonly IPlayQueueService _playQueueService;
        private readonly IDownloadService _downloadService;
        private readonly IBlockListService _blockListService;
        private readonly IUserMediaService _userMediaService;

        public UserActivityController(
            IPlayQueueService playQueueService,
            IDownloadService downloadService,
            IBlockListService blockListService,
            IUserMediaService userMediaService)
        {
            _playQueueService = playQueueService;
            _downloadService = downloadService;
            _blockListService = blockListService;
            _userMediaService = userMediaService;
        }

        // Quản lý PlayQueue
        public async Task<IActionResult> PlayQueues()
        {
            var queues = await _playQueueService.GetAllQueuesAsync();
            return View(queues);
        }

        public async Task<IActionResult> DeletePlayQueue(int id)
        {
            await _playQueueService.DeleteQueueAsync(id);
            return RedirectToAction(nameof(PlayQueues));
        }

        // Quản lý Download
        public async Task<IActionResult> Downloads()
        {
            var downloads = await _downloadService.GetAllDownloadsAsync();
            return View(downloads);
        }

        public async Task<IActionResult> DeleteDownload(int id)
        {
            await _downloadService.DeleteDownloadAsync(id);
            return RedirectToAction(nameof(Downloads));
        }

        // Quản lý BlockList
        public async Task<IActionResult> BlockList()
        {
            var blocks = await _blockListService.GetAllBlocksAsync();
            return View(blocks);
        }

        public async Task<IActionResult> DeleteBlock(int id)
        {
            await _blockListService.DeleteBlockAsync(id);
            return RedirectToAction(nameof(BlockList));
        }

        // Quản lý UserMedia
        public async Task<IActionResult> UserMedias()
        {
            var userMedias = await _userMediaService.GetAllUserMediasAsync();
            return View(userMedias);
        }

        public async Task<IActionResult> DeleteUserMedia(int id)
        {
            await _userMediaService.DeleteUserMediaAsync(id);
            return RedirectToAction(nameof(UserMedias));
        }
    }
}