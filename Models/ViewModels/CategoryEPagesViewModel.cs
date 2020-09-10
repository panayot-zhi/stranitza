using System.Collections.Generic;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class CategoryEPagesViewModel : PagedViewModel
    {
        public IEnumerable<EPageIndexViewModel> Records { get; set; }        

        public CategoryViewModel CurrentCategory { get; set; }        

        public CategoryEPagesViewModel(int totalRecords, int pageIndex, int pageSize) : base(totalRecords, pageIndex, pageSize)
        {
        }
    }
}
