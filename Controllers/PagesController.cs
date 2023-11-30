using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Repositories;
using stranitza.Services;
using stranitza.Utility;

namespace stranitza.Controllers
{
    public class PagesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly LibraryService _service;

        public PagesController(ApplicationDbContext context, LibraryService service, 
            IWebHostEnvironment environment, 
            IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
            _context = context;
            _service = service;
        }

        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Index(int? id, int? page)
        {
            if (!id.HasValue)
            {
                return RedirectToActionPreserveMethod("IssueNotFound", "Home");
            }
            
            var viewModel = await _context.StranitzaIssues
                .GetIssuePagesPagedAsync(id.Value, page);

            return View(viewModel);
        }

        [Ajax]
        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entry = await _context.StranitzaPages
                .GetPageDetailsAsync(id.Value);

            if (entry == null)
            {
                return NotFound();
            }

            return PartialView("_Details", entry);
        }

        [Ajax]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Edit(PageViewModel vModel)
        {
            var r = new StranitzaJsonResult();

            if (ModelState.IsValid)
            {
                try
                {
                    // update page, updates the viewModel with file info
                    var page = await _context.StranitzaPages.UpdatePageAsync(vModel);

                    // fail if indexes fail
                    await _context.SaveChangesAsync();

                    // pass to update viewModel
                    _service.UpdatePageFile(vModel);

                    // update file entry
                    await _context.StranitzaFiles.UpdateFileAsync(vModel);

                    // save
                    await _context.SaveChangesAsync();

                    r.success = true;
                    return Json(r);
                }
                catch (Exception ex)
                {
                    var knownError = StranitzaDbErrorHandler.GetKnownError(ex);
                    if (knownError == KnownErrors.ER_DUP_ENTRY)
                    {
                        ModelState.AddModelError("PageNumber", "Страница със същия номер вече съществува.");                        
                    }
                    else
                    {
                        StranitzaDbErrorHandler.Instance.HandleError(ModelState, ex);
                    }                    
                }
            }

            r.errors = ModelState.GatherErrors();
            return Json(r);

        }
        
        [Ajax]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var r = new StranitzaJsonResult();

            try
            {
                await _service.DeletePageFileAndRecords(id.Value);

                r.success = true;
            }
            catch (Exception ex)
            {
                StranitzaDbErrorHandler.Instance.HandleError(ModelState, ex);
            }

            return Json(r);
        }

        [ResponseCache(CacheProfileName = StranitzaCacheProfile.Weekly, VaryByQueryKeys = new [] { "thumb" })]
        public IActionResult Load(int id, bool thumb = false)
        {
            var page = _context.StranitzaPages
                .Include(x => x.PageFile).SingleOrDefault(x => x.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            if (!page.IsAvailable)
            {
                if (!User.IsAtLeast(StranitzaRoles.Editor))
                {
                    return Forbid();
                }
            }

            var file = page.PageFile;
            var result = file.FilePath;
            if (thumb && !string.IsNullOrEmpty(file.ThumbPath))
            {
                result = file.ThumbPath;
            }

            if (_environment.IsDevelopment())
            {
                var localRepositoryPath = _configuration["RepositoryPath"];
                var productionRepositoryPath = _configuration["ProductionRepositoryPath"];
                result = result.Replace(productionRepositoryPath, localRepositoryPath);
            }

            if (!System.IO.File.Exists(result))
            {
                return NotFound($"{file.FileName} could not be found!");
            }

            return new PhysicalFileResult(result, file.MimeType)
            {
                FileDownloadName = file.FileName
            };
        }

    }
}
