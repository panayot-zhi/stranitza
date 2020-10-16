using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using stranitza.Models.Database;

namespace stranitza.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;

        public DeletePersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Password)]
            [Display(Name = "Текуща парола")]
            [Required(ErrorMessage = "Моля, въведете текущата Ви парола за да продължите.")]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public int AuthoredEPagesCount { get; set; }

        public int UploadedEPagesCount { get; set; }

        public int SourcesCount { get; set; }

        public int CommentsCount { get; set; }

        public int ModeratedCommentsCount { get; set; }

        public int PostsCount { get; set; }

        public bool HasConnectedResources =>
            AuthoredEPagesCount > 0 ||
            UploadedEPagesCount > 0 ||
            SourcesCount > 0 ||
            CommentsCount > 0 ||
            ModeratedCommentsCount > 0 ||
            PostsCount > 0;

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Няма потребител с този идентификационен номер '{_userManager.GetUserId(User)}'.");
            }

            GetConnectedResourcesCount(user);
            RequirePassword = await _userManager.HasPasswordAsync(user);

            return Page();
        }

        private void GetConnectedResourcesCount(ApplicationUser user)
        {
            AuthoredEPagesCount = _dbContext.StranitzaEPages.Count(x => x.AuthorId == user.Id);
            UploadedEPagesCount = _dbContext.StranitzaEPages.Count(x => x.UploaderId == user.Id);
            SourcesCount = _dbContext.StranitzaSources.Count(x => x.AuthorId == user.Id);
            CommentsCount = _dbContext.StranitzaComments.Count(x => x.AuthorId == user.Id);
            ModeratedCommentsCount = _dbContext.StranitzaComments.Count(x => x.ModeratorId == user.Id);
            PostsCount = _dbContext.StranitzaPosts.Count(x => x.UploaderId == user.Id);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Няма потребител с този идентификационен номер '{_userManager.GetUserId(User)}'.");
            }

            GetConnectedResourcesCount(user);

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Грешна парола.");
                    return Page();
                }
            }

            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
            }

            await _signInManager.SignOutAsync();

            Log.Logger.Information("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}