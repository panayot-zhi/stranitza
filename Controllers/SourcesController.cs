using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Utility;
using Serilog;
using stranitza.Repositories;
using stranitza.Services;

namespace stranitza.Controllers
{
    public class SourcesController : Controller
    {
        private readonly IndexService _index;
        private readonly LibraryService _library;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _link;

        public SourcesController(ApplicationDbContext context, LinkGenerator link, LibraryService library, IndexService index)
        {
            _link = link;
            _library = library;
            _index = index;
            _context = context;            
        }

        public async Task<IActionResult> Index(int? page, int? year, int? category, string origin)
        {
            origin = System.Net.WebUtility.UrlDecode(origin);
            var viewModel = await _context.StranitzaSources.GetSourcesPagedAsync(
                year: year, categoryId: category, origin: origin, pageIndex: page);

            /*
            // TODO: Functionality suspended, consult with m.vlashki
             
            foreach (var sourceIndexViewModel in viewModel.Records.Where(x => !string.IsNullOrEmpty(x.AuthorId)))
            {
                var user = await _context.Users.FindAsync(sourceIndexViewModel.AuthorId);
                sourceIndexViewModel.AuthorDisplayName = StranitzaExtensions.GetDisplayName(user);
                sourceIndexViewModel.AuthorAvatarPath = StranitzaExtensions.GetAvatarPath(user);
            }*/

            viewModel.YearFilter = _context.CountByYears.GetSourcesCountByYears();
            viewModel.CategoriesFilter = await _context.StranitzaCategories.GetCategoryFilterViewModelAsync();
            viewModel.OriginFilter = new[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ю", "Я", "„“" };

            viewModel.CurrentOrigin = origin;
            viewModel.CurrentCategoryId = category;
            viewModel.CurrentYear = year;            

            return View(viewModel);            
        }

        [StranitzaAuthorize(StranitzaRoles.Editor)]
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

        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public IActionResult Create(int? issueId)
        {
            return View(new SourceCreateViewModel
            {
                IssueId = issueId,
                Categories = new SelectList(_context.StranitzaCategories, "Id", "Name")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Create(SourceCreateViewModel vModel)
        {
            if (ModelState.IsValid)
            {
                var entry = await _context.StranitzaSources.CreateSourceAsync(vModel, uploader: User.GetUserName());

                if (!entry.IssueId.HasValue)
                {
                    // NOTE: Seek an issue to be linked with the source here immediately
                    await _library.AttachIssueToSource(entry);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = entry.Id });
            }

            vModel.Categories = new SelectList(_context.StranitzaCategories, "Id", "Name", vModel.CategoryId);

            return View(vModel);
        }

        [StranitzaAuthorize(StranitzaRoles.Editor)]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Edit(SourceEditViewModel vModel)
        {                        
            if (ModelState.IsValid)
            {
                try
                {
                    await _index.UpdateIndexRecord(vModel);
                    return RedirectToAction(nameof(Details), new { id = vModel.Id });
                }
                catch (Exception ex)
                {
                    StranitzaDbErrorHandler.Instance.HandleError(ModelState, ex);
                }                
            }

            vModel.Categories = new SelectList(_context.StranitzaCategories, "Id", "Name", vModel.CategoryId);

            return View(vModel);
        }

        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
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

        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
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

            return RedirectToAction("FindPage", new { id = source.Id });
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

            TempData.AddModalMessage("За съжаление поисканата от Вас страница все още не е част от дигиталния архив на списанието.");

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
            var uri = _link.GetUriByAction(HttpContext, "PreviewPdf", "Issues", new { id = issueId },
                options: new LinkOptions()
                {
                    LowercaseUrls = true                    
                });

            return $"{uri}#page={pageNumber}";
        }

        public async Task<IActionResult> Search(string q, int? page)
        {
            var vModel = await _context.StranitzaSources.SearchSourcesPagedAsync(q, page);

            vModel.SearchQuery = q;

            return View(vModel);
        }

        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> FindAuthor(int? id)
        {
            var entry = await _context.StranitzaSources.FindAsync(id);
            if (entry == null)
            {
                return NotFound();
            }

            var author = await _context.Users.FindAuthorAsync(entry.FirstName, entry.LastName);
            if (author != null)
            {
                _context.Attach(entry);

                entry.AuthorId = author.Id;

                await _context.SaveChangesAsync();

                TempData.AddModalMessage($"В системата беше открит автор '{author.Names}' ({author.UserName}) и асоцииран с произведението.", "success");
            }
            else
            {
                TempData.AddModalMessage($"Не беше открит автор '{entry.FirstName} {entry.LastName}' в системата.", "info");
            }

            return RedirectToAction("Details", new { id = entry.Id });
        }
    }
}
