using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using stranitza.Models.Database;
using stranitza.Models.Database.Repositories;
using stranitza.Models.ViewModels;
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

        // GET: Issues
        public async Task<IActionResult> Index(int? page, int? year)
        {
            var retrieveOnlyAvailableIssues = !User.IsAtLeast(StranitzaRoles.Editor);
            var viewModel = await _context.StranitzaIssues.GetIssuesPagedAsync(year, page,
                shouldBeAvailable: retrieveOnlyAvailableIssues);

            viewModel.YearFilter =
                _context.StranitzaIssues.GetYearFilterViewModels(shouldBeAvailable: retrieveOnlyAvailableIssues);
            viewModel.CurrentYear = year;

            return View(viewModel);
        }

        // GET: Issues/Details/5
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
            Task.Run(() => _stats.UpdateIssueViewCountAsync(stranitzaIssue.Id));
#pragma warning restore 4014

            return View(stranitzaIssue);
        }

        // GET: Issues/Create
        public IActionResult Create()
        {
            return View(new IssueCreateViewModel());
        }

        // POST: Issues/Create
        [HttpPost]
        [DisableRequestSizeLimit]
        [ValidateAntiForgeryToken]
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

        // GET: Issues/Edit/5
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

        // POST: Issues/Edit/5
        [HttpPost]
        [DisableRequestSizeLimit]
        [ValidateAntiForgeryToken]
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

        // GET: Issues/Delete/5
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

        // POST: Issues/Delete/5
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
            Task.Run(() => _stats.UpdateIssueViewCountAsync(issueEntry.Id));
#pragma warning restore 4014

            var pdfEntry = await _context.StranitzaFiles.FindAsync(issueEntry.PdfFilePreviewId);
            var content = await _service.GetPreviewPdfForUser(User, pdfEntry, thumb);

            return new FileContentResult(content, pdfEntry.MimeType);
        }

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

            return await _service.GetDownloadPdfForUser(User, issueEntry, reduced);
        }

        public async Task<IActionResult> DownloadZip(int? id, bool thumb = false)
        {
            var issue = await _context.StranitzaIssues.FindAsync(id);
            if (issue == null)
            {
                return View("IssueNotFound");
            }

            _stats.UpdateIssueDownloadCountAsync(issue.Id);

            var content = await _service.GetZipForUser(User, issue, thumb);
            return new FileContentResult(content, issue.ZipFile.MimeType)
            {
                FileDownloadName = issue.ZipFile.FileName
            };
        }
    }
}
