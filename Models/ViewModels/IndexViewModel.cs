using System.Collections.Generic;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class IndexViewModel : PagedViewModel
    {
        public IEnumerable<SourceIndexViewModel> Records { get; set; }

        public IEnumerable<FilterCategoryViewModel> CategoriesFilter { get; set; }

        public int? CurrentCategoryId { get; set; }

        public IEnumerable<FilterYearViewModel> YearFilter { get; set; }

        public int? CurrentYear { get; set; }

        public IEnumerable<string> OriginFilter { get; set; }

        public string CurrentOrigin { get; set; }

        public IndexViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {
            
        }
    }
}
