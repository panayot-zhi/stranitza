using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace stranitza.Utility
{
    public class AjaxAttribute : ActionMethodSelectorAttribute
    {
        public string HttpVerb { get; set; }

        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            return routeContext.HttpContext.Request.IsAjax(HttpVerb);
        }
    }

    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        private readonly string[] _sizes = { "B", "KB", "MB", "GB" };

        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {                    
                    return new ValidationResult(FormatErrorMessage(ErrorMessage));
                }
            }

            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            int order = 0;
            double length = _maxFileSize;
            while (length >= 1024 && ++order < _sizes.Length)
            {
                length = length / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = $"{length:0.##} {_sizes[order]}";
            return base.FormatErrorMessage(result);
        }
    }

    public class StranitzaAuthorizeAttribute : AuthorizeAttribute
    {        
        public StranitzaAuthorizeAttribute(StranitzaRoles role, bool andAbove = true)
        {
            var roles = StranitzaRolesHelper.GetRoleName(role);

            if (andAbove)
            {
                roles = string.Join(",", StranitzaRolesHelper.GetRoleNamesAbove(role));
            }

            this.Roles = roles;

        }
    }
}
