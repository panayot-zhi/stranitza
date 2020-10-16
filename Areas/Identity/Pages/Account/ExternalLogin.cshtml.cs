using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using stranitza.Models.Database;
using stranitza.Repositories;
using stranitza.Utility;

namespace stranitza.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Display(Name = "Име")]
            [Required(ErrorMessage = "Моля, въведете име.")]
            [RegularExpression(StranitzaConstants.CyrillicNamePattern, ErrorMessage = "Моля, въведете име на кирилица.")]
            public string FirstName { get; set; }

            [Display(Name = "Фамилия")]
            [Required(ErrorMessage = "Моля, въведете фамилия.")]
            [RegularExpression(StranitzaConstants.CyrillicNamePattern, ErrorMessage = "Моля, въведете име на кирилица.")]
            public string LastName { get; set; }

            [Display(Name = "Псевдоним")]
            [Required(ErrorMessage = "Моля, въведете псевдоним.")]
            public string UserName { get; set; }

            [EmailAddress]
            [Display(Name = "Email")]
            [Required(ErrorMessage = "Моля, въведете email адрес.")]
            public string Email { get; set; }

            // [Required(ErrorMessage = "Моля, въведете парола.")]
            // [StringLength(100, ErrorMessage = "Полето за {0} трябва да е с поне {2} и най-много {1} символа дължина.", MinimumLength = 6)]
            // [DataType(DataType.Password)]
            // [Display(Name = "Парола")]
            // public string Password { get; set; }
            //
            // [DataType(DataType.Password)]
            // [Display(Name = "Потвърди")]
            // [Compare("Password", ErrorMessage = "Двете пароли не съвпадат.")]
            // public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "За да се регистрирате се изисква да сте прочели и да приемате общите условия на сайта и политиката му за поверителност.")]
            [Display(Name = "Прочетох и приемам Политиката за поверителност на сайта и Общите му условия")]
            public bool PrivacyConsent { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Възникна грешка от {LoginProvider}: {remoteError}";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = $"Възникна грешка при зареждане на данните от {LoginProvider}.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            // NOTE: Successful social logins should be equivalent to hitting remember me on login, persist them
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                // Update avatar path on each login
                //await _userManager.UpdateUserAvatarPathAsync(info);

                Log.Logger.Information("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);

                return RedirectToPage("./Redirector", new { redirectUrl = returnUrl });
            }

            if (result.IsNotAllowed)
            {
                ErrorMessage = "Променили сте Вашия email адрес. Изпратихме Ви писмо, което съдържа връзка за потвърждение на новият Ви email. " +
                               "Моля, преди да влезете в сайта, потвърдете електронна си поща.";

                return RedirectToPage("./Login");
            }

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account,
                // then ask the user to create an account.

                ReturnUrl = returnUrl;
                Input = new InputModel();
                LoginProvider = info.LoginProvider;

                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    Input.UserName = info.Principal.FindFirstValue(ClaimTypes.Name);
                    Input.FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                    Input.LastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
                }

                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                //var profilePicture = info.Principal.GetProfilePicture();
                var verifiedEmail = info.Principal.GetVerifiedEmail();

                var user = new ApplicationUser
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    UserName = Input.UserName,

                    Email = Input.Email,

                    // The presumption is, that a social
                    // login has the email confirmed
                    EmailConfirmed = verifiedEmail ?? true,

                    AvatarType = StranitzaExtensions.GetAvatarType(info.LoginProvider)
                };

                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        await _userManager.UpdateUserAvatarPathAsync(info);

                        Log.Logger.Information("User created an account using {Name} provider.", info.LoginProvider);

                        return RedirectToPage("./Redirector", new { redirectUrl = returnUrl });
                    }
                }

                ModelState.AddIdentityErrors(result.Errors);
            }

            LoginProvider = info.LoginProvider;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
