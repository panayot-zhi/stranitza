using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using stranitza.Services;
using stranitza.Utility;

namespace stranitza.Areas.Identity.Pages.Account.Manage
{
    public class AdminModel : PageModel
    {
        private readonly LibraryService _library;
        private readonly IConfiguration _configuration;

        public AdminModel(LibraryService library, IConfiguration configuration)
        {
            _library = library;
            _configuration = configuration;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostCreateIssueFromFolderAsync(int releaseNumber, int releaseYear)
        {
            if (!User.IsAtLeast(StranitzaRoles.Administrator))
            {
                TempData.AddModalMessage($"Функционалността е достъпна само за администратори.", "warning");
                return OnGet();
            }

            var rootFolderPath = Path.Combine(_configuration["RepositoryPath"], StranitzaConstants.IssuesFolderName);
            var directoryPath = Path.Combine(rootFolderPath, releaseYear.ToString(), $"{releaseYear}-{releaseNumber}");
            if (!Directory.Exists(directoryPath))
            {
                TempData.AddModalMessage($"Не е намерена директория '{directoryPath}'.", "warning");
                return OnGet();
            }

            var directoryInfo = new DirectoryInfo(directoryPath);
            var issue = await _library.CreateIssueRecord(directoryInfo);
            if (issue == null)
            {
                TempData.AddModalMessage($"Вече съществува брой {releaseNumber} за {releaseYear} г.", "warning");
                return OnGet();
            }

            return RedirectToAction("Edit","Issues", new { id = issue.Id });
        }
    }
}
