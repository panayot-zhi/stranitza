using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using stranitza.Models.Database;
using stranitza.Utility;

namespace stranitza.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMailSender _emailSender;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public bool IsEmailConfirmed { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Име")]
            [Required(ErrorMessage = "Моля, въведете име.")]
            [MaxLength(255, ErrorMessage = "Въведеното име надвишава позволеният размер ({1}).")]
            [RegularExpression(StranitzaConstants.CyrillicNamePattern, ErrorMessage = "Моля, въведете име на кирилица.")]
            public string FirstName { get; set; }

            [Display(Name = "Фамилия")]
            [Required(ErrorMessage = "Моля, въведете фамилия.")]
            [MaxLength(255, ErrorMessage = "Въведеното име надвишава позволеният размер ({1}).")]
            [RegularExpression(StranitzaConstants.CyrillicNamePattern, ErrorMessage = "Моля, въведете име на кирилица.")]
            public string LastName { get; set; }

            [Display(Name = "Псевдоним")]
            [MaxLength(126, ErrorMessage = "Въведеният псевдоним надвишава позволеният размер ({1}).")]
            [Required(ErrorMessage = "Моля, въведете псевдоним.")]
            public string Username { get; set; }

            [Display(Name = "Email")]
            [Required(ErrorMessage = "Моля, въведете email адрес.")]
            [EmailAddress(ErrorMessage = "Моля, въведете валиден email адрес.")]
            [MaxLength(255, ErrorMessage = "Въведеният email надвишава позволеният размер ({1}).")]
            public string Email { get; set; }

            [Display(Name= "Показвай email адресът ми")]
            public bool DisplayEmail { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Няма потребител с този идентификационен номер '{_userManager.GetUserId(User)}'.");
            }

            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            Input = new InputModel
            {
                Email = email,
                Username = userName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayEmail = user.DisplayEmail
            };

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

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            IdentityResult result;
            bool updateUser = false;            
            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
                updateUser = true;
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
                updateUser = true;
            }

            if (Input.DisplayEmail != user.DisplayEmail)
            {
                user.DisplayEmail = Input.DisplayEmail;
                updateUser = true;
            }

            if (updateUser)
            {
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
            }

            var username = await _userManager.GetUserNameAsync(user);
            if (Input.Username != username)
            {
                result = await _userManager.SetUserNameAsync(user, Input.Username);
                if (!result.Succeeded)
                {
                    if (result.Errors.Any())
                    {
                        ModelState.AddIdentityErrors(result.Errors);
                        return Page();
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting username for user with ID '{userId}'.");
                }
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.Email != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                result = await _userManager.SetEmailAsync(user, Input.Email);

                if (!result.Succeeded)
                {
                    if (result.Errors.Any())
                    {
                        ModelState.AddIdentityErrors(result.Errors);
                        return Page();
                    }

                    throw new InvalidOperationException($"Unexpected error occurred setting email for user with ID '{userId}'.");
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = userId, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendMailAsync(Input.Email, "Потвърждение на профил", "ReConfirmEmail", new { Names = user.Names, ButtonLink = callbackUrl });
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData.AddModalMessage("Информацията за профила Ви беше обновена.", "success");
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
    }
}
