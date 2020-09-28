using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using stranitza.Models.Database;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class UserFilterViewModel
    {
        public string Name { get; set; }

        public virtual string UserName { get; set; }

        public virtual string Email { get; set; }

        public string Description { get; set; }

        public UserFilterType Type { get; set; }

        public override string ToString()
        {
            var result = "";
            if (!string.IsNullOrEmpty(Name))
            {
                result += $"име като '{Name}', ";
            }

            if (!string.IsNullOrEmpty(UserName))
            {
                result += $"псевдоним '{UserName}', ";
            }

            if (!string.IsNullOrEmpty(Email))
            {
                result += $"email, или част от него е '{Email}', ";
            }

            if (!string.IsNullOrEmpty(Description))
            {
                result += $"съдържа описание '{Description}', ";
            }

            var lastCommaIndex = result.LastIndexOf(", ", StringComparison.CurrentCulture);
            if (lastCommaIndex > -1)
            {
                result = result.Remove(lastCommaIndex);
            }
            else
            {
                result = "няма";
            }

            return result;
        }
    }
}
