using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class EPageSearchViewModel : PagedViewModel
    {
        public string SearchQuery { get; set; }

        public IEnumerable<EPageIndexViewModel> Records { get; set; }

        public EPageSearchViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {
        }
    }
}
