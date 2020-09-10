using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace stranitza.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public LoginProvidersViewModel LoginProviders { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Display(Name = "Псевдоним / Email")]
            [Required(ErrorMessage = "Моля, въведете псевдоним или email адрес.")]
            public string UserName { get; set; }

            [Display(Name = "Парола")]
            [DataType(DataType.Password)]
            [Required(ErrorMessage = "Моля, въведете парола.")]
            public string Password { get; set; }

            [Display(Name = "Запомни ме")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(returnUrl);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            LoginProviders = new LoginProvidersViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };
            
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginProviders = new LoginProvidersViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };

            if (!ModelState.IsValid)
            {
                // validation failed, 
                // redisplay form
                return Page();
            }

            SignInResult result;
            var user = await GetUserFromInputAsync(Input.UserName);
            if (user == null)
            {                
                result = SignInResult.Failed;
            }
            else
            {
                result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);
            }
                        
            if (result.Succeeded)
            {
                // ReSharper disable once PossibleNullReferenceException
                _logger.LogInformation($"User '{user.UserName}' logged in.");
                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Изпратихме Ви писмо, което съдържа връзка за потвърждение на email адресът Ви. " +
                                                       "Моля, преди да влезете в сайта, потвърдете електронна си поща.");
                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Невалиден опит за вход.");
                return Page();
            }            
        }

        private async Task<ApplicationUser> GetUserFromInputAsync(string input)
        {
            var userManager = _signInManager.UserManager;
            var byName = await userManager.FindByNameAsync(input);
            if (byName != null)
            {
                _logger.LogInformation($"User '{byName.UserName}' recognized by username.");
                return byName;
            }

            var byEmail = await userManager.FindByEmailAsync(input);
            if (byEmail != null)
            {
                _logger.LogInformation($"User {byEmail.UserName} recognized by email.");
                return byEmail;
            }

            _logger.LogWarning("User cannot be recognized neither by username nor by email.");

            return null;
        }
    }
}
