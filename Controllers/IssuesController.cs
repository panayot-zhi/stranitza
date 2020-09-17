using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Repositories;
using stranitza.Services;
using stranitza.Utility;

namespace stranitza.Controllers
{
    public class IssuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StatisticService _stats;
        private readonly LibraryService _service;

        public IssuesController(ApplicationDbContext context, LibraryService service, StatisticService stats)
        {
            _context = context;
            _service = service;
            _stats = stats;
        }

        public async Task<IActionResult> Index(int? page, int? year)
        {
            var retrieveOnlyAvailableIssues = !User.IsAtLeast(StranitzaRoles.Editor);
            var viewModel = await _context.StranitzaIssues.GetIssuesPagedAsync(year, page,
                shouldBeAvailable: retrieveOnlyAvailableIssues);

            viewModel.YearFilter = _context.CountByYears.GetIssuesCountByYears(onlyAvailable: retrieveOnlyAvailableIssues);
            viewModel.CurrentYear = year;

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stranitzaIssue = await _context.StranitzaIssues.GetIssueDetailsAsync(id.Value);
            if (stranitzaIssue == null)
            {
                return NotFound();
            }

            if (!stranitzaIssue.IsAvailable)
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Challenge();
                }

                if (!User.IsAtLeast(StranitzaRoles.Editor))
                {
                    return Forbid();
                }
            }

#pragma warning disable 4014
            Task.Run(() =>
            {
                _stats.UpdateIssueViewCountAsync(stranitzaIssue.Id);
                Log.Logger.Information($"Потребител ({User.GetUserId()}:{User.GetUserName()}) " +
                    $"прегледа брой ({stranitzaIssue.Id}:{stranitzaIssue.ReleaseNumber}/{stranitzaIssue.ReleaseYear}-{stranitzaIssue.IssueNumber}).");
            });
#pragma warning restore 4014

            return View(stranitzaIssue);
        }

        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public IActionResult Create()
        {
            return View(new IssueCreateViewModel());
        }

        // POST: Issues/Create
        [HttpPost]
        [DisableRequestSizeLimit]
        [ValidateAntiForgeryToken]
        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Create(IssueCreateViewModel vModel)
        {
            if (!ModelState.IsValid)
            {
                return View(vModel);
            }

            try
            {
                var entry = await _service.SaveIssueRecord(vModel);

                return RedirectToAction(nameof(Edit), new {id = entry.Id});
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

            var vModel = await _context.StranitzaIssues.GetIssueEditAsync(id.Value);
            if (vModel == null)
            {
                return NotFound();
            }

            return View(vModel);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [ValidateAntiForgeryToken]
        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Edit(IssueEditViewModel vModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var entry = await _service.UpdateIssueRecord(vModel);

                    return RedirectToAction(nameof(Details), new {id = entry.Id});
                }
                catch (Exception ex)
                {
                    StranitzaDbErrorHandler.Instance.HandleError(ModelState, ex);
                }
            }

/*            // retrieve pages again
            vModel.Pages = _context.StranitzaPages
                .Where(x => x.IssueId == vModel.Id).AsEnumerable();*/

            return View(vModel);
        }

        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entry = await _context.StranitzaIssues.GetIssueEditAsync(id.Value);
            if (entry == null)
            {
                return NotFound();
            }

            return View(entry);
        }

        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entry = await _context.StranitzaIssues.FindAsync(id);

            var files = _context.StranitzaPages
                .Where(x => x.IssueId == entry.Id).Select(x => x.PageFile).ToList();

            _context.StranitzaIssues.Remove(entry);

            await _context.SaveChangesAsync();

            await _service.DeleteIssueFilesAndRecords(entry, files);

            return RedirectToAction(nameof(Index));
        }

        [StranitzaAuthorize]
        public async Task<IActionResult> PreviewPdf(int? id, bool thumb = false)
        {
            var issueEntry = await _context.StranitzaIssues.FindAsync(id);
            if (issueEntry == null)
            {
                return View("IssueNotFound");
            }

            if (!issueEntry.HasPdf)
            {
                return NotFound();
            }

#pragma warning disable 4014
            Task.Run(() =>
            {
                _stats.UpdateIssueViewCountAsync(issueEntry.Id);

                Log.Logger.Information($"Потребител ({User.GetUserId()}:{User.GetUserName()}) " +
                    $"прегледа брой ({issueEntry.Id}:{issueEntry.ReleaseNumber}/{issueEntry.ReleaseYear}-{issueEntry.IssueNumber}).");
            });
#pragma warning restore 4014

            var pdfEntry = await _context.StranitzaFiles.FindAsync(issueEntry.PdfFilePreviewId);
            var content = await _service.GetPreviewPdfForUser(User, pdfEntry, thumb);

            return new FileContentResult(content, pdfEntry.MimeType);
        }

        [StranitzaAuthorize]
        public async Task<IActionResult> DownloadPdf(int? id, bool reduced = false)
        {
            var issueEntry = await _context.StranitzaIssues.FindAsync(id);
            if (issueEntry == null)
            {
                return View("IssueNotFound");
            }

            if (!issueEntry.HasPdf)
            {
                return NotFound();
            }

            _stats.UpdateIssueDownloadCountAsync(issueEntry.Id);

            Log.Logger.Information($"Потребител ({User.GetUserId()}:{User.GetUserName()}) " +
                $"свали брой ({issueEntry.Id}:{issueEntry.ReleaseNumber}/{issueEntry.ReleaseYear}-{issueEntry.IssueNumber}).");

            return await _service.GetDownloadPdfForUser(User, issueEntry, reduced);
        }

        [AllowAnonymous]
        public async Task<IActionResult> DownloadZip(int? id, bool thumb = false)
        {
            var issue = await _context.StranitzaIssues.FindAsync(id);
            if (issue == null)
            {
                return View("IssueNotFound");
            }

            _stats.UpdateIssueDownloadCountAsync(issue.Id);

            Log.Logger.Information($"Потребител ({User.GetUserId()}:{User.GetUserName()}) " +
                $"свали брой ({issue.Id}:{issue.ReleaseNumber}/{issue.ReleaseYear}-{issue.IssueNumber}).");

            var content = await _service.GetZipForUser(User, issue, thumb);
            return new FileContentResult(content, issue.ZipFile.MimeType)
            {
                FileDownloadName = issue.ZipFile.FileName
            };
        }

        public async Task<IActionResult> Search(string q, int? page)
        {
            var vModel = await _context.StranitzaIssues.SearchIssuesPagedAsync(q, page);

            vModel.SearchQuery = q;

            return View(vModel);
        }
    }
}
