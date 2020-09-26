using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using stranitza.Models;
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

        [StranitzaAuthorize(StranitzaRoles.HeadEditor)]
        public async Task<IActionResult> Index(int? page, 
            string email, string userName, string name, string description)
        {
            var vModel = await _context.Users.GetUsersPagedAsync(
                email: email, userName: userName, name: name, description: description, 
                pageIndex: page);

            vModel.Filter = new UserFilterViewModel()
            {
                Name = name,
                Email = email,
                UserName = userName,
                Description = description,
                //IsAuthor = isAuthor,
            };

            return View(vModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
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

                var entry = await _context.Users.UpdateUserAsync(vModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = entry.Id });

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
            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Search(UserFilterViewModel vModel, int? page)
        {
            throw new NotImplementedException();
        }
    }
}
