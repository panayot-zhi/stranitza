using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using stranitza.Models.Database;
using stranitza.Utility;

namespace stranitza.Areas.Identity.Pages.Account.Manage
{
    public partial class InfoModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IMailSender _emailSender;

        public InfoModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IMailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string CurrentDisplayName { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Какво да се показва за Вас")]
            public StranitzaDisplayNameType DisplayNameType { get; set; }

            [Display(Name = "Телефонен номер")]
            [Phone(ErrorMessage = "Моля, въведете валиден телефонен номер.")]
            [MaxLength(10, ErrorMessage = "Въведеният телефонен номер надвишава позволеният размер ({1} цифри).")]
            public string PhoneNumber { get; set; }
            
            [Display(Name = "Кратко описание")]
            [MaxLength(1024, ErrorMessage = "Въведеният телефонен номер надвишава позволеният размер ({1} символа).")]
            public string Description { get; set; }

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
                DisplayNameType = user.DisplayNameType,
                Description = user.Description,
                PhoneNumber = user.PhoneNumber,
            };

            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;

            CurrentDisplayName = StranitzaExtensions.GetDisplayName(user);

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Няма потребител с този идентификационен номер '{_userManager.GetUserId(User)}'.");
            }

            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;

            CurrentDisplayName = StranitzaExtensions.GetDisplayName(user);

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            IdentityResult result;

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                result = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!result.Succeeded)
                {
                    if (result.Errors.Any())
                    {
                        ModelState.AddIdentityErrors(result.Errors);
                        return Page();
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
                }
            }

            user.DisplayNameType = Input.DisplayNameType;
            user.Description = Input.Description;

            result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                if (result.Errors.Any())
                {
                    ModelState.AddIdentityErrors(result.Errors);
                    return Page();
                }

                var userId = await _userManager.GetUserIdAsync(user);
                throw new StranitzaException($"Unexpected error occurred setting names for user with ID '{userId}'.");
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData.AddModalMessage("Допълнителната информация беше обновена.", "success");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Няма потребител с този идентификационен номер '{_userManager.GetUserId(User)}'.");
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendMailAsync(email, "Потвърждение на профил", "ReConfirmEmail", new { Names = user.Names, ButtonLink = callbackUrl });
            TempData.AddModalMessage("Беше Ви изпратен верификационен email на адресът, който сте посочили. Моля, последвайте линка от писмото за да верифицирате новата си електронна поща.", "info");
            return RedirectToPage();
        }

        private async Task<string> SaveAvatarFileAsync(IFormFile formFile, string userId)
        {
            var rootFolderPath = Path.Combine(_configuration["RepositoryPath"], StranitzaConstants.UploadsFolderName);
            var fileName = userId; //StranitzaExtensions.Md5Hash(formFile.FileName + "-" + DateTime.Now).ToLowerInvariant();
            var fileExtension = formFile.FileName.Split(".", StringSplitOptions.RemoveEmptyEntries).Last();
            var filePath = Path.Combine(rootFolderPath, fileName + "." + fileExtension);

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
