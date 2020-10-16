using System.Collections.Generic;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class SourceSearchViewModel : PagedViewModel
    {
        public string SearchQuery { get; set; }

        public IEnumerable<SourceIndexViewModel> Records { get; set; }

        public SourceSearchViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {
        }
    }
}
