using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using stranitza.Models.Database;
using stranitza.Models.ViewModels;
using stranitza.Utility;

namespace stranitza.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMailSender _emailSender;
        
        public ForgotPasswordModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IMailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public LoginProvidersViewModel LoginProviders { get; set; }

        public class InputModel
        {
            [Display(Name = "Email адрес")]
            [EmailAddress(ErrorMessage = "Моля въведете валиден email адрес.")]
            [Required(ErrorMessage = "Моля, въведете email адрес.")]
            public string Email { get; set; }
        }

        public async Task OnGetAsync()
        {
            LoginProviders = new LoginProvidersViewModel()
            {
                ReturnUrl = "~/",
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    Log.Logger.Warning("An attempt was made to recover a forgotten password for an unknown user: {Input}", Input);
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // TODO: This behaviour should be monitored and turned off if abused with

                var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                if (!isEmailConfirmed)
                {
                    Log.Logger.Information("An attempt was made to recover a forgotten password for user without a confirmed email: {Input}", Input);

                    var confirmEmailCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmEmailCallbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = confirmEmailCode },
                        protocol: Request.Scheme);

                    await _emailSender.SendMailAsync(Input.Email, "Потвърждение на профил", "ConfirmEmail", 
                        new { Names = user.Names, ButtonLink = confirmEmailCallbackUrl });

                    Log.Logger.Information("Email verification was resent.");

                    return RedirectToPage("./ResendVerificationEmail");
                }

                var resetPasswordCode = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordCallbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new {code = resetPasswordCode},
                    protocol: Request.Scheme);

                await _emailSender.SendMailAsync(Input.Email, "Възстановяване на парола", "ResetPassword", 
                    new { Ip = StranitzaExtensions.GetIp(HttpContext), ButtonLink = resetPasswordCallbackUrl });

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            LoginProviders = new LoginProvidersViewModel()
            {
                ReturnUrl = "~/",
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };

            return Page();
        }
    }
}
