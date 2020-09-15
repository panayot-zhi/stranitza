using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Utility;

namespace stranitza.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IMailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IMailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;            
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public LoginProvidersViewModel LoginProviders { get; set; }

        public string ReturnUrl { get; set; }

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
            public string UserName { get; set; }

            [Display(Name = "Email")]
            [Required(ErrorMessage = "Моля, въведете email адрес.")]
            [EmailAddress(ErrorMessage = "Моля, въведете валиден email адрес.")]
            [MaxLength(255, ErrorMessage = "Въведеният email надвишава позволеният размер ({1}).")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Моля, въведете парола.")]
            [StringLength(100, ErrorMessage = "Полето за {0} трябва да е с поне {2} и най-много {1} символа дължина.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Парола")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Потвърди")]
            [Compare("Password", ErrorMessage = "Двете пароли не съвпадат.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "За да се регистрирате се изисква да сте прочели и да приемате общите условия на сайта и политиката му за поверителност.")]
            [Display(Name = "Прочетох и приемам Политиката за поверителност на сайта и Общите му условия")]            
            public bool PrivacyConsent { get; set; }
        }

        public async Task<IActionResult> OnGet(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(returnUrl);
            }

            ReturnUrl = returnUrl;

            LoginProviders = new LoginProvidersViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (Input.PrivacyConsent != true)
            {
                ModelState.AddModelError(string.Empty, "За да се регистрирате се изисква да сте прочели и да приемате общите условия на сайта и политиката му за поверителност.");
            }

            if (!ModelState.IsValid)
            {
                return await OnGet(returnUrl);
            }

            var user = new ApplicationUser
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                UserName = Input.UserName,
                Email = Input.Email
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendMailAsync(Input.Email, "Потвърждение на профил", "ConfirmEmail", new { Names = user.Names, ButtonLink = callbackUrl });

                // NOTE: Roles are given only for special privileges,
                // every registered user can be checked by calling .IsAuthenticated
                //await _userManager.AddToRoleAsync(user, StranitzaRolesHelper.UserRoleName);

                await _signInManager.SignInAsync(user, isPersistent: false);

                return LocalRedirect(returnUrl);
            }

            ModelState.AddIdentityErrors(result.Errors);

            // if we got this far, 
            // something failed, 
            // redisplay form
            return await OnGet(returnUrl);
        }
    }
}
