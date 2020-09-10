using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        private readonly NewsService _service;

        public PostsController(ApplicationDbContext context, NewsService service)
        {            
            _context = context;
            _service = service;
        }

        // GET: Posts
        public async Task<IActionResult> Index(int? page)
        {
            var vModel = await _context.StranitzaPosts.GetPostsPagedAsync(page);
            return View(vModel);
        }

        // GET: Posts/Details/5
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

            return View(vModel);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View(new PostCreateViewModel());
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Posts/Edit/5
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

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Posts/Delete/5
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

        // POST: Posts/Delete/5
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
    }
}
