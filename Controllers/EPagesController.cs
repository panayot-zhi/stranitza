using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Repositories;
using stranitza.Services;
using stranitza.Utility;

namespace stranitza.Controllers
{
    public class EPagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ELibraryService _service;

        public EPagesController(ApplicationDbContext context, ELibraryService service)
        {
            _context = context;
            _service = service;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? year)
        {
            var viewModel = await _context.StranitzaEPages.GetEPagesByYearAsync(year);
            viewModel.YearFilter = _context.CountByYears.GetEPagesCountByYears();

            return View(viewModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entry = await _context.StranitzaEPages.GetEPageForDetailsAsync(id.Value);
            if (entry == null)
            {
                return NotFound();
            }

            return View(entry);
        }

        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public IActionResult Create()
        {                        
            return View(new EPageCreateViewModel()
            {
                Categories = new SelectList(_context.StranitzaCategories, "Id", "Name")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Create(EPageCreateViewModel vModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = User.GetUserId();
                    var currentUserName = User.GetUserName();

                    var entry = await _service.CreateEPageAsync(vModel, currentUserId, currentUserName);

                    return RedirectToAction(nameof(Details), new { id = entry.Id });
                }
                catch (Exception ex)
                {
                    StranitzaDbErrorHandler.Instance.HandleError(ModelState, ex);
                }
            }
            
            vModel.Categories = new SelectList(_context.StranitzaCategories, "Id", "Name", vModel.CategoryId);

            return View(vModel);
        }

        [HttpGet]
        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entry = await _context.StranitzaEPages.GetEPageForDeleteAsync(id.Value);
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
            var epage = await _context.StranitzaEPages.FindAsync(id);

            _context.StranitzaEPages.Remove(epage);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Search(string q, int? page)
        {
            var vModel = await _context.StranitzaEPages.SearchEPagesPagedAsync(q, page);

            vModel.SearchQuery = q;

            return View(vModel);
        }
    }
}
