using System.Collections.Generic;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class CategorySourcesViewModel : PagedViewModel
    {
        public IEnumerable<SourceIndexViewModel> Records { get; set; }

        public CategoryViewModel CurrentCategory { get; set; }                

        public CategorySourcesViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {

        }
    }
}
