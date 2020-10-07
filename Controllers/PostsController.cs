using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Repositories;
using stranitza.Services;
using stranitza.Utility;

namespace stranitza.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StatisticService _stats;
        private readonly NewsService _service;

        public PostsController(ApplicationDbContext context, NewsService service, StatisticService stats)
        {            
            _context = context;
            _service = service;
            _stats = stats;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var vModel = await _context.StranitzaPosts.GetPostsPagedAsync(page);
            return View(vModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vModel = await _context.StranitzaPosts.GetPostDetailsAsync(id.Value);
            if (vModel == null)
            {
                return NotFound();
            }

#pragma warning disable 4014
            Task.Run(() =>
            {
                _stats.UpdatePostViewCountAsync(id.Value);
            });
#pragma warning restore 4014

            return View(vModel);
        }

        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public IActionResult Create()
        {
            return View(new PostCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Create(PostCreateViewModel vModel)
        {
            if (!ModelState.IsValid)
            {
                return View(vModel);
            }

            var currentUserId = User.GetUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                // this shouldn't happen
                return Challenge();
            }

            try
            {                
                if (vModel.ImageFile != null)
                {
                    var headImage = await _service.SaveAndCreatePostImageFileRecord(vModel.ImageFile);
                    vModel.ImageFileId = headImage.Id;
                }

                var entry = await _context.StranitzaPosts.CreatePostAsync(vModel, currentUserId);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = entry.Id });
            }
            catch (Exception ex)
            {
                StranitzaDbErrorHandler.Instance.HandleError(ModelState, ex);
            }

            return View(vModel);

        }

        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vModel = await _context.StranitzaPosts.GetPostEditAsync(id.Value);
            if (vModel == null)
            {
                return NotFound();
            }

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Edit(PostEditViewModel vModel)
        {
            if (!ModelState.IsValid)
            {
                return View(vModel);
            }

            try
            {

                int? oldImageId = null;
                if (vModel.ImageFile != null)
                {
                    // save current image id
                    oldImageId = vModel.ImageFileId;
                    var headImage = await _service.SaveAndCreatePostImageFileRecord(vModel.ImageFile);
                    vModel.ImageFileId = headImage.Id;  // new image                    
                }

                var entry = await _context.StranitzaPosts.UpdatePostAsync(vModel);

                await _context.SaveChangesAsync();

                if (oldImageId.HasValue)
                {
                    // clean up
                    await _service.DeletePostImageFileAndRecord(oldImageId.Value); // current image
                }                

                return RedirectToAction(nameof(Details), new { id = entry.Id });
            }
            catch (Exception ex)
            {
                StranitzaDbErrorHandler.Instance.HandleError(ModelState, ex);
            }

            return View(vModel);
        }

        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vModel = await _context.StranitzaPosts.GetPostEditAsync(id.Value);
            if (vModel == null)
            {
                return NotFound();
            }

            return View(vModel);
        }

        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entry = await _context.StranitzaPosts.FindAsync(id);

            _context.StranitzaPosts.Remove(entry);

            await _context.SaveChangesAsync();

            if (entry.ImageFileId.HasValue)
            {
                await _service.DeletePostImageFileAndRecord(entry.ImageFileId.Value);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Search(string q, int? page)
        {
            var vModel = await _context.StranitzaPosts.SearchPostsPagedAsync(q, page);

            vModel.SearchQuery = q;

            return View(vModel);
        }
    }
}
