using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace stranitza.Models.ViewModels
{
    public class LoginProvidersViewModel
    {
        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}
