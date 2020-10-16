using System;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class UserFilterViewModel
    {
        public string Name { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

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
