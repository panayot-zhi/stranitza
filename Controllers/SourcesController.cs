using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using stranitza.Models.Database;
using stranitza.Models.Database.Repositories;
using stranitza.Models.ViewModels;
using stranitza.Utility;
using Serilog;
using stranitza.Services;

namespace stranitza.Controllers
{
    public class SourcesController : Controller
    {
        private readonly LibraryService _library;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _link;

        public SourcesController(ApplicationDbContext context, LinkGenerator link, LibraryService library)
        {
            _link = link;
            _library = library;
            _context = context;            
        }

        // GET: Sources
        public async Task<IActionResult> Index(int? page, int? year, int? category, string origin)
        {
            origin = System.Net.WebUtility.UrlDecode(origin);
            var viewModel = await _context.StranitzaSources.GetSourcesPagedAsync(
                year: year, categoryId: category, origin: origin, pageIndex: page);
            
            viewModel.CategoriesFilter = await _context.StranitzaCategories.GetCategoryFilterViewModelAsync();
            viewModel.YearFilter = _context.StranitzaSources.GetYearFilterViewModels();
            viewModel.OriginFilter = new[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ю", "Я", "„“" };

            viewModel.CurrentOrigin = origin;
            viewModel.CurrentCategoryId = category;
            viewModel.CurrentYear = year;            

            return View(viewModel);            
        }

        // GET: Sources/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vModel = await _context.StranitzaSources.GetSourceDetailsViewModelAsync(id.Value);

            if (vModel == null)
            {
                return NotFound();
            }

            return View(vModel);
        }

        // GET: Sources/Create
        public IActionResult Create()
        {
            return View(new SourceCreateViewModel
            {
                Categories = new SelectList(_context.StranitzaCategories, "Id", "Name")
            });
        }

        // POST: Sources/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SourceCreateViewModel vModel)
        {
            if (ModelState.IsValid)
            {
                var entry = await _context.StranitzaSources.CreateSourceAsync(vModel, uploader: User.GetUserName());
                
                // NOTE: Seek an issue to be linked with the source here immediately
                await _library.AttachIssueToSource(entry);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = entry.Id });
            }

            vModel.Categories = new SelectList(_context.StranitzaCategories, "Id", "Name", vModel.CategoryId);

            return View(vModel);
        }

        // GET: Sources/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vModel = await _context.StranitzaSources.GetSourceEditViewModelAsync(id.Value);

            if (vModel == null)
            {
                return NotFound();
            }

            vModel.Categories = new SelectList(_context.StranitzaCategories, "Id", "Name", vModel.CategoryId);

            return View(vModel);
        }

        // POST: Sources/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SourceEditViewModel vModel)
        {                        
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.StranitzaSources.UpdateSourceAsync(vModel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    StranitzaDbErrorHandler.Instance.HandleError(ModelState, ex);
                }                
            }

            vModel.Categories = new SelectList(_context.StranitzaCategories, "Id", "Name", vModel.CategoryId);

            return View(vModel);
        }

        // GET: Sources/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vModel = await _context.StranitzaSources.GetSourceDetailsViewModelAsync(id.Value);

            if (vModel == null)
            {
                return NotFound();
            }

            return View(vModel);
        }

        // POST: Sources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entry = await _context.StranitzaSources.FindAsync(id);

            _context.StranitzaSources.Remove(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> FindIssue(int id)
        {
            var source = await _context.StranitzaSources.FindAsync(id);
            if (source == null)
            {
                return NotFound();
            }

            if (source.EPageId.HasValue)
            {
                return RedirectToAction("Details", "EPages", new { id = source.EPageId.Value });
            }

            if (!source.IssueId.HasValue)
            {
                var success = await _library.AttachIssueToSource(source);

                if (!success)
                {
                    return RedirectToActionPreserveMethod("IssueNotFound", "Home");
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("FindPage", source.Id);
        }

        public async Task<IActionResult> FindPage(int id)
        {
            var source = await _context.StranitzaSources.FindAsync(id);
            if (source == null)
            {
                return NotFound();
            }            

            var issue = await _context.StranitzaIssues.Include(x => x.Pages)
                .SingleOrDefaultAsync(x => x.Id == source.IssueId);

            if (issue == null)
            {                
                return RedirectToActionPreserveMethod("IssueNotFound", "Home");
            }            

            // first off check if there is a gallery page with this number
            var page = issue.Pages.SingleOrDefault(x => x.PageNumber == source.StartingPage);
            if (page != null)
            {
                // is it available?
                if (page.IsAvailable)
                {
                    return Redirect(GetGallerySlideUrl(issue.Id, page.SlideNumber));
                }

                // lol, log this, this is weird
                Log.Logger.Warning("Page was found (image), but not available: {PageId}", page.Id);

                return Forbid();
            }

            // if there is a pdf, redirect to download link,
            // NOTE: the download link should handle rights
            if (issue.HasPdf)
            {
                return Redirect(GetPdfPageUrl(issue.Id, source.StartingPage));
            }

            // TODO: Append a message to the user that this page is not available!
            return RedirectToAction("Details", "Issues", new { id = issue.Id });
        }

        private string GetGallerySlideUrl(int issueId, int slideNumber)
        {
            var uri = _link.GetUriByAction(HttpContext, "Details", "Issues", new { id = issueId }, 
                options: new LinkOptions()
                {
                    LowercaseUrls = true
                });
            
            return $"{uri}#lg=1&slide={slideNumber}";
        }

        private string GetPdfPageUrl(int issueId, int pageNumber)
        {
            var uri = _link.GetUriByAction(HttpContext, "DownloadPdf", "Issues", new { id = issueId },
                options: new LinkOptions()
                {
                    LowercaseUrls = true                    
                });

            return $"{uri}#page={pageNumber}";
        }
    }
}
