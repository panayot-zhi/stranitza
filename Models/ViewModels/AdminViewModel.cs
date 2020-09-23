using System.Collections.Generic;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class AdminViewModel : PagedViewModel
    {
        public IEnumerable<UserDetailsViewModel> Records { get; set; }

        public UserFilterViewModel Filter { get; set; }

        public AdminViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {
        }
    }
}
