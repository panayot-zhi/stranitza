using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using stranitza.Models.Database;
using stranitza.Utility;

namespace stranitza.Areas.Identity.Pages.Account.Manage
{
    public partial class AvatarModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AvatarModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IMailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public string DefaultAvatarPath { get; set; }

        public string GravatarAvatarPath { get; set; }

        public string FacebookAvatarPath { get; set; }

        public string TwitterAvatarPath { get; set; }

        public string GoogleAvatarPath { get; set; }

        public string InternalAvatarPath { get; set; }

        public bool HasExternalAvatar => !string.IsNullOrEmpty(FacebookAvatarPath) ||
                                         !string.IsNullOrEmpty(TwitterAvatarPath) ||
                                         !string.IsNullOrEmpty(GoogleAvatarPath);

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Нова снимка")]
            public IFormFile AvatarFile { get; set; }

            [Display(Name = "Текуща снимка")]
            public StranitzaAvatarType AvatarType { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Няма потребител с този идентификационен номер '{_userManager.GetUserId(User)}'.");
            }

            Input = new InputModel
            {
                AvatarType = user.AvatarType,
            };

            GatherPaths(user);
            return Page();
        }

        private void GatherPaths(ApplicationUser user)
        {
            DefaultAvatarPath = Url.GetAvatarPath(user, StranitzaAvatarType.Default);

            GravatarAvatarPath = Url.GetAvatarPath(user, StranitzaAvatarType.Gravatar);

            FacebookAvatarPath = Url.GetAvatarPath(user, StranitzaAvatarType.Facebook);
            TwitterAvatarPath = Url.GetAvatarPath(user, StranitzaAvatarType.Twitter);
            GoogleAvatarPath = Url.GetAvatarPath(user, StranitzaAvatarType.Google);

            InternalAvatarPath = Url.GetAvatarPath(user, StranitzaAvatarType.Internal);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Няма потребител с този идентификационен номер '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                GatherPaths(user);
                return Page();
            }

            user.AvatarType = Input.AvatarType;
            if (Input.AvatarType == StranitzaAvatarType.Internal)
            {
                if (Input.AvatarFile == null)
                {
                    ModelState.AddModelError("Input.AvatarFile",
                        "Моля, прикачете изображение за снимка.");
                    GatherPaths(user);
                    return Page();
                }

                if (Input.AvatarFile.Length > StranitzaConstants.AvatarMaxSize)
                {
                    ModelState.AddModelError("Input.AvatarFile",
                        "Снимката, която сте прикачили, надвишава позволеният размер (от 160Kb).");
                    GatherPaths(user);
                    return Page();
                }

                if (!string.IsNullOrEmpty(user.InternalAvatarPath))
                {
                    DeleteAvatarFile(user.InternalAvatarPath, user.Id);
                }

                user.InternalAvatarPath = await SaveAvatarFileAsync(Input.AvatarFile, user.Id);
            }
            else
            {
                DeleteAvatarFile(user.InternalAvatarPath, user.Id);
                user.InternalAvatarPath = null;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                if (result.Errors.Any())
                {
                    ModelState.AddIdentityErrors(result.Errors);
                    GatherPaths(user);
                    return Page();
                }

                var userId = await _userManager.GetUserIdAsync(user);
                throw new StranitzaException($"Unexpected error occurred setting names for user with ID '{userId}'.");
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData.AddModalMessage("Информацията за профила беше обновена.", "success");
            return RedirectToPage();
        }

        private async Task<string> SaveAvatarFileAsync(IFormFile formFile, string userId)
        {
            var rootFolderPath = Path.Combine(_configuration["RepositoryPath"], StranitzaConstants.UploadsFolderName);
            var fileName = userId; //StranitzaExtensions.Md5Hash(formFile.FileName + "-" + DateTime.Now).ToLowerInvariant();
            var fileExtension = formFile.FileName.Split(".", StringSplitOptions.RemoveEmptyEntries).Last();
            var filePath = Path.Combine(rootFolderPath, fileName + "." + fileExtension);

            // overwrite file if it exists
            using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                await formFile.CopyToAsync(fileStream);
            }

            return $"~/{StranitzaConstants.UploadsFolderName}/{fileName}.{fileExtension}";
        }

        private void DeleteAvatarFile(string internalFilePath, string userId)
        {
            if (string.IsNullOrEmpty(internalFilePath))
            {
                return;
            }

            var fileName = $"{userId}.{StranitzaExtensions.GetFileExtension(internalFilePath)}";
            var rootPath = Path.Combine(_configuration["RepositoryPath"], StranitzaConstants.UploadsFolderName);
            var filePath = Path.Combine(rootPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
