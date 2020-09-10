using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Repositories;
using stranitza.Utility;

namespace stranitza.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string sort, string order)
        {
            Enum.TryParse(order, ignoreCase: true, result: out SortOrder sortOrder);
            var vModel = await _context.StranitzaCategories
                .GetCategoryIndexViewModelAsync(sort, sortOrder);
            return View(vModel);
        }

        // GET: Categories/EPages/5        
        public async Task<IActionResult> EPages(int id, int? page, string sort, string order)
        {
            var entry = await _context.StranitzaCategories.FindAsync(id);

            if (entry == null)
            {
                return NotFound();
            }

            Enum.TryParse(order, ignoreCase: true, result: out SortOrder sortOrder);
            var vModel = await _context.StranitzaEPages.GetEPagesByCategoryAsync(
                categoryId: entry.Id, pageIndex: page, sortPropertyName: sort, sortOrder: sortOrder);

            vModel.CurrentCategory = new CategoryViewModel()
            {
                Id = entry.Id,
                Name = entry.Name,
                Description = entry.Description
            };

            return View(vModel);
        }

        // GET: Categories/Sources/5        
        public async Task<IActionResult> Sources(int id, int? page, string sort, string order)
        {
            var entry = await _context.StranitzaCategories.FindAsync(id);

            if (entry == null)
            {
                return NotFound();
            }

            Enum.TryParse(order, ignoreCase: true, result: out SortOrder sortOrder);
            var vModel = await _context.StranitzaSources.GetCategorySourcesPagedAsync(
                categoryId: entry.Id, pageIndex: page, sortPropertyName: sort, sortOrder: sortOrder);

            vModel.CurrentCategory = new CategoryViewModel()
            {
                Id = entry.Id,
                Name = entry.Name,
                Description = entry.Description
            };

            return View(vModel);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vModel = await _context.StranitzaCategories
                .GetCategoryViewModelAsync(id.Value);

            if (vModel == null)
            {
                return NotFound();
            }

            return View(vModel);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        // POST: Categories/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel vModel)
        {
            if (ModelState.IsValid)
            {
                var entry = await _context.StranitzaCategories.CreateCategoryAsync(vModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = entry.Id });
            }

            return View(vModel);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vModel = await _context.StranitzaCategories
                .GetCategoryViewModelAsync(id.Value);

            if (vModel == null)
            {
                return NotFound();
            }

            return View(vModel);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel vModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.StranitzaCategories.UpdateCategoryAsync(vModel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    StranitzaDbErrorHandler.Instance.HandleError(ModelState, ex);
                }                
            }

            return View(vModel);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vModel = await _context.StranitzaCategories.GetCategoryViewModelAsync(id.Value);
            if (vModel == null)
            {
                return NotFound();
            }

            return View(vModel);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entry = await _context.StranitzaCategories.FindAsync(id);
            _context.StranitzaCategories.Remove(entry);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
