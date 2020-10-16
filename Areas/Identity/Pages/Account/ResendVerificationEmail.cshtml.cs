using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace stranitza.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendVerificationEmail : PageModel
    {
        public void OnGet()
        {
        }
    }
}
