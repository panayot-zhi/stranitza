using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Repositories;
using stranitza.Utility;

namespace stranitza.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [StranitzaAuthorize(StranitzaRoles.Editor)]
        public async Task<IActionResult> Index(int? page, UserFilterType type,
            string email, string userName, string name, string description)
        {
            var filter = new UserFilterViewModel()
            {
                Name = name,
                Email = email,
                UserName = userName,
                Description = description,
                Type = type,
            };

            var vModel = await _userManager.GetUsersPagedAsync(filter: filter, pageIndex: page);

            vModel.Filter = filter;

            return View(vModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id, int? page, int? year, int? category)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated && !user.IsAuthor)
            {
                return Challenge();
            }

            var vModel = UserRepository.FromApplicationUser(user);

            vModel.Roles = await _userManager.GetRolesAsync(user);

            vModel.Comments = _context.StranitzaComments.Where(x => x.AuthorId == user.Id && x.ModeratorId == null).ToList();
            vModel.Sources = await _context.StranitzaSources.GetSourcesByAuthorPagedAsync(user.Id,year, category, page);
            vModel.Sources.CategoriesFilter = await _context.StranitzaCategories.GetCategoryFilterForAuthorViewModelAsync(user.Id);
            vModel.Sources.YearFilter = _context.CountByYears.GetSourcesCountByYearsAndAuthor(user.Id);
            vModel.Sources.CurrentCategoryId = category;
            vModel.Sources.CurrentYear = year;

            return View(vModel);
        }

        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var vModel = UserRepository.FromApplicationUser(user);
            vModel.Roles = await _userManager.GetRolesAsync(user);

            var firstRole = vModel.Roles.FirstOrDefault();
            vModel.Role = StranitzaRolesHelper.GetRole(firstRole);

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
        public async Task<IActionResult> Edit(UserDetailsViewModel vModel)
        {
            if (!ModelState.IsValid)
            {
                return View(vModel);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(vModel.Id);

                if (user == null)
                {
                    return NotFound();
                }

                user.IsAuthor = vModel.IsAuthor;
                user.EmailConfirmed = vModel.EmailConfirmed;
                user.PhoneNumber = vModel.PhoneNumber;
                user.PhoneNumberConfirmed = vModel.PhoneNumberConfirmed;
                user.LockoutEnd = vModel.LockoutEnd;

                await _userManager.UpdateAsync(user);
                await _userManager.UpdateRoleAsync(user, vModel.Role);

                return RedirectToAction(nameof(Details), new { id = user.Id });

            }
            catch (Exception ex)
            {
                StranitzaDbErrorHandler.Instance.HandleError(ModelState, ex);
            }

            return View(vModel);
        }

        [StranitzaAuthorize(StranitzaRoles.Administrator, andAbove: false)]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var vModel = UserRepository.FromApplicationUser(user);

            return View(vModel);
        }

        [StranitzaAuthorize(StranitzaRoles.Administrator, andAbove: false)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.FindAsync(id);
            var userId = await _userManager.GetUserIdAsync(user);
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Search(string q)
        {
            return RedirectToAction("Index", new { userName = q });
        }
    }
}
