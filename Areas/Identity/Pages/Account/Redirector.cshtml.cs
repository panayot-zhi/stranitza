using Microsoft.AspNetCore.Mvc.RazorPages;

namespace stranitza.Areas.Identity.Pages.Account
{
    public class RedirectorModel : PageModel
    {
        public string RedirectUrl { get; set; }

        public void OnGet(string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                redirectUrl = Url.Content("~/");
            }

            RedirectUrl = redirectUrl;
        }
    }
}